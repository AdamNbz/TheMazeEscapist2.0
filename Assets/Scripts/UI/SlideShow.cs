using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SlideShow : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button openButton;
    //[SerializeField] private Button closeButton;

    [SerializeField] private Button moveLeft;
    [SerializeField] private Button moveRight;

    [Header("UI")]
    [SerializeField] private CanvasGroup container;
    [SerializeField] private CanvasGroup canvas;

    [SerializeField] private RectTransform[] slides;

    [Header("Animation")]
    [SerializeField] private float slideDuration = 0.35f;

    private int currentIndex;

    private void Awake()
    {
        openButton.onClick.AddListener(Open);
        //closeButton.onClick.AddListener(Close);

        moveLeft.onClick.AddListener(MoveLeft);
        moveRight.onClick.AddListener(MoveRight);
    }

    private void Start()
    {
        container.alpha = 0;
        container.blocksRaycasts = false;
        container.interactable = false;

        canvas.alpha = 0;
        canvas.blocksRaycasts = false;
        canvas.interactable = false;

        ShowSlideInstant(currentIndex);
        UpdateButtons();
    }

    private void Open()
    {
        container.DOFade(1, 0.25f);

        container.blocksRaycasts = true;
        container.interactable = true;

        canvas.DOFade(1, 0.25f);

        canvas.blocksRaycasts = true;
        canvas.interactable = true;
    }

    private void Close()
    {
        container.DOFade(0, 0.25f)
                 .OnComplete(() =>
                 {
                     container.blocksRaycasts = false;
                     container.interactable = false;
                 });
    }

    private void MoveLeft()
    {
        if (currentIndex <= 0)
            return;

        ChangeSlide(currentIndex - 1);
    }

    private void MoveRight()
    {
        if (currentIndex >= slides.Length - 1)
            return;

        ChangeSlide(currentIndex + 1);
    }

    private void ChangeSlide(int newIndex)
    {
        RectTransform current = slides[currentIndex];
        RectTransform next = slides[newIndex];

        float direction = newIndex > currentIndex ? 1 : -1;

        next.gameObject.SetActive(true);

        next.anchoredPosition = new Vector2(direction * 1200f, 0);

        current.DOAnchorPosX(-direction * 1200f, slideDuration)
               .SetEase(Ease.OutCubic);

        next.DOAnchorPosX(0, slideDuration)
            .SetEase(Ease.OutCubic);

        currentIndex = newIndex;

        UpdateButtons();

        DOVirtual.DelayedCall(slideDuration, () =>
        {
            for (int i = 0; i < slides.Length; i++)
            {
                if (i != currentIndex)
                    slides[i].gameObject.SetActive(false);
            }
        });
    }

    private void ShowSlideInstant(int index)
    {
        for (int i = 0; i < slides.Length; i++)
        {
            bool isCurrent = i == index;

            slides[i].gameObject.SetActive(isCurrent);

            slides[i].anchoredPosition = Vector2.zero;
        }
    }

    private void UpdateButtons()
    {
        moveLeft.interactable = currentIndex > 0;
        moveRight.interactable = currentIndex < slides.Length - 1;
    }
}