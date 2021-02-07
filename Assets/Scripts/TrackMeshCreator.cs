using UnityEngine;
using PathCreation;

public class TrackMeshCreator : MonoBehaviour
{
    /// Editor functionality
    #region Editor functionality
    public Material material;
    public GameObject lastGo;

    [Header("Individual unit sizes")]
    public float width = 0.45f;
    public float length = 0.45f;

    [Header("Desired width & length")]
    public float DesiredWidth = 3.6f;
    public float DesiredLength = 32f;

    [Header("Other properties")]
    public float xPosition = 0f;
    public float yElevation = 0f;

    public GameObject CreateGameObject(bool debug)
    {
        Mesh mesh = CreateMesh_Editor(debug);

        var go = new GameObject();
        var meshFilter = go.AddComponent<MeshFilter>();
        var meshRenderer = go.AddComponent<MeshRenderer>();

        // Assign mesh
        meshFilter.sharedMesh = mesh;
        // Assign material
        meshRenderer.material = material;

        lastGo = go;
        return go;
    }
    public void UpdateGameObject()
    {
        if (!lastGo) { Debug.LogError("MeshTest: lastGo is null!"); return; }
        lastGo.GetComponent<MeshFilter>().mesh = CreateMesh_Editor();
    }

    public bool TestDivisibility_Editor = true;
    public Mesh CreateMesh_Editor(bool debug = false) { return CreateMesh(DesiredWidth, DesiredLength, width, length, debug); }
    #endregion

    /// Standalone functionality
    public static bool TestDivisibility = true;


    /// <summary>
    /// Checks whether the piece sizings are divisible.
    /// </summary>
    /// <returns>
    /// true if the piece sizings are divisible <br/>
    /// false if the piece sizings are NOT divisible
    /// </returns>
    public static bool TestPieceSizingDivisibility(float desiredX, float desiredZ, float pieceX, float pieceZ) { return (desiredX % pieceX == 0 & desiredZ % pieceZ == 0); }
    public bool TestPieceSizingDivisibility() { return TestPieceSizingDivisibility(DesiredWidth, DesiredLength, width, length); }

    static Mesh cachedMesh;
    static float lastDesiredValues;

