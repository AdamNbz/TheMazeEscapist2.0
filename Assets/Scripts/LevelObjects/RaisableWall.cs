using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class RaisableWall : SpecialTile
{
    [SerializeField] private string soundEffectName = "trash_can_collected";
    public static UnityAction OnInkEffectTriggered;
    public override TileType Type => TileType.Wall;

    void Start()
    {
        //OnInstantiated();
    }

    public void Raise()
    {
        this.gameObject.SetActive(true);
        // Tween y scale from 0 to 1
        transform.localScale = new Vector3(transform.localScale.x, 0, transform.localScale.z);
        transform.DOScaleY(1, 1f);
    }

    public void Lower()
    {
        this.gameObject.SetActive(true);
        // Tween y scale from 1 to 0
        transform.localScale = new Vector3(transform.localScale.x, 1, transform.localScale.z);
        var tween = transform.DOScaleY(0, 1f).SetEase(Ease.InElastic);
        tween.OnComplete(() =>
        {
            Destroy(this.gameObject);
        });

    }
}
