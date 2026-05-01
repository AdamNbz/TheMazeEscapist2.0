using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] bool lockMoving = false;
    //new input system
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
        var _ = TestingMovingAnim(path);
    }

    public async Awaitable TestingMovingAnim(Path path)
    {
        lockMoving = true;
        foreach (var dir in path.directions)
        {
            //play anim
            await Awaitable.WaitForSecondsAsync(0.5f);
            transform.position += new Vector3(dir.x, dir.y, 0);
        }
        lockMoving = false;
    }
}