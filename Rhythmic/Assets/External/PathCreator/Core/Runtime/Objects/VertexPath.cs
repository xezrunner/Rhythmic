#define XZ_OPTIMIZE_TRANS

using System.Collections.Generic;
using PathCreation.Utility;
using UnityEngine;

namespace PathCreation
{
    public class VertexPath
    {
        public Transform trans;
        public Vector3 trans_pos;

        #region Fields

        public readonly PathSpace space;
        public readonly bool isClosedLoop;
        public readonly Vector3[] localPoints;
        public readonly Vector3[] localTangents;
        public readonly Vector3[] localNormals;
        public readonly float[] funky_angles;
        public float funky_angle_global;
        public float funky_angle_global_offset;
        public float funky_angle_global_mult;

        public readonly float[] times; /// Percentage along the path at each vertex (0 being start of path, and 1 being the end)
        public readonly float length; /// Total distance between the vertices of the polyline
        public readonly float[] cumulativeLengthAtEachVertex; /// Total distance from the first vertex up to each vertex in the polyline
        public readonly Bounds bounds; /// Bounding box of the path
        public readonly Vector3 up; /// Equal to (0,0,-1) for 2D paths, and (0,1,0) for XZ paths

        // Default values and constants:    
        // XZ - TODO: Adjust accuracy!
#if UNITY_EDITOR
        const int accuracy = 10; // A scalar for how many times bezier path is divided when determining vertex positions
#else
        const int accuracy = 100;
#endif
        const float minVertexSpacing = .01f;

        Transform transform;

        #endregion

        #region Constructors

        /// <summary> Splits bezier path into array of vertices along the path.</summary>
        ///<param name="maxAngleError">How much can the angle of the path change before a vertex is added. This allows fewer vertices to be generated in straighter sections.</param>
        ///<param name="minVertexDst">Vertices won't be added closer together than this distance, regardless of angle error.</param>
        public VertexPath(BezierPath bezierPath, Transform transform, float maxAngleError = 0.3f, float minVertexDst = 0) :
            this(bezierPath, VertexPathUtility.SplitBezierPathByAngleError(bezierPath, maxAngleError, minVertexDst, VertexPath.accuracy), transform)
        { }

        /// <summary> Splits bezier path into array of vertices along the path.</summary>
        ///<param name="maxAngleError">How much can the angle of the path change before a vertex is added. This allows fewer vertices to be generated in straighter sections.</param>
        ///<param name="minVertexDst">Vertices won't be added closer together than this distance, regardless of angle error.</param>
        ///<param name="accuracy">Higher value means the change in angle is checked more frequently.</param>
        public VertexPath(BezierPath bezierPath, Transform transform, float vertexSpacing) :
            this(bezierPath, VertexPathUtility.SplitBezierPathEvenly(bezierPath, Mathf.Max(vertexSpacing, minVertexSpacing), VertexPath.accuracy), transform)
        { }