    // TODO: Optimize this code!
    public static Mesh CreateMesh(float desiredX, float desiredZ, float pieceX = 0.45f, float pieceZ = 0.5f, bool debug = false)
    {
        // Returned cached mesh if request is the same
        if (desiredX + desiredZ == lastDesiredValues) return cachedMesh;

        // Temp!
        if (Application.isPlaying)
        {
            //pieceX = RhythmicGame.TrackWidth / 8;
            pieceX = RhythmicGame.TrackWidth;
            pieceZ = desiredZ / 32;
        }

        // Test whether individual piece sizings are divisible0
        if (TestDivisibility & !TestPieceSizingDivisibility(desiredX, desiredZ, pieceX, pieceZ))
            // TODO: logging control here
            Debug.LogWarningFormat("MeshTest/CreateMesh(): Piece sizings are not divisible! (width: {0} | length: {1})", pieceX, pieceZ);

        // Get counts for the individual pieces
        int xSize = Mathf.RoundToInt(desiredX / pieceX);
        int zSize = Mathf.RoundToInt(desiredZ / pieceZ);
        //for (float x = 0f; x < desiredX; x += pieceX) xSize++;
        //for (float z = 0f; z < desiredZ; z += pieceZ) zSize++;

        // TODO: it adds 1 extra piece for some reason...
        //if (desiredZ % pieceZ != 0)
        //zSize--;

        Vector3[] vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        int[] triangles = new int[xSize * zSize * 6];
        Vector2[] uv = new Vector2[vertices.Length];
        Vector4[] tangents = new Vector4[vertices.Length];

        // Vertices:
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++, i++)
            {
                float xValue = x * pieceX - desiredX / 2; // Center X
                float zValue = z * pieceZ;
                vertices[i] = new Vector3(xValue, 0, zValue);
                uv[i] = new Vector2((float)x / xSize, (float)z / xSize);
                tangents[i] = new Vector4(1f, 0f, 0f, -1f);
            }
        }

        // Triangles:
        for (int ti = 0, vi = 0, y = 0; y < zSize; y++, vi++)
        {
            for (int x = 0; x < xSize; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
                triangles[ti + 5] = vi + xSize + 2;
            }
        }

        if (debug)
        {
            Logger.Log(vertices, true);
            Logger.Log(triangles, false);

            // Debug draw vertices
            if (GameObject.FindGameObjectWithTag("Remove") == null)
            {
                int vID = 0;
                foreach (Vector3 v in vertices)
                {
                    GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    go.tag = "Remove";
                    go.transform.position = v;
                    go.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
                    go.name = vID.ToString();
                    vID++;
                }
            }
        }

        Mesh mesh = new Mesh() { name = "Generated mesh (cust)" };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.tangents = tangents;
        mesh.RecalculateNormals();

        // Store currently requested values for caching
        lastDesiredValues = desiredX + desiredZ;
        cachedMesh = mesh;

        return mesh;
    }
    public static Mesh CreateMeshFromPathIndexes(float width, float height, Vector3 position, float angle, VertexPath path = null)
    {
        if (width == 0) width = RhythmicGame.TrackWidth;
        if (path == null) path = PathTools.Path;

        Vector3 tunnelCenter = (Tunnel.Instance) ? Tunnel.Instance.center : Vector3.zero;

        bool isStartPointAdjustment = (SongController.Instance.StartDistance < 0);
        int startPointAdjustionNum = isStartPointAdjustment ? 1 : 0;
        float startPointAdjustment = SongController.Instance.StartDistance;

        Vector3[] vertices = new Vector3[(startPointAdjustionNum + path.NumPoints) * 8];
        Vector2[] uvs = new Vector2[vertices.Length];
        Vector3[] normals = new Vector3[vertices.Length];

        int numTris = 2 * ((startPointAdjustionNum + path.NumPoints) - 1) + (path.isClosedLoop ? 2 : 0);
        int[] roadTriangles = new int[numTris * 3];
        int[] underRoadTriangles = new int[numTris * 3];
        int[] sideOfRoadTriangles = new int[numTris * 4 * 3];

        /* Vertices for the top of the road are laid out like this:
           0  1
           8  9
        So the triangle map 0,8,1 for example, defines a triangle from top left to bottom left to bottom right. */
        int[] triangleMap = { 0, 8, 1, 1, 8, 9 };

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

        // First vertices & tris are the starting point adjustion!
        if (isStartPointAdjustment)
        {
            Vector3 localUp = Vector3.Cross(path.GetTangent(0), path.GetNormal(0));
            Vector3 localRight = path.GetNormal(0);

            // Find position to left and right of current path vertex
            Vector3 vertSideB = PathTools.GetPositionOnPath(path, startPointAdjustment, position - tunnelCenter) + localRight * Mathf.Abs(width / 2f);
            Vector3 vertSideA = PathTools.GetPositionOnPath(path, startPointAdjustment, position - tunnelCenter) - localRight * Mathf.Abs(width / 2f);

            vertSideA += (localUp * 0.01f);
            vertSideB += (localUp * 0.01f);

            #region Add vertices, UVs and normals
            // Add top of road vertices
            vertices[0] = vertSideA;
            vertices[1] = vertSideB;
            // Add bottom of road vertices
            vertices[2] = vertSideA - localUp * (height / 2f);
            vertices[3] = vertSideB - localUp * (height / 2f);

            // Duplicate vertices to get flat shading for sides of road
            vertices[4] = vertices[0];
            vertices[5] = vertices[1];
            vertices[6] = vertices[2];
            vertices[7] = vertices[3];

            // Set uv on y axis to path time (0 at start of path, up to 1 at end of path)
            uvs[0] = new Vector2(0, path.times[0]);
            uvs[1] = new Vector2(1, path.times[0]);

            // Top of road normals
            normals[0] = localUp;
            normals[1] = localUp;
            // Bottom of road normals
            normals[2] = -localUp;
            normals[3] = -localUp;
            // Sides of road normals
            normals[4] = -localRight;
            normals[5] = localRight;
            normals[6] = -localRight;
            normals[7] = localRight;
            #endregion

            // Set triangle indices
            for (int j = 0; j < triangleMap.Length; j++)
            {
                roadTriangles[0 + j] = (0 + triangleMap[j]) % vertices.Length;

                // reverse triangle map for under road so that triangles wind the other way and are visible from underneath
                underRoadTriangles[0 + j] = (0 + triangleMap[triangleMap.Length - 1 - j] + 2) % vertices.Length;
            }

            for (int j = 0; j < sidesTriangleMap.Length; j++)
                sideOfRoadTriangles[0 * 4 + j] = (0 + sidesTriangleMap[j]) % vertices.Length;
        }

        // Get start and end vertex index data
        var startVertex = path.GetIndexAtDistance(0, EndOfPathInstruction.Stop);
        var endVertex = path.GetIndexAtDistance(path.length, EndOfPathInstruction.Stop);

        int vertIndex = isStartPointAdjustment ? 8 : 0;
        int triIndex = isStartPointAdjustment ? 6 : 0;

        // ----- WORK ----- //
        for (int i = startVertex.previousIndex; i <= endVertex.nextIndex; i++) // Go through indexes between start and end (end is not included) // TODO: investigate into non-inclusion of end index?
        {
            Vector3 localUp = Vector3.Cross(path.GetTangent(i), path.GetNormal(i));
            Vector3 localRight = path.GetNormal(i);

            // Find position to left and right of current path vertex
            Vector3 vertSideA = path.GetPoint(i) - localRight * Mathf.Abs(width / 2f);
            Vector3 vertSideB = path.GetPoint(i) + localRight * Mathf.Abs(width / 2f);

            // ***** OFFSET VERTEX POINTS FOR PARALLEL CURVES *****
            // The calculations below create the mesh based on the vertex positions.
            // By offseting the vertex positions, the calculations will take place according to their positions and the curve will thus be bigger or smaller, depending on the vertex positions.
            vertSideB += (localRight * position.x) + (localUp * position.y);
            vertSideA += (localRight * position.x) + (localUp * position.y);

            vertSideA += (localUp * 0.01f);
            vertSideB += (localUp * 0.01f);

            #region Add vertices, UVs and normals
            // Add top of road vertices
            vertices[vertIndex + 0] = vertSideA;
            vertices[vertIndex + 1] = vertSideB;
            // Add bottom of road vertices
            vertices[vertIndex + 2] = vertSideA - localUp * (height / 2f);
            vertices[vertIndex + 3] = vertSideB - localUp * (height / 2f);

            // Duplicate vertices to get flat shading for sides of road
            vertices[vertIndex + 4] = vertices[vertIndex + 0];
            vertices[vertIndex + 5] = vertices[vertIndex + 1];
            vertices[vertIndex + 6] = vertices[vertIndex + 2];
            vertices[vertIndex + 7] = vertices[vertIndex + 3];

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
                    roadTriangles[triIndex + j] = (vertIndex + triangleMap[j]) % vertices.Length;

                    // reverse triangle map for under road so that triangles wind the other way and are visible from underneath
                    underRoadTriangles[triIndex + j] = (vertIndex + triangleMap[triangleMap.Length - 1 - j] + 2) % vertices.Length;
                }
                for (int j = 0; j < sidesTriangleMap.Length; j++)
                    sideOfRoadTriangles[triIndex * 4 + j] = (vertIndex + sidesTriangleMap[j]) % vertices.Length;
            }

            // add to vert and tri index counters
            vertIndex += 8;
            triIndex += 6;
        }

        // ----- FINAL ----- //

        Mesh mesh = new Mesh() { name = "Generated mesh (full-length)" };

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.normals = normals;
        mesh.subMeshCount = 3;

        mesh.SetTriangles(roadTriangles, 0);
        mesh.SetTriangles(sideOfRoadTriangles, 1);
        mesh.SetTriangles(underRoadTriangles, 2);

        mesh.RecalculateBounds();

        return mesh;
    }

