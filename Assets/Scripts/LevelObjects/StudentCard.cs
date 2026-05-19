using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class StudentCard : SpecialTile
{
    [SerializeField] private WinpointUnlockCondition winpointUnlockCondition;

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
            gameObject.SetActive(false);
            OnSpecialTileInteracted?.Invoke(this);
        }
    }
}