        /// Internal contructor
        VertexPath(BezierPath bezierPath, VertexPathUtility.PathSplitData pathSplitData, Transform transform)
        {
            this.transform = transform;
            trans = transform;
            trans_pos = trans.position;

            space = bezierPath.Space;
            isClosedLoop = bezierPath.IsClosed;
            int numVerts = pathSplitData.vertices.Count;
            length = pathSplitData.cumulativeLength[numVerts - 1];

            localPoints = new Vector3[numVerts];
            localNormals = new Vector3[numVerts];
            localTangents = new Vector3[numVerts];
            funky_angles = new float[numVerts];
            funky_angle_global = bezierPath.funky_angle_global;
            funky_angle_global_offset = bezierPath.funky_angle_global_offset;
            funky_angle_global_mult = bezierPath.funky_angle_global_mult;

            cumulativeLengthAtEachVertex = new float[numVerts];
            times = new float[numVerts];
            bounds = new Bounds((pathSplitData.minMax.Min + pathSplitData.minMax.Max) / 2, pathSplitData.minMax.Max - pathSplitData.minMax.Min);

            // Figure out up direction for path
            up = (bounds.size.z > bounds.size.y) ? Vector3.up : -Vector3.forward;
            Vector3 lastRotationAxis = up;

            // Loop through the data and assign to arrays.
            for (int i = 0; i < localPoints.Length; ++i)
            {
                localPoints[i] = pathSplitData.vertices[i];
                localTangents[i] = pathSplitData.tangents[i];
                cumulativeLengthAtEachVertex[i] = pathSplitData.cumulativeLength[i];
                times[i] = cumulativeLengthAtEachVertex[i] / length;

                // Calculate normals
                if (space == PathSpace.xyz)
                {
                    if (i == 0)
                    {
                        localNormals[0] = Vector3.Cross(lastRotationAxis, pathSplitData.tangents[0]).normalized;
                    }
                    else
                    {
                        // First reflection
                        Vector3 offset = (localPoints[i] - localPoints[i - 1]);
                        float sqrDst = offset.sqrMagnitude;
                        Vector3 r = lastRotationAxis - offset * 2 / sqrDst * Vector3.Dot(offset, lastRotationAxis);
                        Vector3 t = localTangents[i - 1] - offset * 2 / sqrDst * Vector3.Dot(offset, localTangents[i - 1]);

                        // Second reflection
                        Vector3 v2 = localTangents[i] - t;
                        float c2 = Vector3.Dot(v2, v2);

                        Vector3 finalRot = r - v2 * 2 / c2 * Vector3.Dot(v2, r);
                        Vector3 n = Vector3.Cross(finalRot, localTangents[i]).normalized;
                        localNormals[i] = n;
                        lastRotationAxis = finalRot;
                    }
                }
                else
                {
                    localNormals[i] = Vector3.Cross(localTangents[i], up) * ((bezierPath.FlipNormals) ? 1 : -1);
                }
            }

            // Apply correction for 3d normals along a closed path
            if (space == PathSpace.xyz && isClosedLoop)
            {
                // Get angle between first and last normal (if zero, they're already lined up, otherwise we need to correct)
                float normalsAngleErrorAcrossJoin = Vector3.SignedAngle(localNormals[localNormals.Length - 1], localNormals[0], localTangents[0]);
                // Gradually rotate the normals along the path to ensure start and end normals line up correctly
                if (Mathf.Abs(normalsAngleErrorAcrossJoin) > 0.1f) // don't bother correcting if very nearly correct
                {
                    for (int i = 1; i < localNormals.Length; i++)
                    {
                        float t = (i / (localNormals.Length - 1f));
                        float angle = normalsAngleErrorAcrossJoin * t;
                        Quaternion rot = Quaternion.AngleAxis(angle, localTangents[i]);
                        localNormals[i] = rot * localNormals[i] * ((bezierPath.FlipNormals) ? -1 : 1);
                    }
                }
            }

            // Rotate normals to match up with user-defined anchor angles
            if (space == PathSpace.xyz)
            {
                for (int anchorIndex = 0; anchorIndex < pathSplitData.anchorVertexMap.Count - 1; anchorIndex++)
                {
                    int nextAnchorIndex = (isClosedLoop) ? (anchorIndex + 1) % bezierPath.NumSegments : anchorIndex + 1;

                    float startAngle = bezierPath.GetAnchorNormalAngle(anchorIndex) + bezierPath.GlobalNormalsAngle;
                    float endAngle = bezierPath.GetAnchorNormalAngle(nextAnchorIndex) + bezierPath.GlobalNormalsAngle;
                    float deltaAngle = Mathf.DeltaAngle(startAngle, endAngle);

                    float funky_start = bezierPath.GetFunkyAngle(anchorIndex);
                    float funky_end = bezierPath.GetFunkyAngle(nextAnchorIndex);
                    float funky_delta = funky_end - funky_start;

                    int startVertIndex = pathSplitData.anchorVertexMap[anchorIndex];
                    int endVertIndex = pathSplitData.anchorVertexMap[anchorIndex + 1];

                    int num = endVertIndex - startVertIndex;
                    if (anchorIndex == pathSplitData.anchorVertexMap.Count - 2)
                    {
                        num += 1;
                    }
                    for (int i = 0; i < num; i++)
                    {
                        int vertIndex = startVertIndex + i;
                        float t = i / (num - 1f);
                        float angle = startAngle + deltaAngle * t;
                        Quaternion rot = Quaternion.AngleAxis(angle, localTangents[vertIndex]);
                        localNormals[vertIndex] = (rot * localNormals[vertIndex]) * ((bezierPath.FlipNormals) ? -1 : 1);
                        funky_angles[vertIndex] = funky_start + (funky_delta * t);
                    }
                }
            }
        }

        #endregion

        #region Public methods and accessors

