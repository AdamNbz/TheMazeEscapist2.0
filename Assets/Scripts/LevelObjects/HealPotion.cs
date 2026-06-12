using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class HealPotion : SpecialTile
{
    [SerializeField] private string soundEffectName = "health_potion_collected";
    public static UnityAction OnHealEffectTriggered;
    public override TileType Type => TileType.Item;

    void Start()
    {
        OnInstantiated();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Heal Potion tile triggered!");
            OnHealEffectTriggered?.Invoke();
            OnSpecialTileInteracted?.Invoke(this);
            AudioManager.Instance.PlaySfx(soundEffectName, transform.position);
            Destroy(gameObject);
        }
    }
}
