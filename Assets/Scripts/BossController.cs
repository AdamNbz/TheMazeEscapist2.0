using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class BossController : MonoBehaviour
{
    public float fadeDuration = 1f;
    [SerializeField] private Image InkEffectImage;
    private Tween inkEffectTween;

    void OnEnable()
    {
        Ink.OnInkEffectTriggered += TriggerInkEffect;
    }

    void OnDisable()
    {
        Ink.OnInkEffectTriggered -= TriggerInkEffect;
    }

    void Start()
    {
        InkEffectImage.gameObject.SetActive(false);
    }

    public void TriggerInkEffect()
    {
        Debug.Log("Boss triggered ink effect!");
        inkEffectTween?.Kill();
        InkEffectImage.gameObject.SetActive(true);
        InkEffectImage.color = new Color(InkEffectImage.color.r, InkEffectImage.color.g, InkEffectImage.color.b, 1f);
        inkEffectTween = InkEffectImage.DOFade(0f, fadeDuration);
        inkEffectTween.OnComplete(() => InkEffectImage.gameObject.SetActive(false));
    }
}