        public void UpdateTransform(Transform transform)
        {
            this.transform = transform;
        }
        public int NumPoints
        {
            get
            {
                return localPoints.Length;
            }
        }

        public Vector3 GetTangent(int index)
        {
#if XZ_OPTIMIZE_TRANS 
            return trans_pos + localTangents[index];
#else
            return MathUtility.TransformDirection(localTangents[index], transform, space);
#endif
        }
        public Vector3 GetNormal(int index)
        {
#if XZ_OPTIMIZE_TRANS
            return trans_pos + localNormals[index];
#else
            return MathUtility.TransformDirection(localNormals[index], transform, space);
#endif
        }
        public Vector3 GetPoint(int index)
        {
#if XZ_OPTIMIZE_TRANS
            return trans_pos + localPoints[index];
#else
            return MathUtility.TransformPoint(localPoints[index], transform, space);
#endif
        }

        /// ------- XZ ------- ///

        public static bool XZ_EnableRot = true;

        public Vector3 XZ_GetPointAtPosition(Vector3 pos, float? p_x_rot = null) => XZ_GetPointAtDistance(pos.z, (Vector2)pos, p_x_rot);
        public Vector3 XZ_GetPointAtDistance(float dst, Vector3 pos = default, float? p_x_rot = null)
        {
            float x_rot = (p_x_rot == null) ? pos.x : p_x_rot.Value;
            float t = dst / length;
            return XZ_GetPointAtTime(t, pos, x_rot);
        }
        public Vector3 XZ_GetPointAtTime(float t, Vector3 pos_offset, float x_rot)
        {
            var data = CalculatePercentOnPathData(t, EndOfPathInstruction.Stop);
            var rot = XZ_GetRotation(t, XZ_EnableRot ? x_rot : 0f, data);
            Vector3 result = Vector3.Lerp(GetPoint(data.previousIndex), GetPoint(data.nextIndex), data.percentBetweenIndices) + (rot * pos_offset);

            // If we are before or beyond the path, 'extrapolate':
            if (t < 0 || t > 1)
            {
                // Subtract the length if we are beyond the path, to get the delta between the desired and full length:
                float z = (t * length) - (t > 1 ? length : 0);
                result += rot * new Vector3(0, 0, z); // Add as Z
            }

            return result;
        }

        public Quaternion XZ_GetRotationAtDistance(float dst, float x = 0f)
        {
            float t = dst / length;
            return XZ_GetRotation(t, x);
        }
        public float XZ_GetMultForRotOffset(float t, TimeOnPathData? p_data = null)
        {
            if (t < 0f) return ((funky_angle_global * funky_angle_global_mult) + funky_angles[0]);
            else if (t > 1f) return ((funky_angle_global * funky_angle_global_mult) + funky_angles[funky_angles.Length - 1]);

            TimeOnPathData data = (p_data != null) ? p_data.Value : CalculatePercentOnPathData(t, EndOfPathInstruction.Stop);
            return (funky_angle_global * funky_angle_global_mult) + Mathf.Lerp(funky_angles[data.previousIndex], funky_angles[data.nextIndex], data.percentBetweenIndices);
        }
        public Quaternion XZ_GetRotation(float t, float x = 0f, TimeOnPathData? p_data = null)
        {
            //x *= XZ_GetMultForRotOffset(t, p_data);
            //x *= 0.01f;
            x += funky_angle_global_offset;
            x *= funky_angle_global_mult;

            TimeOnPathData data = (p_data != null) ? p_data.Value : CalculatePercentOnPathData(t, EndOfPathInstruction.Stop);
            Vector3 direction = Vector3.Lerp(localTangents[data.previousIndex], localTangents[data.nextIndex], data.percentBetweenIndices);
            Vector3 normal = Vector3.Lerp(localNormals[data.previousIndex], localNormals[data.nextIndex], data.percentBetweenIndices);

            Vector3 right = Vector3.Cross(direction, normal);
            normal += (right * x);
            normal = normal.normalized;

#if XZ_OPTIMIZE_TRANS
            return Quaternion.LookRotation(direction, normal);
#else
            return Quaternion.LookRotation(MathUtility.TransformDirection(direction, transform, space), MathUtility.TransformDirection(normal, transform, space));
#endif
        }

