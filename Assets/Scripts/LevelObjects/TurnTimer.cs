using TMPro;
using UnityEngine;

public class TurnTimer : MonoBehaviour
{
    [SerializeField] private int turnLimit = 5;
    [SerializeField] private string turnDisplayFormat = "Số lượt còn lại: {0}";
    [SerializeField] private TextMeshProUGUI turnText;

    private int currentTurn = 0;

    void Start()
    {
        turnText.text = string.Format(turnDisplayFormat, turnLimit - currentTurn);
    }

    void OnEnable()
    {
        PlayerController.OnTurnMove += HandleTurnMove;
    }
    void OnDisable()
    {
        PlayerController.OnTurnMove -= HandleTurnMove;
    }

    private void HandleTurnMove()
    {
        currentTurn++;
        turnText.text = string.Format(turnDisplayFormat, turnLimit - currentTurn);
        if (currentTurn >= turnLimit)
            PlayerController.OnLoseGame?.Invoke();
    }
}