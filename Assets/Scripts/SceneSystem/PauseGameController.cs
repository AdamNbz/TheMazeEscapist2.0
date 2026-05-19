using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseGameController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button closeButton;

    [SerializeField] private Button homeButton;
    [SerializeField] private Button replayButton;

    [Header("UI")]
    [SerializeField] private Transform pausePanel;
    [SerializeField] private CanvasGroup canvasGroup;

    private bool isPaused;

    private void Start()
    {
        pausePanel.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;
        pausePanel.gameObject.SetActive(false);

        pauseButton.onClick.AddListener(PauseGame);
        closeButton.onClick.AddListener(Continue);

        homeButton.onClick.AddListener(BackToSelectLevel);
        replayButton.onClick.AddListener(RestartLevel);
    }

    public void PauseGame()
    {
        if (isPaused) return;

        isPaused = true;
        Time.timeScale = 0f;

        pausePanel.gameObject.SetActive(true);

        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true);

        seq.Append(
            pausePanel.DOScale(Vector3.one, 0.4f)
                      .From(Vector3.zero)
                      .SetEase(Ease.OutBack)
        );

        seq.Join(
            canvasGroup.DOFade(1f, 0.25f)
        );
    }

    public void Continue()
    {
        if (!isPaused) return;

        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true);

        seq.Append(
            pausePanel.DOScale(Vector3.zero, 0.25f)
                      .SetEase(Ease.InBack)
        );

        seq.Join(
            canvasGroup.DOFade(0f, 0.2f)
        );

        seq.OnComplete(() =>
        {
            pausePanel.gameObject.SetActive(false);

            Time.timeScale = 1f;
            isPaused = false;
        });
    }

    public void BackToSelectLevel()
    {
        Time.timeScale = 1f;
        SceneController.Instance.TransitionToScene("LevelSelection");
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;

        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneController.Instance.TransitionToScene(currentSceneName);
    }
}