        public Vector3 XZ_NormTest(float t, float x) {
            TimeOnPathData data = CalculatePercentOnPathData(t, EndOfPathInstruction.Stop);
            Vector3 direction = Vector3.Lerp(localTangents[data.previousIndex], localTangents[data.nextIndex], data.percentBetweenIndices);
            Vector3 normal = Vector3.Lerp(localNormals[data.previousIndex], localNormals[data.nextIndex], data.percentBetweenIndices);

            Vector3 right = Vector3.Cross(direction, normal);
            normal += (right * x);

            return normal.normalized;
        }

        public Vector3 XZ_GetNormalAtDistance(float dst, float x = 0f)
        {
            float t = dst / length;
            return XZ_GetNormal(t, x);
        }
        public Vector3 XZ_GetNormal(float t, float x = 0f)
        {
            var data = CalculatePercentOnPathData(t, EndOfPathInstruction.Stop);
            Vector3 direction = Vector3.Lerp(localTangents[data.previousIndex], localTangents[data.nextIndex], data.percentBetweenIndices);
            Vector3 normal = Vector3.Lerp(localNormals[data.previousIndex], localNormals[data.nextIndex], data.percentBetweenIndices);
#if XZ_OPTIMIZE_TRANS
            return normal;
#else
            return MathUtility.TransformDirection(normal, transform, space);
#endif
        }

        /// ------------------ ///

        Vector3 GetPointAtDistance(float dst, EndOfPathInstruction endOfPathInstruction = EndOfPathInstruction.Stop)
        {
            float t = dst / length;
            return GetPointAtTime(t, endOfPathInstruction);
        }
        Vector3 GetPointAtTime(float t, EndOfPathInstruction endOfPathInstruction = EndOfPathInstruction.Stop)
        {
            var data = CalculatePercentOnPathData(t, endOfPathInstruction);
            return Vector3.Lerp(GetPoint(data.previousIndex), GetPoint(data.nextIndex), data.percentBetweenIndices);
        }

        Quaternion GetRotationAtDistance(float dst, EndOfPathInstruction endOfPathInstruction = EndOfPathInstruction.Stop)
        {
            float t = dst / length;
            return GetRotation(t, endOfPathInstruction);
        }
        Quaternion GetRotation(float t, EndOfPathInstruction endOfPathInstruction = EndOfPathInstruction.Stop)
        {
            var data = CalculatePercentOnPathData(t, endOfPathInstruction);
            Vector3 direction = Vector3.Lerp(localTangents[data.previousIndex], localTangents[data.nextIndex], data.percentBetweenIndices);
            Vector3 normal = Vector3.Lerp(localNormals[data.previousIndex], localNormals[data.nextIndex], data.percentBetweenIndices);
#if XZ_OPTIMIZE_TRANS
            return Quaternion.LookRotation(direction, normal);
#else
            return Quaternion.LookRotation(MathUtility.TransformDirection(direction, transform, space), MathUtility.TransformDirection(normal, transform, space));
#endif
        }

        Vector3 GetDirection(float t, EndOfPathInstruction endOfPathInstruction = EndOfPathInstruction.Stop)
        {
            var data = CalculatePercentOnPathData(t, endOfPathInstruction);
            Vector3 dir = Vector3.Lerp(localTangents[data.previousIndex], localTangents[data.nextIndex], data.percentBetweenIndices);
#if XZ_OPTIMIZE_TRANS
            return dir;
#else
            return MathUtility.TransformDirection(dir, transform, space);
#endif
        }
        Vector3 GetNormal(float t, EndOfPathInstruction endOfPathInstruction = EndOfPathInstruction.Stop)
        {
            var data = CalculatePercentOnPathData(t, endOfPathInstruction);
            Vector3 normal = Vector3.Lerp(localNormals[data.previousIndex], localNormals[data.nextIndex], data.percentBetweenIndices);
#if XZ_OPTIMIZE_TRANS
            return normal;
#else
            return MathUtility.TransformDirection(normal, transform, space);
#endif
        }

        Vector3 GetNormalAtDistance(float dst, EndOfPathInstruction endOfPathInstruction = EndOfPathInstruction.Stop)
        {
            float t = dst / length;
            return GetNormal(t, endOfPathInstruction);
        }
        public Vector3 GetDirectionAtDistance(float dst, EndOfPathInstruction endOfPathInstruction = EndOfPathInstruction.Stop)
        {
            float t = dst / length;
            return GetDirection(t, endOfPathInstruction);
        }

