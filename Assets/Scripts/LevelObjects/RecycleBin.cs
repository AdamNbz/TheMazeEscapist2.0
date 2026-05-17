using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;


[RequireComponent(typeof(BoxCollider2D))]
public class RecycleBin : MonoBehaviour
{
    private int trashCollected = 0;
    private int trashHandled = 0;
    [SerializeField] private int trashToHandle = 3;

    [SerializeField] private CanvasGroup textLineCanvasGroup;
    [SerializeField] private TextMeshProUGUI textLine;

    [SerializeField] private string idleLine = "Hãy cho toi rác!!";
    [SerializeField] private string interactedLine = "Cảm ơn bạn!";

    [SerializeField] private float textDisplayDuration = 2f;

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
        trashCollected++;
        Debug.Log($"Trash collected: {trashCollected}");
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (trashCollected <= 0)
            return;
        if (collision.CompareTag("Player"))
        {
            PlayInteractedLine().Forget();
            trashHandled += trashCollected;
            trashCollected = 0;
            AudioManager.Instance.PlaySfx("recycle_trash", transform.position);

            if (trashHandled >= trashToHandle)
                WinPoint.OnUnlockedConditionMet?.Invoke();
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
