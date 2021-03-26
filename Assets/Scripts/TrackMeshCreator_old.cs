using PathCreation;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class TrackMeshCreator_old : PathSceneTool
{
    public static TrackMeshCreator_old Instance;

    #region Properties
    [Header("Road settings")]
    public float roadWidth = 2.36f;
    [Range(0, .5f)]
    public float roadThickness = .4f;
    public bool flattenSurface;

    [Header("Material settings")]
    public Material roadMaterial;
    public Material undersideMaterial;
    public float textureTiling = 1;

    [Header("Rhythmic track creation")]
    public float debug_startPoint;
    public float debug_length;
    [Range(-10, 10)]
    public int debug_xPosition;

    public float edgeLightsThickness = 0.1f;

    public bool reactToPathChanges;
    #endregion

    #region Prefabs & Materials
    public GameObject EdgeLightsPrefab;
    public GameObject TrackPrefab;
    public GameObject SectionPrefab;

    public Material EdgeLightsMaterial;
    #endregion

    private void Awake()
    {
        Instance = this;
        if (!pathCreator) pathCreator = GameObject.Find("Path").GetComponent<PathCreator>(); // temp!

        EdgeLightsPrefab = (GameObject)Resources.Load("Prefabs/EdgeLights");
        EdgeLightsMaterial = (Material)Resources.Load("Materials/EdgeLightsMaterial");
    }

    public static bool forceUpdate;
    protected override void PathUpdated(bool force = false)
    {
        if (TrackObjects != null && (force ||reactToPathChanges))
        {
            int counter = 0;
            foreach (GameObject trackObj in TrackObjects)
            {
                if (trackObj) trackObj.GetComponent<MeshFilter>().sharedMesh = CreateMesh(debug_startPoint, debug_length, RhythmicGame.TrackWidth * int.Parse(trackObj.name));
                counter++;
            }
        }
    }

    [HideInInspector]
    public List<GameObject> TrackObjects = new List<GameObject>();

    // Functions
    public Mesh CreateMeshFloat(float startDistance = 0f, float length = 8f, float xPosition = 0f, float width = 0f, float thickness = 0f, float yElevation = 0f)
    {
        if (width == 0f) width = roadWidth;
        if (thickness == 0f) thickness = roadThickness;

        bool usePathNormals = !(path.space == PathSpace.xyz && flattenSurface);

        /* Vertices for the top of the road are laid out like this:
           0  1
           8  9
        and so on... So the triangle map 0,8,1 for example, defines a triangle from top left to bottom left to bottom right. */
        int[] triangleMap = { 0, 8, 1, 1, 8, 9 };

        // Doubled the side triangles so that we have both outside and inside materials
        int[] sidesTriangleMap = {
           4, 6, 14,
           12, 4, 14,
           5, 15, 7,
           13, 15, 5,

           4, 12, 14,
           6, 4, 14,
           5, 15, 13,
           7, 15, 5 };

        // Get start and end vertex index data
        if (length == -1) // -1 means full path length
            length = path.length;
        var startVertex = path.GetIndexAtDistance(startDistance, EndOfPathInstruction.Stop);
        var endVertex = path.GetIndexAtDistance(startDistance + length, EndOfPathInstruction.Stop);

        Debug.LogFormat("startVertex: {0} | endVertex: {1}", startVertex.previousIndex, endVertex.nextIndex);

        Vector3[] verts = new Vector3[(startVertex.previousIndex + endVertex.nextIndex) * 2 * 8];
        Vector2[] uvs = new Vector2[verts.Length];
        Vector3[] normals = new Vector3[verts.Length];

        int numTris = 2 * (path.NumPoints - 1) + ((path.isClosedLoop) ? 2 : 0);
        int[] roadTriangles = new int[numTris * 3];
        int[] underRoadTriangles = new int[numTris * 3];
        int[] sideOfRoadTriangles = new int[numTris * 4 * 3];

        int vertIndex = 0;
        int triIndex = 0;

        int indexCounter = 0;
        for (float i = startDistance; i <= startDistance + length; i += 1f) // Go through indexes between start and end
        {
            Vector3 localUp = (usePathNormals) ? Vector3.Cross(path.GetTangentAtDistance(i), path.GetNormalAtDistance(i)) : path.up;
            Vector3 localRight = (usePathNormals) ? path.GetNormalAtDistance(i) : Vector3.Cross(localUp, path.GetTangentAtDistance(i));

            // Find position to left and right of current path vertex
            Vector3 vertSideA = path.GetPointAtDistance(i) - localRight * Mathf.Abs(roadWidth / 2f);
            Vector3 vertSideB = path.GetPointAtDistance(i) + localRight * Mathf.Abs(roadWidth / 2f);

            // ***** OFFSET VERTEX POINTS FOR PARALLEL CURVES *****
            // The calculations below create the mesh based on the vertex positions.
            // By offseting the vertex positions, the calculations will take place according to their positions and the curve will thus be bigger or smaller, depending on the vertex positions.
            vertSideB += (localRight * xPosition) + (localUp * yElevation);
            vertSideA += (localRight * xPosition) + (localUp * yElevation);

            #region Add vertices, UVs and normals
            // Add top of road vertices
            verts[vertIndex + 0] = vertSideA;
            verts[vertIndex + 1] = vertSideB;
            // Add bottom of road vertices
            verts[vertIndex + 2] = vertSideA - localUp * (thickness / 2f);
            verts[vertIndex + 3] = vertSideB - localUp * (thickness / 2f);

            // Duplicate vertices to get flat shading for sides of road
            verts[vertIndex + 4] = verts[vertIndex + 0];
            verts[vertIndex + 5] = verts[vertIndex + 1];
            verts[vertIndex + 6] = verts[vertIndex + 2];
            verts[vertIndex + 7] = verts[vertIndex + 3];

            // Set uv on y axis to path time (0 at start of path, up to 1 at end of path)
            uvs[vertIndex + 0] = new Vector2(0, path.GetPointAtDistance(i).z);
            uvs[vertIndex + 1] = new Vector2(1, path.GetPointAtDistance(i).z);

            // Top of road normals
            normals[vertIndex + 0] = localUp;
            normals[vertIndex + 1] = localUp;
            // Bottom of road normals
            normals[vertIndex + 2] = -localUp;
            normals[vertIndex + 3] = -localUp;
            // Sides of road normals
            normals[vertIndex + 4] = -localRight;
            normals[vertIndex + 5] = localRight;
            normals[vertIndex + 6] = -localRight;
            normals[vertIndex + 7] = localRight;
            #endregion

            int prevIndexCounter = indexCounter;
            indexCounter = path.GetIndexAtDistance(i).nextIndex;

            if (prevIndexCounter == indexCounter) continue;
            // add to vert and tri index counters
            vertIndex += 8;
            triIndex += 6;
        }

        vertIndex = 0;
        triIndex = 0;

        List<int> roadTris = new List<int>();
        List<int> underRoadTris = new List<int>();
        List<int> sideRoadTris = new List<int>();

        //for (int i = startVertex.previousIndex; i < endVertex.nextIndex; i++)
        for (int i = 0; i < 1; i++)
        {
            // Set triangle indices
            //if (i < endVertex.nextIndex || path.isClosedLoop)
            {
                try
                {
                    for (int j = 0; j < triangleMap.Length; j++)
                    {
                        //roadTriangles[triIndex + j] = (vertIndex + triangleMap[j]) % verts.Length;
                        roadTris.Add((vertIndex + triangleMap[j]) % verts.Length);

                        // reverse triangle map for under road so that triangles wind the other way and are visible from underneath
                        //underRoadTriangles[triIndex + j] = (vertIndex + triangleMap[triangleMap.Length - 1 - j] + 2) % verts.Length;
                        underRoadTris.Add((vertIndex + triangleMap[triangleMap.Length - 1 - j] + 2) % verts.Length);
                    }
                    for (int j = 0; j < sidesTriangleMap.Length; j++)
                        //sideOfRoadTriangles[triIndex * 4 + j] = (vertIndex + sidesTriangleMap[j]) % verts.Length;
                        sideRoadTris.Add((vertIndex + sidesTriangleMap[j]) % verts.Length);
                }
                catch
                {
                    Debug.DebugBreak();
                }
            }

            // add to vert and tri index counters
            vertIndex += 8;
            triIndex += 6;
        }

        roadTriangles = roadTris.ToArray();
        underRoadTriangles = underRoadTris.ToArray();
        sideOfRoadTriangles = sideRoadTris.ToArray();

        // Create and setup mesh
        Mesh mesh = new Mesh();

        mesh.vertices = verts;
        mesh.uv = uvs;
        mesh.normals = normals;
        mesh.subMeshCount = 3;

        mesh.SetTriangles(roadTriangles, 0);
        mesh.SetTriangles(sideOfRoadTriangles, 1);
        mesh.SetTriangles(underRoadTriangles, 2);

        //mesh.RecalculateNormals();
        //mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        return mesh;
    }
    public Mesh CreateMesh(float startDistance = 0f, float length = 8f, float xPosition = 0f, float width = 0f, float thickness = 0f, float yElevation = 0f)
    {
        if (width == 0f) width = roadWidth;
        if (thickness == 0f) thickness = roadThickness;

        Vector3[] verts = new Vector3[path.NumPoints * 8];
        Vector2[] uvs = new Vector2[verts.Length];
        Vector3[] normals = new Vector3[verts.Length];

        int numTris = 2 * (path.NumPoints - 1) + ((path.isClosedLoop) ? 2 : 0);
        int[] roadTriangles = new int[numTris * 3];
        int[] underRoadTriangles = new int[numTris * 3];
        int[] sideOfRoadTriangles = new int[numTris * 4 * 3];

        bool usePathNormals = !(path.space == PathSpace.xyz && flattenSurface);

        /* Vertices for the top of the road are laid out like this:
           0  1
           8  9
        and so on... So the triangle map 0,8,1 for example, defines a triangle from top left to bottom left to bottom right. */
        int[] triangleMap = { 0, 8, 1, 1, 8, 9 };
        /*int[] sidesTriangleMap = { 
           4, 6, 14,
           12, 4, 14,
           5, 15, 7,
           13, 15, 5 };*/

        // Doubled the sides so that we have both outside and inside materials
        int[] sidesTriangleMap = {
           4, 6, 14,
           12, 4, 14,
           5, 15, 7,
           13, 15, 5,

           4, 12, 14,
           6, 4, 14,
           5, 15, 13,
           7, 15, 5 };

        // Get start and end vertex index data
        if (length == -1) // -1 means full path length
            length = path.length;
        var startVertex = path.GetIndexAtDistance(startDistance, EndOfPathInstruction.Stop);
        var endVertex = path.GetIndexAtDistance(startDistance + length, EndOfPathInstruction.Stop);

        //Debug.LogFormat("startVertex: {0} | endVertex: {1}", startVertex.previousIndex, endVertex.nextIndex);

        int vertIndex = 0;
        int triIndex = 0;

        for (int i = startVertex.previousIndex; i <= endVertex.nextIndex; i++) // Go through indexes between start and end (end is not included) // TODO: investigate into non-inclusion of end index?
        {
            Vector3 localUp = (usePathNormals) ? Vector3.Cross(path.GetTangent(i), path.GetNormal(i)) : path.up;
            Vector3 localRight = (usePathNormals) ? path.GetNormal(i) : Vector3.Cross(localUp, path.GetTangent(i));

            // Find position to left and right of current path vertex
            Vector3 vertSideA = path.GetPoint(i) - localRight * Mathf.Abs(roadWidth / 2f);
            Vector3 vertSideB = path.GetPoint(i) + localRight * Mathf.Abs(roadWidth / 2f);

            // ***** OFFSET VERTEX POINTS FOR PARALLEL CURVES *****
            // The calculations below create the mesh based on the vertex positions.
            // By offseting the vertex positions, the calculations will take place according to their positions and the curve will thus be bigger or smaller, depending on the vertex positions.
            vertSideB += (localRight * xPosition) + (localUp * yElevation);
            vertSideA += (localRight * xPosition) + (localUp * yElevation);

            #region Add vertices, UVs and normals
            // Add top of road vertices
            verts[vertIndex + 0] = vertSideA;
            verts[vertIndex + 1] = vertSideB;
            // Add bottom of road vertices
            verts[vertIndex + 2] = vertSideA - localUp * (thickness / 2f);
            verts[vertIndex + 3] = vertSideB - localUp * (thickness / 2f);

            // Duplicate vertices to get flat shading for sides of road
            verts[vertIndex + 4] = verts[vertIndex + 0];
            verts[vertIndex + 5] = verts[vertIndex + 1];
            verts[vertIndex + 6] = verts[vertIndex + 2];
            verts[vertIndex + 7] = verts[vertIndex + 3];

            // Set uv on y axis to path time (0 at start of path, up to 1 at end of path)
            uvs[vertIndex + 0] = new Vector2(0, path.times[i]);
            uvs[vertIndex + 1] = new Vector2(1, path.times[i]);

            // Top of road normals
            normals[vertIndex + 0] = localUp;
            normals[vertIndex + 1] = localUp;
            // Bottom of road normals
            normals[vertIndex + 2] = -localUp;
            normals[vertIndex + 3] = -localUp;
            // Sides of road normals
            normals[vertIndex + 4] = -localRight;
            normals[vertIndex + 5] = localRight;
            normals[vertIndex + 6] = -localRight;
            normals[vertIndex + 7] = localRight;
            #endregion

            // Set triangle indices
            if (i <= endVertex.previousIndex || path.isClosedLoop)
            {
                for (int j = 0; j < triangleMap.Length; j++)
                {
                    roadTriangles[triIndex + j] = (vertIndex + triangleMap[j]) % verts.Length;

                    // reverse triangle map for under road so that triangles wind the other way and are visible from underneath
                    underRoadTriangles[triIndex + j] = (vertIndex + triangleMap[triangleMap.Length - 1 - j] + 2) % verts.Length;
                }
                for (int j = 0; j < sidesTriangleMap.Length; j++)
                    sideOfRoadTriangles[triIndex * 4 + j] = (vertIndex + sidesTriangleMap[j]) % verts.Length;
            }

            // add to vert and tri index counters
            vertIndex += 8;
            triIndex += 6;
        }

        // Create and setup mesh
        Mesh mesh = new Mesh();

        mesh.vertices = verts;
        mesh.uv = uvs;
        mesh.normals = normals;
        mesh.subMeshCount = 3;

        mesh.SetTriangles(roadTriangles, 0);
        mesh.SetTriangles(sideOfRoadTriangles, 1);
        mesh.SetTriangles(underRoadTriangles, 2);

        mesh.RecalculateBounds();

        return mesh;
    }

    public GameObject CreateTrackObject(string name = "", AmpTrack.InstrumentType inst = AmpTrack.InstrumentType.Bass, float startDistance = 0f, float length = 8f, float xPosition = 0f, float yElevation = 0f)
    {
        GameObject obj = new GameObject() { name = name };

        // Create and add mesh
        var mesh = CreateMesh(startDistance, length, xPosition, RhythmicGame.TrackWidth, RhythmicGame.TrackHeight, yElevation);

        obj.AddComponent<MeshFilter>().mesh = mesh;
        var renderer = obj.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = AmpTrack.Colors.GetMaterialForInstrument(inst);
        renderer.sharedMaterials[0].mainTextureScale = new Vector2(1, path.length); // TODO: texture scaling seems wrong (?)

        // Create and add Edge lights!
        //var edgelights = CreateEdgeLights(startDistance, length, xPosition, Track.Colors.ColorFromTrackType(inst));
        //edgelights.transform.parent = obj.transform;

        // Add measure script!
        // TODO!!!

        return obj;
    }

    public GameObject CreateEdgeLights(float startPoint, float length = 16f, float xPosition = 0f, Color? color = null, string name = "Edge lights")
    {
        // Create object and mesh
        var gObj = Instantiate(EdgeLightsPrefab); gObj.name = name; gObj.layer = 11;

        var meshFilter = gObj.GetComponent<MeshFilter>();
        var meshRenderer = gObj.GetComponent<MeshRenderer>();

        meshFilter.mesh = CreateMesh(startPoint, length, xPosition, RhythmicGame.TrackWidth, edgeLightsThickness, edgeLightsThickness / 2);
        meshRenderer.sharedMaterials = new Material[2] { EdgeLightsMaterial, EdgeLightsMaterial };

        var edgeLightCom = gObj.GetComponent<EdgeLights>();
        if (color.HasValue) edgeLightCom.Color = color.Value;

        return gObj;
    }

    // Debug functions
    public GameObject Debug_CreateTestMesh(bool keepCurrentXPosition = false, bool keepIncreasingStartPoint = false)
    {
        if (!pathCreator)
            throw new System.Exception("TrackMeshCreator: No PathCreator was found!");

        // Create track mesh object!
        var go = CreateTestObject(); go.layer = 11;
        //var edgelight = CreateEdgeLights(debug_startPoint, debug_length, debug_xPosition * RhythmicGame.TrackWidth);
        //edgelight.GetComponent<EdgeLights>().GlowIntenstiy = 1.2f;
        //edgelight.transform.parent = go.transform;

        // Automatically increase track index counter
        if (!keepCurrentXPosition)
            debug_xPosition++;
        if (keepIncreasingStartPoint)
            debug_startPoint += debug_length;

        TrackObjects.Add(go);

        return go;
    }
    // Add MeshRenderer and MeshFilter components to this gameobject if not already attached
    GameObject CreateTestObject(string objectName = "TestTrackMesh")
    {
        var meshHolder = new GameObject();
        meshHolder.name = debug_xPosition.ToString();

        meshHolder.transform.rotation = Quaternion.identity;
        meshHolder.transform.position = Vector3.zero;
        meshHolder.transform.localScale = Vector3.one;

        // Ensure mesh renderer and filter components are assigned
        if (!meshHolder.gameObject.GetComponent<MeshFilter>())
            meshHolder.gameObject.AddComponent<MeshFilter>();
        if (!meshHolder.GetComponent<MeshRenderer>())
            meshHolder.gameObject.AddComponent<MeshRenderer>();

        var meshRenderer = meshHolder.GetComponent<MeshRenderer>();
        var meshFilter = meshHolder.GetComponent<MeshFilter>();
        meshFilter.sharedMesh = CreateMesh(debug_startPoint, debug_length, debug_xPosition * RhythmicGame.TrackWidth);

        if (roadMaterial != null && undersideMaterial != null)
        {
            meshRenderer.sharedMaterials = new Material[] { roadMaterial, undersideMaterial, undersideMaterial };
            //meshRenderer.sharedMaterials[0].mainTextureScale = new Vector3(1, path.length);
        }

        return meshHolder;
    }


}