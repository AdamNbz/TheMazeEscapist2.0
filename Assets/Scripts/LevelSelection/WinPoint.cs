using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class WinPoint : MonoBehaviour
{
    public static UnityAction OnLevelComplete;
    [SerializeField] private bool isLockedByDefault = false;
    private bool isLocked;

    public static UnityAction OnUnlockedConditionMet;

    void Awake()
    {
        if (isLockedByDefault)
        {
            gameObject.SetActive(false);
            isLocked = true;
        }

        OnUnlockedConditionMet += UnlockWinPoint;
    }

    void OnDestroy()
    {
        OnUnlockedConditionMet -= UnlockWinPoint;
    }

    private void UnlockWinPoint()
    {
        isLocked = false;
        gameObject.SetActive(true);
        AudioManager.Instance.PlaySfx("win_point_unlocked", transform.position);
        transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CheckLevel();
            OnLevelComplete?.Invoke();
            AudioManager.Instance.PlaySfx("victory", Vector2.zero);
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