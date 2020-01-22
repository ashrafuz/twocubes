using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AUZ_UTIL;
using UnityEditor;
using System;
using System.IO;

[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshRenderer))]
public class BezierPathGen : SlowMono {
    [Range (1, 10)] public float m_Radius;

    [SerializeField] private Vector2[] m_InitialRandomPoints;
    [SerializeField] private List<Vector2> m_PathRingPoints;
    [SerializeField][Range (8, 128)] private int m_FaceDetailLevel;

    internal List<Vector2> GetPathPoints () {
        return m_PathRingPoints;
    }

    [SerializeField] private DataAsset m_MeshData;
    private Mesh m_Mesh;
    [SerializeField][Range (2, 256)] int m_TotalCycle = 10;

    [SerializeField] int m_MeshDetailLevelPerCheckpoint = 4;
    [SerializeField] float m_MaxAmplitude = 5;
    [SerializeField] float m_PathSpan = 100;

    private void Awake () {
        SetUpdateRateInSeconds (5);
        GeneratePath ();
        GenerateMesh ();
    }

    private void GeneratePath () {
        if (m_Mesh == null) {
            m_Mesh = new Mesh ();

            m_Mesh.name = "BezierPath";
            GetComponent<MeshFilter> ().sharedMesh = m_Mesh;
        }

        SetupRandomPathPoints ();
        MakeBezierCurveAlongPath ();
        if (!File.Exists (GameConstants.MESH_DATA_FULL_PATH)) {
            CreateNewMeshData ();
        } else {
            m_MeshData = m_MeshData == null? AssetDatabase.LoadAssetAtPath<DataAsset> (GameConstants.MESH_DATA_FULL_PATH) : m_MeshData;
        }
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

            if (nextRootPointIndex == ring) {
                nextRootPoint = nextRootPoint * 1.01f; // to avoid look rotation log
            }

            MeshPoint mp = new MeshPoint ();
            mp.position = rootPoint;
            mp.rotation = Quaternion.LookRotation (nextRootPoint - rootPoint);

            //Gizmos.DrawSphere (mp.position, 0.5f);

            for (int i = 0; i < m_MeshData.vertices.Length; i++) {
                vertexList.Add (mp.LocalToWorld (m_MeshData.vertices[i].points));
                normalList.Add (mp.LocalToWorldNormal (m_MeshData.vertices[i].normals));
            }
        }

        for (int ring = 0; ring < m_PathRingPoints.Count - 1; ring++) {
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

    private void CreateNewMeshData () {
        m_MeshData = ScriptableObject.CreateInstance<DataAsset> ();

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
        }

        List<int> lineIndices = new List<int> ();
        for (int i = 0; i < m_FaceDetailLevel * 2; i++) {
            lineIndices.Add ((i + 1) % (m_FaceDetailLevel * 2));
        }

        m_MeshData.vertices = vertices.ToArray ();
        m_MeshData.lineIndices = lineIndices.ToArray ();

        AssetDatabase.CreateAsset (m_MeshData, GameConstants.MESH_DATA_FULL_PATH);
    }

    private Vector2 GetBezierPointsByHandle (Vector2 _p0, Vector2 _p1, Vector2 _p2, float _t) {
        Vector2 s0 = Vector2.Lerp (_p0, _p1, _t);
        Vector2 s1 = Vector2.Lerp (_p1, _p2, _t);
        Vector2 m0 = Vector2.Lerp (s0, s1, _t);
        return m0;
    }

    private void MakeBezierCurveAlongPath () {
        Gizmos.color = Color.red;
        m_PathRingPoints = new List<Vector2> ();
        for (int i = 0; i < m_InitialRandomPoints.Length - 2; i += 2) {
            for (int j = 0; j < m_MeshDetailLevelPerCheckpoint; j++) {
                float t = j / (float) m_MeshDetailLevelPerCheckpoint;
                m_PathRingPoints.Add (GetBezierPointsByHandle (
                    m_InitialRandomPoints[i],
                    m_InitialRandomPoints[i + 1],
                    m_InitialRandomPoints[i + 2],
                    t
                ));
            }
        }

        m_PathRingPoints.Add (m_InitialRandomPoints[m_InitialRandomPoints.Length - 1]);
    }

    private void SetupRandomPathPoints () {
        int randPointArraySize = ((m_TotalCycle * 3) - (m_TotalCycle - 1));
        m_InitialRandomPoints = new Vector2[randPointArraySize];
        int alternator = 1;
        for (int i = 0; i < m_InitialRandomPoints.Length; i++) {
            float t = i / (float) m_InitialRandomPoints.Length;
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
    }

    private void OnDrawGizmosSelected () {
        //GeneratePath ();
        //GenerateMesh ();

        // Gizmos.color = Color.green;
        // for (int i = 0; i < m_InitialRandomPoints.Length; i++) {
        //     Gizmos.DrawSphere (m_InitialRandomPoints[i], 0.5f);
        // }

        // Gizmos.color = Color.red;
        // for (int i = 0; i < m_PathRingPoints.Count; i++) {
        //     Gizmos.DrawSphere (m_PathRingPoints[i], 0.3f);
        // }
    }

    protected override void SlowUpdate () {
        // GeneratePath ();
        // GenerateMesh ();
    }
}