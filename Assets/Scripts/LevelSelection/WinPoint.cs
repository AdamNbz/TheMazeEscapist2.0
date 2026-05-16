using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class WinPoint : MonoBehaviour
{
    public static UnityAction OnLevelComplete;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CheckLevel();
            OnLevelComplete?.Invoke();
            AudioManager.Instance.PlaySfx("victory", Vector2.zero);
            Debug.Log("Level Complete!");
        }
    }
    private void CheckLevel()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        int currentLevel = int.Parse(currentScene.Replace("Level ", ""));

        if (currentLevel == PlayerProgress.CurrentLevel && currentLevel != 10)
        {
            PlayerProgress.UnlockNextLevel();
        }

    }

}