using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    [SerializeField] bool lockMoving = false;

    private Sequence moveSequence;

    void OnEnable()
    {
        WinPoint.OnLevelComplete += CompleteLevel;
    }

    void OnDisable()
    {
        WinPoint.OnLevelComplete -= CompleteLevel;
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

    public void OnJump()
    {
        Debug.Log("Jump!");
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

    public void MoveWithPath(Path path)
    {
        lockMoving = true;
        moveSequence = DOTween.Sequence();
        var currentPos = transform.position;
        foreach (var dir in path.directions)
        {
            moveSequence.Append(transform.DOMove(currentPos + new Vector3(dir.x, dir.y, 0) * path.stepLength, 0.1f)
                .SetEase(Ease.Linear));
            currentPos += new Vector3(dir.x, dir.y, 0) * path.stepLength;
        }
        moveSequence.OnComplete(() => lockMoving = false);
    }
}