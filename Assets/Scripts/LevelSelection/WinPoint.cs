using UnityEngine;
using UnityEngine.Events;

public class WinPoint : MonoBehaviour
{
    public static UnityAction OnLevelComplete;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            OnLevelComplete?.Invoke();
            Debug.Log("Level Complete!");
        }
    }
}