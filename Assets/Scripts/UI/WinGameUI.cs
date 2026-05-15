using DG.Tweening;
using DG.Tweening.Core.Easing;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinGameUI : MonoBehaviour
{
    [SerializeField] private Transform popup;

    private void OnEnable()
    {
        WinPoint.OnLevelComplete += DoPop;
    }

    private void OnDisable()
    {
        WinPoint.OnLevelComplete -= DoPop;
    }
    private void Start()
    {
        popup.localScale = Vector3.zero;
    }

    private void DoPop()
    {

        popup.DOScale(Vector3.one, 1.5f)
             .SetEase(Ease.OutBack);
    }

    public void BackToSelectLevel()
    {
        SceneController.Instance.TransitionToScene("LevelSelection");
    }

    public void NextLevel()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        int currentLevel = int.Parse(currentScene.Replace("Level ", ""));

        if(currentLevel >= PlayerProgress.CurrentLevel)
        {
            PlayerProgress.SetCurrentLevel(currentLevel);
        }    

        SceneController.Instance .TransitionToScene($"Level {currentLevel + 1}");
    }
    public void RestartLevel()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneController.Instance.TransitionToScene(currentSceneName);
    }

}
