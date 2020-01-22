using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlocksSpawner : MonoBehaviour {
    [SerializeField] BezierPathGen m_GeneratedMesh;
    [SerializeField] Block m_BlockPrefab;

    [SerializeField] List<Block> m_InstantiatedBlocks;

    void Start () {
        m_InstantiatedBlocks = new List<Block> ();

        List<Vector2> pathPoints = new List<Vector2> ();
        pathPoints = m_GeneratedMesh.GetPathPoints ();

        int t = (int) (pathPoints.Count * 0.15f);

        for (int i = t; i < pathPoints.Count; i += 3) {
            int random = Random.Range (0, 100);
            if (random >= 50) {
                SpawnBlock (pathPoints[i]);
            }
        }
    }

    private void SpawnBlock (Vector3 _pos) { //TODO, might want to take account of rotation
        Block newBlock = Instantiate (
            m_BlockPrefab,
            new Vector3 (_pos.x, _pos.y + m_GeneratedMesh.m_Radius * 1.2f, _pos.z),
            Quaternion.identity
        );
        newBlock.transform.SetParent (this.transform);
        newBlock.SetRandomType ();

        m_InstantiatedBlocks.Add (newBlock);
    }
}