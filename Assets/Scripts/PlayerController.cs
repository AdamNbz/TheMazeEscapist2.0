using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections.Generic;

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

    private readonly List<ICollectible> collectedItems = new();
    private int currentHealth;

    void OnEnable()
    {
        WinPoint.OnLevelComplete += TouchGoal;
        Portal.OnPlayerTeleport += HandleTeleport;
        OnLoseGame += HandleLoseGame;
        EyeofTheStorm.OnTouchPlayer += HandleTouchStorm;
        SpecialTile.OnSpecialTileInteracted += HandleSpecialTileInteraction;
    }

    void OnDisable()
    {
        WinPoint.OnLevelComplete -= TouchGoal;
        Portal.OnPlayerTeleport -= HandleTeleport;
        OnLoseGame -= HandleLoseGame;
        EyeofTheStorm.OnTouchPlayer -= HandleTouchStorm;
        SpecialTile.OnSpecialTileInteracted -= HandleSpecialTileInteraction;
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
        if (EventSystem.current.IsPointerOverGameObject())
        {
            if (EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.gameObject.tag != "EffectUI")
            {
                Debug.Log("Pointer is over UI, ignoring input.");
                return;
            }
        }

        if (value.Get<float>() > 0.5f)
        {
            Debug.Log("Primary Contact Started");
            touchPosition = inputPosition;
            Debug.Log($"Touch Position: {touchPosition}");
        }
        else
        {
            Debug.Log("Primary Contact Canceled");
            releasePosition = inputPosition;
            Debug.Log($"Release Position: {releasePosition}");

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
        foreach (var dir in path.directions)
        {
            var localScale = transform.localScale;
            if (dir.x != 0)
            {
                localScale.x = dir.x > 0 ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
                transform.localScale = localScale;
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

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"Player took {damage} damage. Current health: {currentHealth}");

        if (currentHealth <= 0)
        {
            OnLoseGame?.Invoke();
        }
    }

    public void HandleCollectItem(ICollectible item)
    {
        if (!collectedItems.Contains(item))
        {
            collectedItems.Add(item);
            item.Collect();
        }
    }

    private void HandleTouchStorm()
    {
        moveSequence.Pause();
        transform.DOShakeRotation(1f, new Vector3(0, 0, 30)).OnComplete(() =>
        {
            transform.rotation = Quaternion.identity;
            foreach (var item in collectedItems)
            {
                item.Release(transform.position);
            }
            collectedItems.Clear();
            moveSequence.Play();
        });

    }

    private void HandleSpecialTileInteraction(SpecialTile tile)
    {
        if (tile is ICollectible collectible)
        {
            HandleCollectItem(collectible);
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"Player healed {amount}. Current health: {currentHealth}");
    }
}