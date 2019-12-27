using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshRenderer))]
public class MeshGen : MonoBehaviour {

    [Header ("Path Variables")]
    [SerializeField] float m_WaveAmplitude = 1;
    [SerializeField] float m_Frequency = 1;
    [SerializeField][Range (0, 100)] float m_Distance = 10;
    [SerializeField][Range (8, 128)] int m_StepValue = 8;
    [SerializeField] List<Vector3> m_PathPoints = new List<Vector3> ();

    [Space][Header ("Mesh Data")]

    [SerializeField][Range (8, 128)] int m_FaceDetailLevel = 8;
    [SerializeField] float m_Radius = 5;

    private Mesh m_Mesh;
    private DataAsset m_MeshData;

    private void Init () {
        if (m_Mesh == null) {
            m_Mesh = new Mesh ();

            m_Mesh.name = "Path";
            GetComponent<MeshFilter> ().sharedMesh = m_Mesh;
        }

        if (!File.Exists (GameConstants.MESH_DATA_FULL_PATH)) {
            CreateNewMeshData ();
        } else {
            m_MeshData = m_MeshData == null? AssetDatabase.LoadAssetAtPath<DataAsset> (GameConstants.MESH_DATA_FULL_PATH) : m_MeshData;
        }
    }

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

    int lastPathPointsCount = 0;
    private void OnDrawGizmosSelected () {
        Init ();
        GeneratePath ();
        if (lastPathPointsCount != m_PathPoints.Count) {
            CreateNewMeshData ();
            lastPathPointsCount = m_PathPoints.Count;
        }
        GenerateMesh (m_MeshData, m_PathPoints);
    }

    private void GeneratePath () {
        m_PathPoints.Clear ();

        float currentAmplitude = m_WaveAmplitude;
        float currentFrequency = m_Frequency;
        for (int i = 0; i < m_StepValue; i++) {
            Gizmos.color = Color.grey;
            float t = i / (float) m_StepValue;
            float x = m_Distance * t;
            float y = currentAmplitude * Mathf.Sin (currentFrequency * x);

            m_PathPoints.Add (new Vector3 (0, y, x));

            Gizmos.DrawSphere (m_PathPoints[m_PathPoints.Count - 1], 0.21f);
            Gizmos.color = Color.white;
            Gizmos.DrawSphere (new Vector3 (0, 0, x), 0.1f);
        }
    }

    private void GenerateMesh (DataAsset _da, List<Vector3> _path) {
        m_Mesh.Clear ();

        List<Vector3> vertexList = new List<Vector3> ();
        List<Vector3> normalList = new List<Vector3> ();
        List<int> triangleIndices = new List<int> ();

        int meshDetailLevel = _path.Count;

        for (int ring = 0; ring < meshDetailLevel; ring++) {
            float t = ring / (float) meshDetailLevel;
            Vector3 rootPoint = _path[ring];
            Vector3 nextPoint = (ring != meshDetailLevel - 1) ? _path[ring + 1] : _path[ring]; //last point looks to itself

            MeshPoint mp = new MeshPoint ();
            mp.position = rootPoint;
            mp.rotation = Quaternion.LookRotation (nextPoint - rootPoint);

            for (int i = 0; i < m_MeshData.vertices.Length; i++) {
                vertexList.Add (mp.LocalToWorld (m_MeshData.vertices[i].points));
                normalList.Add (mp.LocalToWorldNormal (m_MeshData.vertices[i].normals));

                Gizmos.DrawSphere (vertexList[vertexList.Count - 1], 0.1f);
            }
        }

        //triangles
        for (int ring = 0; ring < meshDetailLevel - 1; ring++) {
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
    }

}