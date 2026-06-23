using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SpriteRenderer))]
public class WarningTile : SpecialTile
{
    [SerializeField] private string soundEffectName = "trash_can_collected";
    [SerializeField] private Sprite flashSprite;
    public override TileType Type => TileType.Walkable;
    [SerializeField] private Collider2D damageCollider;
    private SpriteRenderer spriteRenderer;
    private float warningDuration = 1f;
    const float flasingDuration = 1f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        damageCollider = GetComponent<Collider2D>();
        damageCollider.enabled = false;
    }

    public async void Init(float warningDuration)
    {
        Debug.Log("Warning tile initialized at " + transform.position);
        this.warningDuration = warningDuration;
        await UniTask.Delay((int)(warningDuration * 1000));
        spriteRenderer.sprite = flashSprite;
        await UniTask.Delay((int)(flasingDuration * 1000));
        //GridManager.Instance.RaiseWall(GridManager.Instance.WorldToCell(transform.position));
        //Check collsion with the player in short amount of time (200ms), if player inside then take damage
        damageCollider.enabled = true;
        spriteRenderer.color = Color.black;
        await UniTask.Delay(500);
        damageCollider.enabled = false;
        Destroy(this.gameObject);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Player takes damage only once
            damageCollider.enabled = false;
        }
    }

}
