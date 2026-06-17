using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class Ink : SpecialTile
{
    [SerializeField] private string soundEffectName = "trash_can_collected";
    public static UnityAction OnInkEffectTriggered;
    public override TileType Type => TileType.Ink;

    void Start()
    {
        OnInstantiated();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Ink tile triggered!");
            OnInkEffectTriggered?.Invoke();
            OnSpecialTileInteracted?.Invoke(this);
            AudioManager.Instance.PlaySfx(soundEffectName, transform.position);
            gameObject.SetActive(false);
        }
    }
}
