using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxController : MonoBehaviour {

    [SerializeField] List<GameObject> m_BoxList = new List<GameObject> ();
    [SerializeField] MeshGen m_GeneratedMesh;
    [SerializeField] float m_Speed = 2;
    [SerializeField] float m_RotSpeed = 3;

    private List<Vector3> m_PathPoints;
    private int m_CurrentPathIndex = 0;

    private float m_CurrentAngle = 0;

    private void Start () {
        m_PathPoints = new List<Vector3> ();
        m_PathPoints = m_GeneratedMesh.GetPathPoints ();

        m_CurrentPathIndex = 0;
        m_CurrentAngle = 0;
        transform.position = m_PathPoints[m_CurrentPathIndex];

        for (int i = 0; i < m_BoxList.Count; i++) {
            float t = i / (float) m_BoxList.Count;
            float angleInRad = t * GameMath.TAU + (GameMath.TAU / 4); //to offset with 90
            m_BoxList[i].transform.localPosition = GameMath.GetPositionWithRadius (transform.position, m_GeneratedMesh.m_Radius * 1.5f, angleInRad);
        }

    }

    private void Update () {
        if (m_CurrentPathIndex < m_PathPoints.Count - 2) {
            MoveForward ();
            RotateBoxes ();

            if (Input.GetMouseButton (0)) {
                m_CurrentAngle += Time.deltaTime * m_RotSpeed;
            } else {
                m_CurrentAngle = Mathf.Clamp (m_CurrentAngle - (Time.deltaTime * m_RotSpeed), 0, m_CurrentAngle);
            }

        }
    }

    private void RotateBoxes () {
        float firstAngle = m_CurrentAngle + (GameMath.TAU / 4);
        float secondAngle = firstAngle + GameMath.TAU * 0.5f;

        m_BoxList[0].transform.localPosition = GameMath.GetPositionWithRadius (
            Vector3.zero, m_GeneratedMesh.m_Radius * 1.5f, firstAngle
        );
        m_BoxList[1].transform.localPosition = GameMath.GetPositionWithRadius (
            Vector3.zero, m_GeneratedMesh.m_Radius * 1.5f, secondAngle
        );

        if (firstAngle >= (GameMath.TAU + (GameMath.TAU / 4))) { //one full circle
            m_CurrentAngle = 0;
        }
    }

    private void MoveForward () {
        Vector3 direction = m_PathPoints[m_CurrentPathIndex + 1] - m_PathPoints[m_CurrentPathIndex];
        transform.position = transform.position + direction.normalized * m_Speed * Time.deltaTime;

        transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation (direction.normalized), m_Speed * Time.deltaTime);

        if (transform.position.z >= (m_PathPoints[m_CurrentPathIndex + 1].z * 0.99f)) {
            m_CurrentPathIndex = m_CurrentPathIndex + 1;
        }
    }

}