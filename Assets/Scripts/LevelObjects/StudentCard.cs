using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class StudentCard : MonoBehaviour
{
    [SerializeField] private WinpointUnlockCondition winpointUnlockCondition;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            WinPoint.OnUnlockedConditionMet?.Invoke(winpointUnlockCondition.conditionName);
            AudioManager.Instance.PlaySfx("student_card_collected", transform.position);
            gameObject.SetActive(false);
        }
    }
}