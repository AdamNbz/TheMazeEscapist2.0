using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    [SerializeField] bool lockMoving = false;
    private Vector2 touchPosition = Vector2.zero;
    private Vector2 releasePosition = Vector2.zero;
    private Vector2 inputPosition = Vector2.zero;

    private Sequence moveSequence;
    private Animator animator;

    public static UnityAction OnLoseGame;
    public static UnityAction OnTurnMove;
    public static UnityAction OnStartMoving;

    public static UnityAction OnPlayerHurt;
    [SerializeField] private int maxHealth = 5;
    private int currentHealth;

    void OnEnable()
    {
        WinPoint.OnLevelComplete += TouchGoal;
        Portal.OnPlayerTeleport += HandleTeleport;
        OnLoseGame += HandleLoseGame;
        HealPotion.OnHealEffectTriggered += Heal;
    }

    void OnDisable()
    {
        WinPoint.OnLevelComplete -= TouchGoal;
        Portal.OnPlayerTeleport -= HandleTeleport;
        OnLoseGame -= HandleLoseGame;
        HealPotion.OnHealEffectTriggered -= Heal;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
    }
    private void TouchGoal()
    {
        lockMoving = true;
        moveSequence?.Kill();
    }

    private void HandleLoseGame()
    {
        Debug.Log("Player Lost! Restarting level...");
        SceneController.Instance.TransitionToScene(SceneManager.GetActiveScene().name);
    }

    private void UnlockMoving()
    {
        lockMoving = false;
    }

    public void OnMove(InputValue value)
    {
        if (lockMoving) return;

        Vector2 input = value.Get<Vector2>();
        Debug.Log($"Move: {input}");

        var direction = Vector2Int.RoundToInt(input);
        if (direction == Vector2Int.zero)
            return;

        var path = GridManager.Instance.FindPathFromWorld(transform.position, direction);
        MoveWithPath(path);
    }

    public void OnPrimaryContact(InputValue value)
    {
        if (lockMoving) return;
        //if (EventSystem.current.IsPointerOverGameObject())
        if (true)
        {
            if (EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.gameObject.tag != "EffectUI")
            {
                Debug.Log("Pointer is over UI, ignoring input.");
                return;
            }
        }

        if (value.Get<float>() > 0.5f)
        {
            //Debug.Log("Primary Contact Started");
            touchPosition = inputPosition;
            //Debug.Log($"Touch Position: {touchPosition}");
        }
        else
        {
            //Debug.Log("Primary Contact Canceled");
            releasePosition = inputPosition;
            //Debug.Log($"Release Position: {releasePosition}");

            var direction = Vector2Int.zero;
            var swipeVector = releasePosition - touchPosition;

            if (Mathf.Abs(swipeVector.x) > Mathf.Abs(swipeVector.y))
            {
                direction.x = swipeVector.x > 0 ? 1 : -1;
            }
            else
            {
                direction.y = swipeVector.y > 0 ? 1 : -1;
            }
            var path = GridManager.Instance.FindPathFromWorld(transform.position, direction);
            MoveWithPath(path);
        }
    }

    public void OnPrimaryPosition(InputValue value)
    {
        inputPosition = value.Get<Vector2>();
    }

    public void MoveWithPath(Path path)
    {
        lockMoving = true;
        moveSequence = DOTween.Sequence();
        var currentPos = transform.position;

        if (animator != null)
        {
            animator.Play("Walk");
        }

        OnStartMoving?.Invoke();
        foreach (var nodeData in path.directions)
        {
            var localScale = transform.localScale;
            var dir = nodeData.direction;
            if (dir.x != 0)
            {
                localScale.x = dir.x > 0 ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
                transform.localScale = localScale;
            }

            // Add stop time if required
            if(nodeData.stopTime > 0)
            {
                moveSequence.AppendInterval(nodeData.stopTime);
            }

            moveSequence.Append(transform.DOMove(currentPos + new Vector3(dir.x, dir.y, 0) * path.stepLength, 0.1f)
                .SetEase(Ease.Linear).OnComplete(() =>
                {
                    AudioManager.Instance.PlaySfx("player_move", transform.position);
                }));

            currentPos += new Vector3(dir.x, dir.y, 0) * path.stepLength;
        }
        moveSequence.OnComplete(() =>
        {
            lockMoving = false;
            if (animator != null)
            {
                animator.Play("Idle");
            }
            OnTurnMove?.Invoke();
        });
    }

    public async void HandleTeleport(TeleportData data)
    {
        moveSequence?.Kill();
        lockMoving = true;

        Vector3 currentScale = transform.localScale;
        AudioManager.Instance.PlaySfx("teleport", transform.position);
        await transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack).ToUniTask();
        transform.position = data.TargetPosition;
        await transform.DOScale(currentScale, 0.25f).SetEase(Ease.OutBack).ToUniTask();

        lockMoving = false;
        data.LinkedPortal.UnlockPortal();
        OnTurnMove?.Invoke();
    }

    public void TakeDamage()
    {
        currentHealth -= 1;
        Debug.Log($"Player took 1 damage. Current health: {currentHealth}");

        if (currentHealth <= 0)
        {
            OnLoseGame?.Invoke();
        }
    }

    public void Heal()
    {
        currentHealth = Mathf.Min(currentHealth + 1, maxHealth);
        Debug.Log($"Player healed 1. Current health: {currentHealth}");
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            //var enemy = collision.GetComponent<EnemyController>();

            TakeDamage();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Enemy"))
        {
            //var enemy = collision.collider.GetComponent<EnemyController>();

            TakeDamage();
        }
    }
}