#if false
    public static Mesh CreateFullLengthMeshForPath(float width, float height, Vector3 position, float angle, VertexPath path = null)
    {
        if (width == 0) width = RhythmicGame.TrackWidth;
        if (path == null) path = PathTools.Path;

        Mesh mesh = new Mesh() { name = "Procgen mesh (full-length)" };

        Vector3[] vertices = new Vector3[path.NumPoints * 8];
        Vector2[] uvs = new Vector2[vertices.Length];
        Vector3[] normals = new Vector3[vertices.Length];

        int numTris = 2 * (path.NumPoints - 1) + (path.isClosedLoop ? 2 : 0);
        int[] roadTriangles = new int[numTris * 3];
        int[] underRoadTriangles = new int[numTris * 3];
        int[] sideOfRoadTriangles = new int[numTris * 4 * 3];

        /* Vertices for the top of the road are laid out like this:
           0  1
           8  9
        So the triangle map 0,8,1 for example, defines a triangle from top left to bottom left to bottom right. */
        int[] triangleMap = { 0, 8, 1, 1, 8, 9 };

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
        var startVertex = path.GetIndexAtDistance(0, EndOfPathInstruction.Stop);
        var endVertex = path.GetIndexAtDistance(path.length, EndOfPathInstruction.Stop);

        int vertIndex = 0;
        int triIndex = 0;

        // ------------------------------ //

        for (int i = startVertex.previousIndex; i <= endVertex.nextIndex; i++) // Go through indexes between start and end (end is not included) // TODO: investigate into non-inclusion of end index?
        {
            Vector3 localUp = Vector3.Cross(path.GetTangent(i), path.GetNormal(i));
            Vector3 localRight = path.GetNormal(i);

            // Find position to left and right of current path vertex
            Vector3 vertSideA = path.GetPoint(i) - localRight * Mathf.Abs(width / 2f);
            Vector3 vertSideB = path.GetPoint(i) + localRight * Mathf.Abs(width / 2f);

            // ***** OFFSET VERTEX POINTS FOR PARALLEL CURVES *****
            // The calculations below create the mesh based on the vertex positions.
            // By offseting the vertex positions, the calculations will take place according to their positions and the curve will thus be bigger or smaller, depending on the vertex positions.
            vertSideB += (localRight * position.x) + (localUp * position.y);
            vertSideA += (localRight * position.x) + (localUp * position.y);

    #region Add vertices, UVs and normals
            // Add top of road vertices
            vertices[vertIndex + 0] = vertSideA;
            vertices[vertIndex + 1] = vertSideB;
            // Add bottom of road vertices
            vertices[vertIndex + 2] = vertSideA - localUp * (height / 2f);
            vertices[vertIndex + 3] = vertSideB - localUp * (height / 2f);

            // Duplicate vertices to get flat shading for sides of road
            vertices[vertIndex + 4] = vertices[vertIndex + 0];
            vertices[vertIndex + 5] = vertices[vertIndex + 1];
            vertices[vertIndex + 6] = vertices[vertIndex + 2];
            vertices[vertIndex + 7] = vertices[vertIndex + 3];

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
                    roadTriangles[triIndex + j] = (vertIndex + triangleMap[j]) % vertices.Length;

                    // reverse triangle map for under road so that triangles wind the other way and are visible from underneath
                    underRoadTriangles[triIndex + j] = (vertIndex + triangleMap[triangleMap.Length - 1 - j] + 2) % vertices.Length;
                }
                for (int j = 0; j < sidesTriangleMap.Length; j++)
                    sideOfRoadTriangles[triIndex * 4 + j] = (vertIndex + sidesTriangleMap[j]) % vertices.Length;
            }

            // add to vert and tri index counters
            vertIndex += 8;
            triIndex += 6;
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.normals = normals;
        mesh.subMeshCount = 3;

        mesh.SetTriangles(roadTriangles, 0);
        mesh.SetTriangles(sideOfRoadTriangles, 1);
        mesh.SetTriangles(underRoadTriangles, 2);

        mesh.RecalculateBounds();

        return mesh;
    }
