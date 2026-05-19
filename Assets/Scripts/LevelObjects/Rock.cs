using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class Rock : MonoBehaviour
{
    [SerializeField] private GameObject rock;
    private SpriteRenderer hiddenRock;

    public static UnityAction<Vector3> OnRockEnabled;

    void Start()
    {
        hiddenRock = GetComponent<SpriteRenderer>();
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
        hiddenRock.enabled = false;
        rock.transform.localScale = Vector3.zero;
        rock.transform.DOScale(1, 0.3f).SetEase(Ease.OutBack);
        OnRockEnabled?.Invoke(rock.transform.position);
    }
}