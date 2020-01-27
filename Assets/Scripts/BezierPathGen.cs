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

    [SerializeField] List<Vector2> m_PathRingPoints;

    [SerializeField] static int m_FaceDetailLevel = 8;

    internal List<Vector2> GetPathPoints () {
        return m_PathRingPoints;
    }

    [SerializeField] DataAsset m_MeshData;
    [SerializeField][Range (2, 256)] int m_TotalCycle = 10;

    [SerializeField] int m_MeshDetailLevelPerCheckpoint = 4;
    [SerializeField] float m_MaxAmplitude = 5;
    [SerializeField] float m_PathSpan = 100;
    private float m_CurrentShiftX = 0;
    private int currentSegmentIndex = 0;

    private void Awake () {
        SetUpdateRateInSeconds (5);
        ClearPath ();
        GenerateNewPath ();
        GenerateMesh ();
    }

    protected override void SlowUpdate () {
        GenerateNewPath ();
        GenerateMesh ();
    }

    private void ClearPath () {
        m_PathRingPoints?.Clear ();
        m_PathRingPoints = new List<Vector2> ();
    }

    private void GenerateNewPath () {
        Vector2[] randomPoints = SetupRandomPathPoints ();

        currentSegmentIndex++;
        if (currentSegmentIndex >= 3) { // pooling
            //Debug.Log ("pooling");
            m_PathRingPoints.RemoveRange (0, (randomPoints.Length / 2) * m_MeshDetailLevelPerCheckpoint);
            currentSegmentIndex = 0;
        }

        MakeBezierCurveAlongPath (randomPoints);
    }

    private void GenerateMesh () {
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

        Mesh newMesh = new Mesh ();
        newMesh.SetVertices (vertexList);
        newMesh.SetTriangles (triangleIndices, 0);
        newMesh.SetNormals (normalList);

        MeshFilter mf = GetComponent<MeshFilter> ();
        mf.sharedMesh.Clear ();
        mf.sharedMesh = newMesh;

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
    }

    // private void OnDrawGizmosSelected () {
    //     if (m_Mesh == null) {
    //         m_Mesh = new Mesh ();
    //         m_Mesh.name = "BezierPath";
    //         GetComponent<MeshFilter> ().sharedMesh = m_Mesh;
    //     }
    //     GeneratePath ();
    //     GenerateMesh ();

    //     for (int i = 0; i < m_PathRingPoints.Count; i++) {
    //         Gizmos.DrawSphere (m_PathRingPoints[i], 0.2f);
    //     }
    // }

    private Vector2[] SetupRandomPathPoints () {
        int randPointArraySize = ((m_TotalCycle * 3) - (m_TotalCycle - 1));
        Vector2[] randPoints = new Vector2[randPointArraySize];
        int alternator = 1;
        for (int i = 0; i < randPointArraySize; i++) {
            float t = i / (float) (randPointArraySize - 1);
            Vector2 randVec = new Vector2 ();
            randVec.x = m_CurrentShiftX + m_PathSpan * t;
            if (i % 2 == 1) {
                randVec.y = m_MaxAmplitude * UnityEngine.Random.Range (0.5f, 1) * alternator;
                alternator = -alternator;
            } else {
                randVec.y = 0; //UnityEngine.Random.Range (0, 10f) * alternator;
            }

            randPoints[i] = randVec;
        }

        m_CurrentShiftX = randPoints[randPoints.Length - 1].x;
        return randPoints;
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