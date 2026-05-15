using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class LevelNodeView : MonoBehaviour
{
    [SerializeField]
    private TMP_Text levelText;

    [SerializeField]
    private Button button;

    [SerializeField]
    private Image lockImage;

    private int levelId;

    private Tween bounceTween;
    public void Setup(
        int id,
        bool unlocked)
    {
        levelId = id;

        levelText.text = id.ToString();

        button.interactable = unlocked;

        if (lockImage != null)
        {
            lockImage.gameObject.SetActive(!unlocked);
        }

        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(OnClick);

        SetupCurrentLevelAnimation();
    }

    void SetupCurrentLevelAnimation()
    {
        transform.localScale = Vector3.one;

        bounceTween?.Kill();

        if (levelId != PlayerProgress.CurrentLevel)
            return;

        bounceTween =
            transform
                .DOScale(1.15f, 0.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
    }
    void OnDestroy()
    {
        bounceTween?.Kill();
    }


    void OnClick()
    {
        SceneController.Instance.TransitionToScene($"Level {levelId}");
    }
}