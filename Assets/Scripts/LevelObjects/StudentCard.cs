using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class StudentCard : SpecialTile
{
    [SerializeField] private WinpointUnlockCondition winpointUnlockCondition;

    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        EyeofTheStorm.OnTouchPlayer += ReleaseCard;
    }

    void OnDisable()
    {
        EyeofTheStorm.OnTouchPlayer -= ReleaseCard;
    }

    private void ReleaseCard()
    {
        ShowCard();
        WinPoint.OnLockedConditionMet?.Invoke(winpointUnlockCondition.conditionName);
    }

    private void HideCard()
    {
        boxCollider.enabled = false;
        spriteRenderer.enabled = false;
    }

    private void ShowCard()
    {
        boxCollider.enabled = true;
        spriteRenderer.enabled = true;
    }

    public override TileType Type => TileType.StudentCard;

    void Start()
    {
        OnInstantiated();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            WinPoint.OnUnlockedConditionMet?.Invoke(winpointUnlockCondition.conditionName);
            AudioManager.Instance.PlaySfx("student_card_collected", transform.position);
            // gameObject.SetActive(false);
            HideCard();
            OnSpecialTileInteracted?.Invoke(this);
        }
    }
}