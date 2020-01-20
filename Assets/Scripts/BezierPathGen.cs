using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AUZ_UTIL;
using System;

[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshRenderer))]
public class BezierPathGen : SlowMono {
    [SerializeField] private Vector2[] m_InitialRandomPoints;
    [SerializeField] private List<Vector2> m_PathRingPoints;
    private Mesh m_Mesh;
    [SerializeField][Range (2, 256)] int m_TotalCycle = 10;
    [SerializeField] int m_MeshDetailLevelPerCheckpoint = 4;
    [SerializeField] float m_MaxAmplitude = 5;
    [SerializeField] float m_PathSpan = 100;

    private void Awake () {
        SetUpdateRateInSeconds (5);
    }

    private void GeneratePath () {
        SetupRandomPathPoints ();
        MakeBezierCurveAlongPath ();
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
        int totalSize = ((m_TotalCycle * 3) - (m_TotalCycle - 1));
        m_InitialRandomPoints = new Vector2[totalSize];
        int alternator = 1;
        for (int i = 0; i < m_InitialRandomPoints.Length; i++) {
            float t = i / (float) m_InitialRandomPoints.Length;
            Vector2 randVec = new Vector2 ();
            randVec.x = m_PathSpan * t;
            if (i % 2 == 1) {
                randVec.y = m_MaxAmplitude * UnityEngine.Random.Range (0.5f, 1) * alternator;
                alternator = -alternator;
            } else {
                randVec.y = 0;
            }
            m_InitialRandomPoints[i] = randVec;
        }
    }

    private void OnDrawGizmos () {
        if (m_InitialRandomPoints == null) {
            return;
        }

        Gizmos.color = Color.green;
        for (int i = 0; i < m_InitialRandomPoints.Length; i++) {
            Gizmos.DrawSphere (m_InitialRandomPoints[i], 0.5f);
        }

        if (m_PathRingPoints == null) {
            return;
        }

        Gizmos.color = Color.red;
        for (int i = 0; i < m_PathRingPoints.Count; i++) {
            Gizmos.DrawSphere (m_PathRingPoints[i], 0.3f);
        }
    }

    protected override void SlowUpdate () {
        GeneratePath ();
    }
}