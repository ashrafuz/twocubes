using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AUZ_UTIL;
using UnityEditor;
using UnityEngine;

[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshRenderer))]
public class BezierPathGen : SlowMono {
    public static float m_Radius = 2;
    public bool m_DebugPath = false;

    [SerializeField] Vector2[] m_InitialRandomPoints;
    [SerializeField] Vector2[] m_NextRandomPoints;

    [SerializeField] List<Vector2> m_PathRingPoints;

    [SerializeField] static int m_FaceDetailLevel = 8;

    internal List<Vector2> GetPathPoints () {
        return m_PathRingPoints;
    }

    [SerializeField] DataAsset m_MeshData;
    private Mesh m_Mesh;
    [SerializeField][Range (2, 256)] int m_TotalCycle = 10;

    [SerializeField] int m_MeshDetailLevelPerCheckpoint = 4;
    [SerializeField] float m_MaxAmplitude = 5;
    [SerializeField] float m_PathSpan = 100;

    private void Awake () {
        m_Mesh = new Mesh ();
        m_Mesh.name = "BezierPath";
        GetComponent<MeshFilter> ().sharedMesh = m_Mesh;

        SetUpdateRateInSeconds (5);
        GeneratePath ();
        GenerateMesh ();
    }

    private void GeneratePath () {
        SetupRandomPathPoints ();
        m_PathRingPoints?.Clear ();
        m_PathRingPoints = new List<Vector2> ();

        MakeBezierCurveAlongPath (m_InitialRandomPoints);
        MakeBezierCurveAlongPath (m_NextRandomPoints);
    }

    private void GenerateMesh () {
        m_Mesh.Clear ();

        List<Vector3> vertexList = new List<Vector3> ();
        List<Vector3> normalList = new List<Vector3> ();
        List<int> triangleIndices = new List<int> ();

        //Debug.Log ("total mesh detail level :: " + m_PathRingPoints.Count);
        for (int ring = 0; ring < m_PathRingPoints.Count; ring++) {
            //float t = ring / (float) m_TotalMeshDetail;

            Vector3 rootPoint = m_PathRingPoints[ring];
            int nextRootPointIndex = (ring != m_PathRingPoints.Count - 1) ? ring + 1 : ring;
            Vector3 nextRootPoint = m_PathRingPoints[nextRootPointIndex];

            MeshPoint mp = new MeshPoint ();
            mp.position = rootPoint;
            mp.rotation = (rootPoint == nextRootPoint) ? Quaternion.identity : Quaternion.LookRotation (nextRootPoint - rootPoint);

            //Gizmos.DrawSphere (mp.position, 0.5f);

            for (int i = 0; i < m_MeshData.vertices.Length; i++) {
                vertexList.Add (mp.LocalToWorld (m_MeshData.vertices[i].points));
                normalList.Add (mp.LocalToWorldNormal (m_MeshData.vertices[i].normals));
            }
        }

        for (int ring = 0; ring < m_PathRingPoints.Count - 2; ring++) {
            int rootIndex = ring * m_MeshData.vertices.Length;
            int rootIndexNext = (ring + 1) * m_MeshData.vertices.Length;

            for (int line = 0; line < m_MeshData.lineIndices.Length; line += 2) {
                int line0 = m_MeshData.lineIndices[line];
                int line1 = m_MeshData.lineIndices[line + 1];

                int currentA = rootIndex + line0;
                int currentB = rootIndex + line1;

                int nextA = rootIndexNext + line0;
                int nextB = rootIndexNext + line1;

                triangleIndices.Add (currentA);
                triangleIndices.Add (nextB);
                triangleIndices.Add (nextA);

                triangleIndices.Add (nextB);
                triangleIndices.Add (currentA);
                triangleIndices.Add (currentB);

                //opposite face
                triangleIndices.Add (currentA);
                triangleIndices.Add (nextA);
                triangleIndices.Add (nextB);

                triangleIndices.Add (nextB);
                triangleIndices.Add (currentB);
                triangleIndices.Add (currentA);
            }
        }

        m_Mesh.SetVertices (vertexList);
        m_Mesh.SetTriangles (triangleIndices, 0);
        m_Mesh.SetNormals (normalList);

    } //generatemesh

    private Vector2 GetBezierPointsByHandle (Vector2 _p0, Vector2 _p1, Vector2 _p2, float _t) {
        Vector2 s0 = Vector2.Lerp (_p0, _p1, _t);
        Vector2 s1 = Vector2.Lerp (_p1, _p2, _t);
        Vector2 m0 = Vector2.Lerp (s0, s1, _t);
        return m0;
    }

