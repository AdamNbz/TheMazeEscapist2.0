using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class BossController : MonoBehaviour
{
    public float fadeDuration = 1f;
    [SerializeField] private Image InkEffectImage;
    [SerializeField] private GameObject PencilAttackPrefab;
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

        // Test pencil attack on cell 0 0
        TriggerPencilAttack(false, 2f, 1f, 5f, new Vector3Int(1, 0, 0), new Vector3Int(0, 0, 0));
        TriggerPencilAttack(false, 2f, 1f, 5f, new Vector3Int(0, 1, 0), new Vector3Int(0, 0, 0));
        TriggerPencilAttack(false, 2f, 1f, 5f, new Vector3Int(-1, 0, 0), new Vector3Int(0, 0, 0));
        TriggerPencilAttack(false, 2f, 1f, 5f, new Vector3Int(0, -1, 0), new Vector3Int(0, 0, 0));
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

    public void TriggerPencilAttack(bool canFollow, float aimingDuration, float lockDuration, float speed, Vector3Int direction, Vector3Int initialPosition)
    {
        Debug.Log("Boss triggered pencil attack!");
        GameObject pencil = Instantiate(PencilAttackPrefab);
        PencilAttack pencilAttack = pencil.GetComponent<PencilAttack>();
        pencilAttack.Initialise(canFollow, aimingDuration, lockDuration, speed, direction, initialPosition);
    }
}
