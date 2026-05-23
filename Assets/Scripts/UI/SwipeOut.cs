using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SwipeOut : MonoBehaviour 
{ 
    [Header("Hint Text")]
    [SerializeField] private RectTransform hintText;

    [Header("Animation")]
    [SerializeField] private float delayBeforeHide = 5f;
    [SerializeField] private float moveDuration = 0.6f;
    [SerializeField] private float scaleDuration = 0.6f;

    [SerializeField] private Ease moveEase = Ease.InBack;
    [SerializeField] private Ease scaleEase = Ease.InBack;

    private Vector3 originalScale;

    private void Start()
    {
        originalScale = hintText.localScale;

        // Hiện text lúc start
        hintText.gameObject.SetActive(true);
        hintText.localScale = originalScale;

        PlayGuideAnimation();
    }

    private void PlayGuideAnimation()
    {
        Sequence seq = DOTween.Sequence();

        // Delay 3 giây
        seq.AppendInterval(delayBeforeHide);

        // Bay về phía button
        seq.Append(
            hintText.DOMove(
                this.transform.position,
                moveDuration
            ).SetEase(moveEase)
        );

        // Scale nhỏ dần kiểu bị hút vào
        seq.Join(
            hintText.DOScale(Vector3.zero, scaleDuration)
                    .SetEase(scaleEase)
        );

        //// Xoay nhẹ cho có cảm giác hút
        //seq.Join(
        //    hintText.DORotate(
        //        new Vector3(0, 0, 180f),
        //        scaleDuration,
        //        RotateMode.FastBeyond360
        //    )
        //);

        // Ẩn sau animation
        seq.OnComplete(() =>
        {
            hintText.gameObject.SetActive(false);
        });
    }
}