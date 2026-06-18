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

    private float warningDuration = 1f;
    const float flasingDuration = 1f;

    void Start()
    {
        //Init(4f);
    }

    public async void Init(float warningDuration)
    {
        this.warningDuration = warningDuration;
        await UniTask.Delay((int)(warningDuration * 1000));
        GetComponent<SpriteRenderer>().sprite = flashSprite;
        await UniTask.Delay((int)(flasingDuration * 1000));
        GridManager.Instance.RaiseWall(GridManager.Instance.WorldToCell(transform.position));
        Destroy(this.gameObject);
    }
}
