using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.Events;


public class BossController : MonoBehaviour
{
    StateMachine stateMachine;
    private Animator animator;

    public float fadeDuration = 1f;
    [SerializeField] private Image InkEffectImage;
    [SerializeField] public GameObject HealPotionPrefab;
    [SerializeField] public GameObject InkPrefab;
    [SerializeField] public GameObject SwordPrefab;
    [SerializeField] public GameObject PencilAttackPrefab;
    [SerializeField] public GameObject WarningTilePrefab;
    [SerializeField] public GameObject SlimePrefab;
    private GameObject grid;

    public GameObject playerObject;

    public static readonly Vector3Int originCell = new Vector3Int(-4, -4, 0);
    public static readonly int size = 7;
    private Tween inkEffectTween;

    void OnEnable()
    {
        Ink.OnInkEffectTriggered += TriggerInkEffect;
    }

    void OnDisable()
    {
        Ink.OnInkEffectTriggered -= TriggerInkEffect;
    }

    void Awake()
    {
        stateMachine = new StateMachine();
        // Initialize states and transitions here if needed

        animator = GetComponent<Animator>();

        // Modify attack commands here
        List<BossCommand> phase1Commands = new List<BossCommand>
        {
            new RandomFourDirectionAttack(this),
            new ThreeByThreeAttack(this),
        };
        List<BossCommand> phase2Commands = new List<BossCommand>
        {
            new SnakeChaseAttack(this),
            new ShortCheckerAttack(this),
            new Phase2PencilAttack(this),
        };
        List<BossCommand> phase3Commands = new List<BossCommand>
        {
            new ChasingAttack(this),
        };
        var phase1 = new BossPhase(this, animator, phase1Commands);
        var phase2 = new BossPhase(this, animator, phase2Commands, 100, new RaisePhase2Walls(this));
        var phase3 = new BossPhase(this, animator, phase3Commands);
        var hurtState = new BossHurtState(this, animator);
        var winState = new BossWinState(this, animator);
        var loseState = new BossLoseState(this, animator);

        At(phase1, phase2, new FuncPredicate(() => phase1.IsPhaseEnded()));
        At(phase2, phase3, new FuncPredicate(() => phase2.IsPhaseEnded()));
        playerObject = GameObject.Find("Player");
        grid = GameObject.Find("Grid");

        stateMachine.SetState(phase1);
    }

    void Start()
    {
        InkEffectImage.gameObject.SetActive(false);
    }

    void Update()
    {
        stateMachine.Update();
    }

    void FixedUpdate()
    {
        stateMachine.FixedUpdate();
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

    public void TriggerPencilAttack(float aimingDuration, float lockDuration, float speed, Vector3Int direction, Vector3Int initialCellPosition)
    {
        Debug.Log("Boss triggered pencil attack!");
        GameObject pencil = Instantiate(PencilAttackPrefab);
        pencil.transform.SetParent(grid.transform, false);
        PencilAttack pencilAttack = pencil.GetComponent<PencilAttack>();
        pencilAttack.Initialise(aimingDuration, lockDuration, speed, direction, initialCellPosition);
    }

    public void TriggerRaisingWall(Vector3Int cellPosition)
    {
        //Debug.Log("Boss triggered raising wall!");
        GridManager.Instance.RaiseWall(cellPosition);
    }

    public void TriggerLoweringWall(Vector3Int cellPosition)
    {
        //Debug.Log("Boss triggered lowering wall!");
        GridManager.Instance.LowerWall(cellPosition);
    }

    public void TriggerLowerAllWalls()
    {
        for (int i = originCell.x; i < originCell.x + size; i++)
        {
            for (int j = originCell.y; j > originCell.y - size; j--)
            {
                GridManager.Instance.LowerWall(new Vector3Int(i, j, 0));
            }
        }
    }

    public GameObject TriggerCreateTile(Vector3Int cellPosition, GameObject tilePrefab)
    {
        return GridManager.Instance.CreateSpecialTile(cellPosition, tilePrefab);
    }

    public void TriggerRemoveTile(Vector3Int cellPosition)
    {
        GridManager.Instance.RemoveSpecialTile(cellPosition);
    }

    public void TriggerInvoke(string methodName, float delay)
    {
        Invoke(methodName, delay);
    }

    public void TriggerCreateRandomItem(GameObject itemPrefab)
    {
        // Get random walkable cell position within the grid bounds
        Vector3Int cellPosition = RandomWalkableCell();
        TriggerCreateTile(cellPosition, itemPrefab);
    }

    public Vector3Int RandomWalkableCell()
    {
        Vector3Int cellPosition;
        do
        {
            int randomX = Random.Range(originCell.x, originCell.x + size);
            int randomY = Random.Range(originCell.y - size + 1, originCell.y + 1);
            cellPosition = new Vector3Int(randomX, randomY, 0);
        } while (!GridManager.Instance.IsWalkable(cellPosition) || GridManager.Instance.IsItem(cellPosition));
        return cellPosition;
    }

    void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
    void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);
}
