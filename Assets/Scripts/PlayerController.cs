using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    [SerializeField] bool lockMoving = false;
    
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
        var sequence = DOTween.Sequence();
        var currentPos = transform.position;
        foreach (var dir in path.directions)
        {
            sequence.Append(transform.DOMove(currentPos + new Vector3(dir.x, dir.y, 0), 0.1f)
                .SetEase(Ease.Linear));
            currentPos += new Vector3(dir.x, dir.y, 0);
        }
        sequence.OnComplete(() => lockMoving = false);
    }
}