using TMPro;
using UnityEngine;

public class LevelTimer : MonoBehaviour
{
    [SerializeField] private float timeLimit = 60f;
    private float timeRemaining;
    private bool timerRunning = false;

    [SerializeField] private string timerDisplayFormat = "Thời gian còn lại: {0:0.00}";
    [SerializeField] private TextMeshProUGUI timerText;

    private void Start()
    {
        timeRemaining = timeLimit;
        StartTimer();
    }

    private void Update()
    {
        if (timerRunning)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0)
            {
                timeRemaining = 0;
                timerRunning = false;
                PlayerController.OnLoseGame?.Invoke();
            }
            UpdateTimerDisplay();
        }
    }

    private void UpdateTimerDisplay()
    {
        timerText.text = string.Format(timerDisplayFormat, timeRemaining);
    }

    public void StartTimer()
    {
        timerRunning = true;
    }

    public void StopTimer()
    {
        timerRunning = false;
    }

    public void ResetTimer()
    {
        timeRemaining = timeLimit;
        UpdateTimerDisplay();
    }
}