#endif
}

#if false

// An attempt at the revival of the old mesh generation code

public static Mesh CreateMesh(float desiredX, float desiredZ, float pieceX = 0.45f, float pieceZ = 0.5f, bool debug = false, float desiredY = .4f)
    {
        // Returned cached mesh if request is the same
        if (desiredX + desiredZ == lastDesiredValues) return cachedMesh;

        // Temp!
        if (Application.isPlaying)
        {
            //pieceX = RhythmicGame.TrackWidth / 8;
            pieceX = RhythmicGame.TrackWidth;
            pieceZ = desiredZ / 32;
        }

        // How many times to go through stuff
        int zSize = Mathf.RoundToInt(desiredZ / pieceZ);
        int xSize = Mathf.RoundToInt(desiredX / pieceX);
        int ySize = 1; // Y is only supposed to repeat once. (?)

        Vector3[] vertices = new Vector3[zSize * 8 + 8];
        Vector2[] uv = new Vector2[vertices.Length];
        Vector4[] tangents = new Vector4[vertices.Length];
        Vector3[] normals = new Vector3[vertices.Length];

        // Triangles:
        int numTris = 2 * zSize;
        int[] triangles_top = new int[numTris * 3];
        int[] underRoadTriangles = new int[numTris * 3];
        int[] sideOfRoadTriangles = new int[numTris * 4 * 3];

        int[] triangleMap = { 0, 8, 1, 1, 8, 9 };
        int[] sidesTriangleMap = {
           4, 6, 14,
           12, 4, 14,
           5, 15, 7,
           13, 15, 5,

           4, 12, 14,
           6, 4, 14,
           5, 15, 13,
           7, 15, 5 };

        // Generate mesh data
        {
            int vi = 0;
            int ti = 0;

            // Vertices & triangles:
            for (int i = 0; i < zSize; i++, vi += 8, ti += 6)
            {
                Vector3 side0 = new Vector3(0 * pieceX - desiredX / 2, 0, i * pieceZ);
                Vector3 side1 = new Vector3(1 * pieceX - desiredX / 2, 0, i * pieceZ);

                // Add vertices
                {
                    /* top */
vertices[vi] = side0;
vertices[vi + 1] = side1;

/* bottom */
vertices[vi + 2] = side0 - Vector3.up * desiredY;
vertices[vi + 3] = side1 - Vector3.up * desiredY;

/* sides */
vertices[vi + 4] = vertices[vi + 0];
vertices[vi + 5] = vertices[vi + 1];
vertices[vi + 6] = vertices[vi + 2];
vertices[vi + 7] = vertices[vi + 3];
                }

                // Add normals
                {
    /* Top of road normals */
    normals[vi + 0] = Vector3.up;
    normals[vi + 1] = Vector3.up;
    /* Bottom of road normals */
    normals[vi + 2] = -Vector3.up;
    normals[vi + 3] = -Vector3.up;
    /* Sides of road normals */
    normals[vi + 4] = -Vector3.right;
    normals[vi + 5] = Vector3.right;
    normals[vi + 6] = -Vector3.right;
    normals[vi + 7] = Vector3.right;
}

uv[vi] = new Vector2(0, side0.z);
uv[vi + 1] = new Vector2(1, side0.z);
tangents[i] = new Vector4(1f, 0f, 0f, -1f);

// Add triangles
{
    if (i < zSize - 1)
    {
        for (int t = 0; t < triangleMap.Length; t++)
        {
            triangles_top[ti + t] = (vi + triangleMap[t]) % vertices.Length;
            underRoadTriangles[ti + t] = (vi + triangleMap[triangleMap.Length - 1 - t] + 2) % vertices.Length;
        }
        for (int t = 0; t < sidesTriangleMap.Length; t++)
            sideOfRoadTriangles[ti * 4 + t] = (vi + sidesTriangleMap[t]) % vertices.Length;
    }

    /*
    triangles[ti] = vi;
    triangles[ti + 3] = triangles[ti + 2] = vi + 1;
    triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
    triangles[ti + 5] = vi + xSize + 2;
    */
}
            }
        }

        if (debug)
{
    Logger.Log(vertices, printIndex: true);
    //Logger.Log(triangles, printIndex: false);

    // Debug draw vertices
    if (GameObject.FindGameObjectWithTag("Remove") == null)
    {
        int vID = 0;
        foreach (Vector3 v in vertices)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.tag = "Remove";
            go.transform.position = v;
            go.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
            go.name = vID.ToString();
            vID++;
        }
    }
}

Mesh mesh = new Mesh() { name = "Generated mesh (cust)" };

mesh.vertices = vertices;
mesh.uv = uv;
mesh.normals = normals;
mesh.tangents = tangents;

mesh.subMeshCount = 2;
mesh.SetTriangles(triangles_top, 0);

int[] sideAndBottomTriangles = new int[sideOfRoadTriangles.Length + underRoadTriangles.Length];
underRoadTriangles.CopyTo(sideAndBottomTriangles, 0);
sideOfRoadTriangles.CopyTo(sideAndBottomTriangles, underRoadTriangles.Length);

mesh.SetTriangles(sideAndBottomTriangles, 1);

//mesh.RecalculateBounds();
mesh.RecalculateNormals();
return mesh;
    }
 
#endif