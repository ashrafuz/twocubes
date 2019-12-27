using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGen : MonoBehaviour {

    [SerializeField] PointsGenerator m_PointGen;

    private void OnDrawGizmosSelected () {
        if (m_PointGen == null)
            return;

        List<Vector3> points = m_PointGen.GetPoints();
    }
}