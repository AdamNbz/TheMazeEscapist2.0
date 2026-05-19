using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;


[RequireComponent(typeof(BoxCollider2D))]
public class RecycleBin : SpecialTile
{
    // private int trashCollected = 0;
    private List<Trash> collectedTrash = new();
    private Queue<Trash> handlingTrashQueue = new();
    private int trashHandled = 0;
    [SerializeField] private int trashToHandle = 3;

    [SerializeField] private CanvasGroup textLineCanvasGroup;
    [SerializeField] private TextMeshProUGUI textLine;

    [SerializeField] private string idleLine = "Hãy cho toi rác!!";
    [SerializeField] private string interactedLine = "Cảm ơn bạn!";

    [SerializeField] private float textDisplayDuration = 2f;
    [SerializeField] private WinpointUnlockCondition winpointUnlockCondition;

    public override TileType Type => TileType.RecycleBin;

    void Start()
    {
        textLine.text = idleLine;
        OnInstantiated();
    }

    void OnEnable()
    {
        Trash.OnTrashCollected += HandlePlayerCollectTrash;
    }

    void OnDisable()
    {
        Trash.OnTrashCollected -= HandlePlayerCollectTrash;
    }

    private void HandlePlayerCollectTrash(Trash trash)
    {
        collectedTrash.Add(trash);
        Debug.Log($"Trash collected: {collectedTrash.Count}");
    }

    async UniTaskVoid OnTriggerEnter2D(Collider2D collision)
    {
        if (collectedTrash.Count <= 0)
            return;
        if (collision.CompareTag("Player"))
        {
            trashHandled += collectedTrash.Count;
            foreach (var trash in collectedTrash)
                handlingTrashQueue.Enqueue(trash);
            collectedTrash.Clear();
            while (handlingTrashQueue.Count > 0)
            {
                var trash = handlingTrashQueue.Dequeue();
                AudioManager.Instance.PlaySfx("recycle_trash", transform.position);
                Debug.Log($"Handling trash: {trash.gameObject.name}");
                await trash.DiscardTrash(transform.position);
            }
            await PlayInteractedLine();

            if (trashHandled >= trashToHandle)
                WinPoint.OnUnlockedConditionMet?.Invoke(winpointUnlockCondition.conditionName);
        }
    }

    async UniTask PlayInteractedLine()
    {
        await textLineCanvasGroup.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).ToUniTask();
        textLine.text = interactedLine;
        await textLineCanvasGroup.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack).ToUniTask();

        await UniTask.Delay((int)(textDisplayDuration * 1000));

        await textLineCanvasGroup.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).ToUniTask();
        textLine.text = idleLine;

        if (trashHandled < trashToHandle)
            await textLineCanvasGroup.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack).ToUniTask();
    }
}
