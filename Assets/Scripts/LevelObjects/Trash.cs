using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class Trash : MonoBehaviour
{
    public static UnityAction<Trash> OnTrashCollected;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            OnTrashCollected?.Invoke(this);
            gameObject.SetActive(false);
        }
    }
}
