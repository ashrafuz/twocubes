using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlocksSpawner : MonoBehaviour {
    [SerializeField] TrackManager m_GeneratedMesh;
    [SerializeField] Block m_BlockPrefab;

    [SerializeField] List<Block> m_InstantiatedBlocks;

    public void SpawnNextSet () {
        m_InstantiatedBlocks = new List<Block> ();
        List<Vector2> pathPoints = m_GeneratedMesh.GetPathPoints ();

        int t = (int) (pathPoints.Count * 0.15f);
        for (int i = t; i < pathPoints.Count - 10; i += 10) {
            int random = Random.Range (0, 100);
            if (random >= 50) {

                Vector2 pointToSpawn = new Vector2 ();
                float angleWithNextPoint = Vector2.SignedAngle (Vector2.right, (pathPoints[i + 1] - pathPoints[i]));
                if (angleWithNextPoint < 0) {
                    angleWithNextPoint = 360 + angleWithNextPoint;
                }
                pointToSpawn.x = Mathf.Cos (Mathf.Deg2Rad * (angleWithNextPoint + 90)) * (TrackManager.PathRadius * 1.2f);
                pointToSpawn.y = Mathf.Sin (Mathf.Deg2Rad * (angleWithNextPoint + 90)) * (TrackManager.PathRadius * 1.2f);

                pointToSpawn += pathPoints[i];
                SpawnBlock (pointToSpawn);
            }
        }
    }

    private void SpawnBlock (Vector3 _pos) {
        Block newBlock = Instantiate (m_BlockPrefab, _pos, Quaternion.identity);
        newBlock.transform.SetParent (this.transform);
        newBlock.SetRandomType ();

        m_InstantiatedBlocks.Add (newBlock);
    }
}