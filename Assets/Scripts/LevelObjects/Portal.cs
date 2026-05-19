using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class Portal : SpecialTile
{
    public static UnityAction<TeleportData> OnPlayerTeleport;
    [SerializeField] Portal linkedPortal;
    private bool isEnabled = true;

    public override TileType Type => TileType.Portal;

    void Start()
    {
        OnInstantiated();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;
        if (!isEnabled) return;
        if (linkedPortal != null)
        {
            linkedPortal.isEnabled = false;
            OnPlayerTeleport?.Invoke(new TeleportData
            {
                StartPosition = transform.position,
                TargetPosition = linkedPortal.transform.position,
                LinkedPortal = linkedPortal
            });
        }
    }
    public void UnlockPortal()
    {
        isEnabled = true;
    }
}

public struct TeleportData
{
    public Vector3 StartPosition;
    public Vector3 TargetPosition;
    public Portal LinkedPortal;
}