    private void MakeBezierCurveAlongPath (Vector2[] _randomPoints) {
        for (int i = 0; i < _randomPoints.Length - 2; i += 2) {
            for (int j = 0; j < m_MeshDetailLevelPerCheckpoint; j++) {
                float t = j / (float) m_MeshDetailLevelPerCheckpoint;
                m_PathRingPoints.Add (GetBezierPointsByHandle (
                    _randomPoints[i],
                    _randomPoints[i + 1],
                    _randomPoints[i + 2],
                    t
                ));
            }
        }
        //m_PathRingPoints.Add (_randomPoints[_randomPoints.Length - 1]);
    }

    private void OnDrawGizmosSelected () {
        if (m_Mesh == null) {
            m_Mesh = new Mesh ();
            m_Mesh.name = "BezierPath";
            GetComponent<MeshFilter> ().sharedMesh = m_Mesh;
        }
        GeneratePath ();
        GenerateMesh ();

        for (int i = 0; i < m_PathRingPoints.Count; i++) {
            Gizmos.DrawSphere (m_PathRingPoints[i], 0.2f);
        }
    }

    private void SetupRandomPathPoints () {
        int randPointArraySize = ((m_TotalCycle * 3) - (m_TotalCycle - 1));
        m_InitialRandomPoints = new Vector2[randPointArraySize];
        m_NextRandomPoints = new Vector2[randPointArraySize];
        int alternator = 1;
        for (int i = 0; i < randPointArraySize; i++) {
            float t = i / (float) (randPointArraySize - 1);
            Vector2 randVec = new Vector2 ();
            randVec.x = m_PathSpan * t;
            if (i % 2 == 1) {
                randVec.y = m_MaxAmplitude * UnityEngine.Random.Range (0.5f, 1) * alternator;
                alternator = -alternator;
            } else {
                randVec.y = 0; //UnityEngine.Random.Range (0, 10f) * alternator;
            }

            m_InitialRandomPoints[i] = randVec;
        }

        m_NextRandomPoints[0] = m_InitialRandomPoints[randPointArraySize - 1];
        for (int i = 1; i < randPointArraySize; i++) {
            Vector2 randVec = m_InitialRandomPoints[i];
            randVec.x += m_PathSpan;
            if (randVec.y != 0) {
                randVec.y = m_MaxAmplitude * UnityEngine.Random.Range (0.5f, 1) * alternator;
                alternator = -alternator;
            }

            m_NextRandomPoints[i] = randVec;
        }
    }

    protected override void SlowUpdate () {
        //Debug.Log ("running slow update");
        GeneratePath ();
        GenerateMesh ();
    }

    [MenuItem ("Tools/CreateNewMeshData")]
    public static void CreateNewMeshData () {
        DataAsset meshData = ScriptableObject.CreateInstance<DataAsset> ();

        List<Vertex> vertices = new List<Vertex> ();

        for (int i = 0; i < m_FaceDetailLevel; i++) {
            float t = i / (float) m_FaceDetailLevel;
            float angleInRad = t * GameMath.TAU;

            Vertex currentPoint = new Vertex ();
            currentPoint.points.x = Mathf.Cos (angleInRad) * m_Radius;
            currentPoint.points.y = Mathf.Sin (angleInRad) * m_Radius;

            currentPoint.normals = currentPoint.points.normalized;
            vertices.Add (currentPoint);

            // Gizmos.color = Color.red;
            // Gizmos.DrawSphere (currentPoint.points, 0.2f);

            //2nd point, culling off
            Vertex oppPoint = new Vertex ();
            oppPoint.points = currentPoint.points * 0.99f; //slightly changed value because of Z level culling
            oppPoint.normals = -currentPoint.normals;

            vertices.Add (oppPoint);

            //Debug.Log ("angle in rad " + currentPoint.points + ", for " + i);
        }

        List<int> lineIndices = new List<int> ();
        for (int i = 0; i < m_FaceDetailLevel * 2; i++) {
            lineIndices.Add ((i + 1) % (m_FaceDetailLevel * 2));
        }

        meshData.vertices = vertices.ToArray ();
        meshData.lineIndices = lineIndices.ToArray ();

        if (File.Exists (GameConstants.MESH_DATA_FULL_PATH)) {
            File.Delete (GameConstants.MESH_DATA_FULL_PATH);
        }

        AssetDatabase.CreateAsset (meshData, GameConstants.MESH_DATA_FULL_PATH);
    }

}