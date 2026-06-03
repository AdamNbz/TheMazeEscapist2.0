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
    [SerializeField] private PlayerMoveBufferHandler moveBufferHandler;
    private Vector2 touchPosition = Vector2.zero;
    private Vector2 releasePosition = Vector2.zero;
    private Vector2 inputPosition = Vector2.zero;

    private Sequence moveSequence;
    private Animator animator;

    public static UnityAction OnLoseGame;
    public static UnityAction OnTurnMove;
    public static UnityAction OnStartMoving;

    void OnEnable()
    {
        WinPoint.OnLevelComplete += LockInput;
        TurnTimer.OnTimeOut += LockInput;
        LevelTimer.OnTimeOut += LockInput;
        Portal.OnPlayerTeleport += HandleTeleport;
        OnLoseGame += HandleLoseGame;
    }

    void OnDisable()
    {
        WinPoint.OnLevelComplete -= LockInput;
        TurnTimer.OnTimeOut -= LockInput;
        LevelTimer.OnTimeOut -= LockInput;
        Portal.OnPlayerTeleport -= HandleTeleport;
        OnLoseGame -= HandleLoseGame;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void LockInput()
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

    void Update()
    {
        moveBufferHandler.Update();
    }

    public void OnMove(InputValue value)
    {

        Vector2 input = value.Get<Vector2>();
        Debug.Log($"Move: {input}");

        var direction = Vector2Int.RoundToInt(input);
        if (direction == Vector2Int.zero)
            return;

        HandleInput(direction);
    }

    public void OnPrimaryContact(InputValue value)
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("Pointer is over UI, ignoring input.");
            return;
        }

        if (value.Get<float>() > 0.5f)
        {
            // Start detecting swipe
            touchPosition = inputPosition;
        }
        else
        {
            // End of swipe, determine direction
            releasePosition = inputPosition;

            var direction = Vector2Int.zero;
            var swipeVector = releasePosition - touchPosition;

            if (Mathf.Abs(swipeVector.x) > Mathf.Abs(swipeVector.y))
                direction.x = swipeVector.x > 0 ? 1 : -1;
            else
                direction.y = swipeVector.y > 0 ? 1 : -1;

            HandleInput(direction);
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
        moveSequence.OnComplete(EndMoving);
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

        data.LinkedPortal.UnlockPortal();
        EndMoving();
    }

    private void EndMoving()
    {
        lockMoving = false;
        if (animator != null)
            animator.Play("Idle");

        var bufferedMove = moveBufferHandler.GetBufferedMove();
        if (bufferedMove.HasValue)
        {
            var newPath = GridManager.Instance.FindPathFromWorld(transform.position, bufferedMove.Value);
            MoveWithPath(newPath);
        }
        OnTurnMove?.Invoke();
    }

    private void HandleInput(Vector2Int direction)
    {
        if (lockMoving)
            moveBufferHandler.AddMove(direction);
        else
        {
            var path = GridManager.Instance.FindPathFromWorld(transform.position, direction);
            MoveWithPath(path);
        }
    }
}