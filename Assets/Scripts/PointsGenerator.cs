using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointsGenerator : MonoBehaviour {

    [SerializeField] float m_WaveAmplitude = 1;
    [SerializeField] float m_Frequency = 1;
    [SerializeField][Range (0, 100)] float m_Distance = 10;
    [SerializeField][Range (8, 128)] int m_StepValue = 8;

    [SerializeField] private List<Vector3> m_PointsAlongPath = new List<Vector3> ();

    private void OnDrawGizmosSelected () {
        
        m_PointsAlongPath.Clear ();

        float currentAmplitude = m_WaveAmplitude;
        float currentFrequency = m_Frequency;
        for (int i = 0; i < m_StepValue; i++) {
            Gizmos.color = Color.grey;
            float t = i / (float) m_StepValue;
            float x = m_Distance * t;
            float y = currentAmplitude * Mathf.Sin (currentFrequency * x);

            m_PointsAlongPath.Add (new Vector3 (0, y, x));

            Gizmos.DrawSphere (m_PointsAlongPath[m_PointsAlongPath.Count - 1], 0.21f);
            Gizmos.color = Color.white;
            Gizmos.DrawSphere (new Vector3(0,0,x), 0.1f);
        }
    }

    public List<Vector3> GetPoints(){
        return m_PointsAlongPath;
    }

}