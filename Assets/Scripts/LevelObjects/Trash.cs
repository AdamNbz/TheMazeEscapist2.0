using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class Trash : SpecialTile
{
    [SerializeField] private string soundEffectName = "trash_can_collected";
    private bool isCollected = false;

    public override TileType Type => TileType.Trash;

    void Start()
    {
        OnInstantiated();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCollected) return;
        if (collision.CompareTag("Player"))
        {
            isCollected = true;
            OnSpecialTileInteracted?.Invoke(this);
            gameObject.SetActive(false);
            AudioManager.Instance.PlaySfx(soundEffectName, transform.position);
        }
    }

    public async UniTask DiscardTrash(Vector3 recycleBinPosition)
    {
        transform.position = recycleBinPosition;
        gameObject.SetActive(true);

        await transform.DOScale(Vector3.one * 1.5f, 0.3f).SetEase(Ease.OutBack).ToUniTask();

        await transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
        gameObject.SetActive(false);
    }
}
