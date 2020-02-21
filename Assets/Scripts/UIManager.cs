using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour {

    public int m_CurrentPoint;
    public int m_CurrentLife;

    private int mPreviousHighScore;
    private const string HIGH_SCORE_KEY = "highscore";

    [SerializeField] private TextMeshProUGUI m_LifeText;
    [SerializeField] private TextMeshProUGUI m_PointsText;
    [SerializeField] private GameObject m_GameOverPanel;
    [SerializeField] private Button m_RestartButton;
    [SerializeField] private Button m_ExitBtn;
    [SerializeField] private GameObject m_HighscorePanel;
    [SerializeField] private TextMeshProUGUI m_HighScoreText;
    [SerializeField] private TextMeshProUGUI m_TitleHighScoreText;

    [SerializeField] private List<Image> m_LifeImages;

    void Start () {

        mPreviousHighScore = PlayerPrefs.GetInt (HIGH_SCORE_KEY, 0);

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
        if (m_CurrentLife <= 0) { ShowGameOver (); }
        UpdateLifeUI ();
    }

    private void UpdateLifeUI () {
        for (int i = m_LifeImages.Count; i > m_CurrentLife; i--) {
            m_LifeImages[i - 1].transform.DOScale (0, 0.5f);
        }
        m_LifeText.text = m_CurrentLife.ToString ();
    }

    private void ShowGameOver () {
        m_CurrentLife = 0;
        m_GameOverPanel.gameObject.SetActive (true);
        m_HighscorePanel.SetActive (true);
        GameEventManager.OnNoMoreLivesLeft?.Invoke ();

        if (m_CurrentPoint > mPreviousHighScore) {
            PlayerPrefs.SetInt (HIGH_SCORE_KEY, m_CurrentPoint);
            m_HighScoreText.text = m_CurrentPoint.ToString ();
            m_TitleHighScoreText.text = "New Highscore!!";
        } else {
            m_HighScoreText.text = m_CurrentPoint.ToString ();
            m_TitleHighScoreText.text = "Your Score";
        }
    }

}