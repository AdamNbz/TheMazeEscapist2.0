using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using static UnityEngine.InputSystem.InputAction;

public class PlayerController : MonoBehaviour
{
    [SerializeField] bool lockMoving = false;
    private Vector2 touchPosition = Vector2.zero;
    private Vector2 releasePosition = Vector2.zero;
    private Vector2 inputPosition = Vector2.zero;

    private Sequence moveSequence;
    private Animator animator;

    void OnEnable()
    {
        WinPoint.OnLevelComplete += CompleteLevel;
    }

    void OnDisable()
    {
        WinPoint.OnLevelComplete -= CompleteLevel;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void CompleteLevel()
    {
        lockMoving = true;
        moveSequence?.Kill();

        PlayerProgress.UnlockNextLevel();
        SceneController.Instance.TransitionToScene($"Level {PlayerProgress.CurrentLevel}");
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
        // Debug.Log($"Primary Position: {inputPosition}");
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
        moveSequence.OnComplete(() => {
            lockMoving = false;
            if (animator != null)
            {
                animator.Play("Idle");
            }
        });
    }
}