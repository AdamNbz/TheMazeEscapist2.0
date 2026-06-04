using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Threading.Tasks;


public class BossController : MonoBehaviour
{
    public float fadeDuration = 1f;
    [SerializeField] private Image InkEffectImage;
    [SerializeField] private GameObject PencilAttackPrefab;
    [SerializeField] private GameObject WarningTilePrefab;
    [SerializeField] private Vector3Int originCell = new Vector3Int(-6, -1, 0);
    [SerializeField] private int size = 12;
    private Tween inkEffectTween;

    void OnEnable()
    {
        Ink.OnInkEffectTriggered += TriggerInkEffect;
    }

    void OnDisable()
    {
        Ink.OnInkEffectTriggered -= TriggerInkEffect;
    }

    void Start()
    {
        InkEffectImage.gameObject.SetActive(false);
        DemoSequence();
    }

    async void DemoSequence()
    {
        for (int i = 1; i < size - 1; i++)
        {
            for (int j = 1; j < size - 1; j++)
            {
                TriggerRaisingWall(new Vector3Int((int)originCell.x + i, (int)originCell.y - j, 0));
            }
        }
        await Task.Delay(8000);

        // Test pencil attack on cell 0 0
        TriggerPencilAttack(2f, 1f, 5f, new Vector3Int(1, 0, 0), originCell);
        TriggerPencilAttack(2f, 1f, 5f, new Vector3Int(0, 1, 0), originCell);

        //wait 8 seconds then lower wall
        await Task.Delay(3000);

        for (int i = 1; i < size - 1; i++)
        {
            for (int j = 1; j < size - 1; j++)
            {
                TriggerLoweringWall(new Vector3Int((int)originCell.x + i, (int)originCell.y - j, 0));
            }
        }
    }

    public void TriggerInkEffect()
    {
        Debug.Log("Boss triggered ink effect!");
        inkEffectTween?.Kill();
        InkEffectImage.gameObject.SetActive(true);
        InkEffectImage.color = new Color(InkEffectImage.color.r, InkEffectImage.color.g, InkEffectImage.color.b, 1f);
        inkEffectTween = InkEffectImage.DOFade(0f, fadeDuration);
        inkEffectTween.OnComplete(() => InkEffectImage.gameObject.SetActive(false));
    }

    public void TriggerPencilAttack(float aimingDuration, float lockDuration, float speed, Vector3Int direction, Vector3Int initialPosition)
    {
        Debug.Log("Boss triggered pencil attack!");
        GameObject pencil = Instantiate(PencilAttackPrefab);
        PencilAttack pencilAttack = pencil.GetComponent<PencilAttack>();
        pencilAttack.Initialise(aimingDuration, lockDuration, speed, direction, initialPosition);
    }

    public void TriggerRaisingWall(Vector3Int cellPosition)
    {
        Debug.Log("Boss triggered raising wall!");
        var warningTile = Instantiate(WarningTilePrefab, GridManager.Instance.CellToWorld(cellPosition) + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
        warningTile.GetComponent<WarningTile>().Init(3f);
    }

    public void TriggerLoweringWall(Vector3Int cellPosition)
    {
        Debug.Log("Boss triggered lowering wall!");
        GridManager.Instance.LowerWall(cellPosition);
    }
}
