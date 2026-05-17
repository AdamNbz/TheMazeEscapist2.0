using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class Trash : MonoBehaviour
{
    public static UnityAction<Trash> OnTrashCollected;
    [SerializeField] private string soundEffectName = "trash_can_collected";
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            OnTrashCollected?.Invoke(this);
            gameObject.SetActive(false);
            AudioManager.Instance.PlaySfx(soundEffectName, transform.position);
        }
    }
}
