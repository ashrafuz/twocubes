using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlocksSpawner : MonoBehaviour {
    [SerializeField] TrackManager m_Track;
    [SerializeField] Block m_BlockPrefab;
    [SerializeField] float m_BlockSpawnDistance;
    [SerializeField] GameObject m_ReferenceBox;
    [SerializeField] int m_SpawnIntervalInPath = 20;
    [SerializeField] float m_SpawnPercentage = 60;
    [SerializeField] List<Block> m_InstantiatedBlocks;
    [SerializeField] float m_HideDistanceThreshold = 25;

    private void Start () {
        m_InstantiatedBlocks = new List<Block> ();
        GameEventManager.OnNewTrackGenerated += SpawnNextSet;
        SpawnNextSet (0);
    }

    public void SpawnNextSet (int _totalPooledPoints) {
        if (!m_Track.m_GameIsRunning) { return; }

        for (int i = 0; i < m_InstantiatedBlocks.Count; i++) {
            if (Vector3.Distance (m_ReferenceBox.transform.position, m_InstantiatedBlocks[i].transform.position) > m_HideDistanceThreshold) {
                m_InstantiatedBlocks[i].gameObject.SetActive (false);
            }
        }

        List<Vector2> pathPoints = m_Track.GetPathPoints ();

        int t = (int) (pathPoints.Count * 0.2f);
        for (int i = pathPoints.Count - 5; i > t; i -= m_SpawnIntervalInPath) {
            int random = Random.Range (0, 100);
            if (random < m_SpawnPercentage) {
                Vector2 pointToSpawn = new Vector2 ();
                float angleWithNextPoint = Vector2.SignedAngle (Vector2.right, (pathPoints[i + 1] - pathPoints[i]));
                if (angleWithNextPoint < 0) {
                    angleWithNextPoint = 360 + angleWithNextPoint;
                }
                pointToSpawn.x = Mathf.Cos (Mathf.Deg2Rad * (angleWithNextPoint + 90)) * (TrackManager.PathRadius * m_BlockSpawnDistance);
                pointToSpawn.y = Mathf.Sin (Mathf.Deg2Rad * (angleWithNextPoint + 90)) * (TrackManager.PathRadius * m_BlockSpawnDistance);

                pointToSpawn += pathPoints[i];

                float distanceFromCamera = Vector3.Distance (pointToSpawn, m_ReferenceBox.transform.position);

                if (Mathf.Abs (distanceFromCamera) < m_HideDistanceThreshold) {
                    Debug.Log ("too close spawn point, skiping " + distanceFromCamera);
                    continue;
                }
                SpawnBlock (pointToSpawn);
            }
        }
    }

    private void SpawnBlock (Vector3 _pos) {
        int idleBoxAvailableAt = -1;
        for (int i = 0; i < m_InstantiatedBlocks.Count; i++) {
            if (!m_InstantiatedBlocks[i].gameObject.activeInHierarchy) {
                idleBoxAvailableAt = i;
                break;
            }
        }

        Block newBlock = idleBoxAvailableAt > -1 ? m_InstantiatedBlocks[idleBoxAvailableAt] :
            Instantiate (m_BlockPrefab, _pos, Quaternion.identity);

        newBlock.transform.SetParent (this.transform);
        newBlock.SetRandomType ();
        newBlock.transform.position = _pos;
        newBlock.gameObject.SetActive (true);

        if (idleBoxAvailableAt == -1) {
            m_InstantiatedBlocks.Add (newBlock);
        }

    }
}