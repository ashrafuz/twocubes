using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlocksSpawner : MonoBehaviour {
    [SerializeField] TrackManager m_Track;
    [SerializeField] Block m_BlockPrefab;
    [SerializeField] List<Block> m_InstantiatedBlocks;

    private Camera m_MainCam;

    private void Start () {
        m_MainCam = Camera.main;
        m_InstantiatedBlocks = new List<Block> ();
        GameEventManager.OnNewTrackGenerated += SpawnNextSet;
        SpawnNextSet (0);
    }

    public void SpawnNextSet (int _totalPooledPoints) {
        if (!m_Track.m_GameIsRunning) { return; }

        for (int i = 0; i < m_InstantiatedBlocks.Count; i++) {
            //if camera passed my position, hide it
            if (m_MainCam.transform.position.x - 50 > m_InstantiatedBlocks[i].transform.position.x) {
                m_InstantiatedBlocks[i].gameObject.SetActive (false);
            }
        }

        List<Vector2> pathPoints = m_Track.GetPathPoints ();

        int t = (int) (pathPoints.Count * 0.2f);
        for (int i = t; i < pathPoints.Count - 10; i += 10) {
            int random = Random.Range (0, 100);
            if (random >= 50) {
                Vector2 pointToSpawn = new Vector2 ();
                float angleWithNextPoint = Vector2.SignedAngle (Vector2.right, (pathPoints[i + 1] - pathPoints[i]));
                if (angleWithNextPoint < 0) {
                    angleWithNextPoint = 360 + angleWithNextPoint;
                }
                pointToSpawn.x = Mathf.Cos (Mathf.Deg2Rad * (angleWithNextPoint + 90)) * (TrackManager.PathRadius * 1.5f);
                pointToSpawn.y = Mathf.Sin (Mathf.Deg2Rad * (angleWithNextPoint + 90)) * (TrackManager.PathRadius * 1.5f);

                pointToSpawn += pathPoints[i];
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
        newBlock.gameObject.SetActive (true);

        m_InstantiatedBlocks.Add (newBlock);
    }
}