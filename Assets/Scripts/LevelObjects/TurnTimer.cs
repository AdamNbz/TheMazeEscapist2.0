using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TurnTimer : MonoBehaviour
{
    [SerializeField] private int turnLimit = 5;
    [SerializeField] private string turnDisplayFormat = "Số lượt còn lại: {0}";
    [SerializeField] private TextMeshProUGUI turnText;
    public static UnityAction OnTimeOut;

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
        {
            turnText.text = "NO MORE TURNS !!!";
            OnTimeOut?.Invoke();
            AudioManager.Instance.PlaySfx("lose", Vector3.zero);
            DoAnimation();
        }
    }

    public void DoAnimation()
    {
        turnText.color = Color.red;

        RectTransform rect = turnText.rectTransform;

        // Lưu world position hiện tại
        Vector3 worldPos = rect.position + new Vector3(0, -100, 0);

        // Đổi anchor/pivot
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        // Gán lại vị trí world để tránh bị jump
        rect.position = worldPos;

        Sequence seq = DOTween.Sequence();

        seq.Append(
            rect.DOAnchorPos(Vector2.zero, 1.5f)
                .SetEase(Ease.InOutQuart)
        );

        seq.Append(
            turnText.DOFade(0f, 0.25f)
                     .SetLoops(6, LoopType.Yoyo)
        );

        seq.OnComplete(() =>
        {
            PlayerController.OnLoseGame?.Invoke();
        });
    }
}