using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour {

    public int m_CurrentPoint;
    public int m_CurrentLife;

    [SerializeField] private TextMeshProUGUI m_LifeText;
    [SerializeField] private TextMeshProUGUI m_PointsText;
    [SerializeField] private GameObject m_GameOverPanel;
    [SerializeField] private Button m_RestartButton;
    [SerializeField] private Button m_ExitBtn;

    void Start () {
        GameEventManager.OnRightCollide += PointUp;
        GameEventManager.OnWrongCollide += PointDown;
        m_PointsText.text = m_CurrentPoint.ToString ();
        m_LifeText.text = m_CurrentLife.ToString ();

        m_RestartButton.onClick.RemoveAllListeners ();
        m_RestartButton.onClick.AddListener (() => {
            SceneManager.LoadScene (0, LoadSceneMode.Single);
        });

        m_ExitBtn.onClick.RemoveAllListeners ();
        m_ExitBtn.onClick.AddListener (() => {
            Application.Quit ();
        });
    }

    private void PointUp (Vector3 _collidPos) {
        m_CurrentPoint += 10;
        m_PointsText.text = m_CurrentPoint.ToString ();
    }

    private void PointDown (Vector3 __collidPos) {
        m_CurrentLife--;
        if (m_CurrentLife <= 0) {
            m_CurrentLife = 0;
            m_GameOverPanel.gameObject.SetActive (true);
            GameEventManager.OnNoMoreLivesLeft?.Invoke ();
        }
        m_LifeText.text = m_CurrentLife.ToString ();
    }

}