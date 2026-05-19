using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class Rock : MonoBehaviour
{
    [SerializeField] private GameObject rock;

    public static UnityAction<Vector3> OnRockEnabled;

    void Start()
    {
        rock.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            EnableRock();
        }
    }

    public void EnableRock()
    {
        rock.SetActive(true);
        rock.transform.localScale = Vector3.zero;
        rock.transform.DOScale(1, 0.3f).SetEase(Ease.OutBack);
        OnRockEnabled?.Invoke(rock.transform.position);
    }
}