        Vector3 GetClosestPointOnPath(Vector3 worldPoint)
        {
            TimeOnPathData data = CalculateClosestPointOnPathData(worldPoint);
            return Vector3.Lerp(GetPoint(data.previousIndex), GetPoint(data.nextIndex), data.percentBetweenIndices);
        }
        public float GetClosestTimeOnPath(Vector3 worldPoint)
        {
            TimeOnPathData data = CalculateClosestPointOnPathData(worldPoint);
            return Mathf.Lerp(times[data.previousIndex], times[data.nextIndex], data.percentBetweenIndices);
        }
        public float GetClosestDistanceAlongPath(Vector3 worldPoint)
        {
            TimeOnPathData data = CalculateClosestPointOnPathData(worldPoint);
            return Mathf.Lerp(cumulativeLengthAtEachVertex[data.previousIndex], cumulativeLengthAtEachVertex[data.nextIndex], data.percentBetweenIndices);
        }

        #endregion

        #region Internal methods

        /// For a given value 't' between 0 and 1, calculate the indices of the two vertices before and after t. 
        /// Also calculate how far t is between those two vertices as a percentage between 0 and 1.
        TimeOnPathData CalculatePercentOnPathData(float t, EndOfPathInstruction endOfPathInstruction)
        {
            // Constrain t based on the end of path instruction
            switch (endOfPathInstruction)
            {
                case EndOfPathInstruction.Loop:
                    // If t is negative, make it the equivalent value between 0 and 1
                    if (t < 0)
                    {
                        t += Mathf.CeilToInt(Mathf.Abs(t));
                    }
                    t %= 1;
                    break;
                case EndOfPathInstruction.Reverse:
                    t = Mathf.PingPong(t, 1);
                    break;
                case EndOfPathInstruction.Stop:
                    t = Mathf.Clamp01(t);
                    break;
            }

            int prevIndex = 0;
            int nextIndex = NumPoints - 1;
            int i = Mathf.RoundToInt(t * (NumPoints - 1)); // starting guess

            // Starts by looking at middle vertex and determines if t lies to the left or to the right of that vertex.
            // Continues dividing in half until closest surrounding vertices have been found.
            while (true)
            {
                // t lies to left
                if (t <= times[i])
                {
                    nextIndex = i;
                }
                // t lies to right
                else
                {
                    prevIndex = i;
                }
                i = (nextIndex + prevIndex) / 2;

                if (nextIndex - prevIndex <= 1)
                {
                    break;
                }
            }

            float abPercent = Mathf.InverseLerp(times[prevIndex], times[nextIndex], t);
            return new TimeOnPathData(prevIndex, nextIndex, abPercent);
        }

        /// Calculate time data for closest point on the path from given world point
        TimeOnPathData CalculateClosestPointOnPathData(Vector3 worldPoint)
        {
            float minSqrDst = float.MaxValue;
            Vector3 closestPoint = Vector3.zero;
            int closestSegmentIndexA = 0;
            int closestSegmentIndexB = 0;

            for (int i = 0; i < localPoints.Length; i++)
            {
                int nextI = i + 1;
                if (nextI >= localPoints.Length)
                {
                    if (isClosedLoop)
                    {
                        nextI %= localPoints.Length;
                    }
                    else
                    {
                        break;
                    }
                }

                Vector3 closestPointOnSegment = MathUtility.ClosestPointOnLineSegment(worldPoint, GetPoint(i), GetPoint(nextI));
                float sqrDst = (worldPoint - closestPointOnSegment).sqrMagnitude;
                if (sqrDst < minSqrDst)
                {
                    minSqrDst = sqrDst;
                    closestPoint = closestPointOnSegment;
                    closestSegmentIndexA = i;
                    closestSegmentIndexB = nextI;
                }

            }
            float closestSegmentLength = (GetPoint(closestSegmentIndexA) - GetPoint(closestSegmentIndexB)).magnitude;
            float t = (closestPoint - GetPoint(closestSegmentIndexA)).magnitude / closestSegmentLength;
            return new TimeOnPathData(closestSegmentIndexA, closestSegmentIndexB, t);
        }

        public struct TimeOnPathData
        {
            public readonly int previousIndex;
            public readonly int nextIndex;
            public readonly float percentBetweenIndices;

            public TimeOnPathData(int prev, int next, float percentBetweenIndices)
            {
                this.previousIndex = prev;
                this.nextIndex = next;
                this.percentBetweenIndices = percentBetweenIndices;
            }
        }

        #endregion

    }

}