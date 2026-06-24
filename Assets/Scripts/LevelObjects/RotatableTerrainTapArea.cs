using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class RotatableTerrainTapArea : MonoBehaviour
{
    [SerializeField] private RotatableTerrainBlock targetBlock;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private GameObject highlightRoot;
    [SerializeField] private SpriteRenderer[] highlightRenderers;
    [SerializeField] private Color highlightColor = new(1f, 0.82f, 0.18f, 0.35f);
    [SerializeField] private float minHighlightAlpha = 0.2f;
    [SerializeField] private float maxHighlightAlpha = 0.45f;
    [SerializeField] private float highlightPulseSpeed = 2f;
    [SerializeField] private float maxTapMoveDistance = 40f;
    [SerializeField] private int clockwiseQuarterTurns = 1;
    [SerializeField] private bool hideHighlightWhileRotating = true;
    [SerializeField] private bool lockPlayerWhileRotating = true;

    private Collider2D tapCollider;
    private bool pointerStartedInside;
    private bool lockedPlayers;
    private Vector2 pointerStartPosition;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Awake()
    {
        tapCollider = GetComponent<Collider2D>();
        tapCollider.isTrigger = true;

        if (targetBlock == null)
            targetBlock = GetComponentInParent<RotatableTerrainBlock>();

        if (targetCamera == null)
            targetCamera = Camera.main;

        AutoCollectHighlightRenderersIfNeeded();
        SetHighlightVisible(true);
    }

    private void OnEnable()
    {
        if (targetBlock != null)
            targetBlock.RotationFinished += HandleRotationFinished;
    }

    private void OnDisable()
    {
        if (targetBlock != null)
            targetBlock.RotationFinished -= HandleRotationFinished;

        UnlockPlayers();
    }

    private void Update()
    {
        UpdateHighlightPulse();

        if (TryHandleTouchscreen())
            return;

        TryHandleMouse();
    }

    private bool TryHandleTouchscreen()
    {
        var touchscreen = Touchscreen.current;
        if (touchscreen == null)
            return false;

        var touch = touchscreen.primaryTouch;
        var position = touch.position.ReadValue();
        var touchId = touch.touchId.ReadValue();

        if (touch.press.wasPressedThisFrame)
            BeginPointer(position, touchId);

        if (touch.press.wasReleasedThisFrame)
            EndPointer(position, touchId);

        return touch.press.isPressed || touch.press.wasPressedThisFrame || touch.press.wasReleasedThisFrame;
    }

    private void TryHandleMouse()
    {
        var mouse = Mouse.current;
        if (mouse == null)
            return;

        var position = mouse.position.ReadValue();

        if (mouse.leftButton.wasPressedThisFrame)
            BeginPointer(position, -1);

        if (mouse.leftButton.wasReleasedThisFrame)
            EndPointer(position, -1);
    }

    private void BeginPointer(Vector2 screenPosition, int pointerId)
    {
        if (IsPointerOverUi(pointerId))
            return;

        pointerStartedInside = ContainsScreenPosition(screenPosition);
        pointerStartPosition = screenPosition;
    }

    private void EndPointer(Vector2 screenPosition, int pointerId)
    {
        if (!pointerStartedInside)
            return;

        pointerStartedInside = false;

        if (IsPointerOverUi(pointerId))
            return;

        var movedDistance = Vector2.Distance(pointerStartPosition, screenPosition);
        if (movedDistance > maxTapMoveDistance || !ContainsScreenPosition(screenPosition))
            return;

        TryRotateTargetBlock();
    }

    private void TryRotateTargetBlock()
    {
        if (targetBlock == null)
        {
            Debug.LogWarning($"{nameof(RotatableTerrainTapArea)} has no target block assigned.", this);
            return;
        }

        if (targetBlock.IsRotating)
            return;

        if (lockPlayerWhileRotating)
            LockPlayers();

        if (!targetBlock.TryRotateClockwiseTurns(clockwiseQuarterTurns))
        {
            UnlockPlayers();
            return;
        }

        if (hideHighlightWhileRotating)
            SetHighlightVisible(false);
    }

    private bool ContainsScreenPosition(Vector2 screenPosition)
    {
        var cameraToUse = targetCamera != null ? targetCamera : Camera.main;
        if (cameraToUse == null)
            return false;

        var distanceToArea = Mathf.Abs(cameraToUse.transform.position.z - transform.position.z);
        var worldPosition = cameraToUse.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, distanceToArea));
        return tapCollider.OverlapPoint(worldPosition);
    }

    private bool IsPointerOverUi(int pointerId)
    {
        if (EventSystem.current == null)
            return false;

        if (pointerId >= 0 && EventSystem.current.IsPointerOverGameObject(pointerId))
            return true;

        return EventSystem.current.IsPointerOverGameObject();
    }

    private void LockPlayers()
    {
        if (lockedPlayers)
            return;

        PlayerController.SetAllExternalMovementLocks(true);
        lockedPlayers = true;
    }

    private void UnlockPlayers()
    {
        if (!lockedPlayers)
            return;

        PlayerController.SetAllExternalMovementLocks(false);
        lockedPlayers = false;
    }

    private void HandleRotationFinished(RotatableTerrainBlock finishedBlock)
    {
        UnlockPlayers();

        if (hideHighlightWhileRotating)
            SetHighlightVisible(true);
    }

    private void AutoCollectHighlightRenderersIfNeeded()
    {
        if (highlightRenderers != null && highlightRenderers.Length > 0)
            return;

        if (highlightRoot != null)
        {
            highlightRenderers = highlightRoot.GetComponentsInChildren<SpriteRenderer>(true);
            return;
        }

        highlightRenderers = GetComponents<SpriteRenderer>();
    }

    private void SetHighlightVisible(bool isVisible)
    {
        if (highlightRoot != null)
            highlightRoot.SetActive(isVisible);

        if (highlightRenderers == null)
            return;

        foreach (var spriteRenderer in highlightRenderers)
        {
            if (spriteRenderer != null)
                spriteRenderer.enabled = isVisible;
        }
    }

    private void UpdateHighlightPulse()
    {
        if (highlightRenderers == null || highlightRenderers.Length == 0)
            return;

        var alpha = Mathf.Lerp(minHighlightAlpha, maxHighlightAlpha, (Mathf.Sin(Time.time * highlightPulseSpeed) + 1f) * 0.5f);
        var color = highlightColor;
        color.a = alpha;

        foreach (var spriteRenderer in highlightRenderers)
        {
            if (spriteRenderer != null && spriteRenderer.enabled)
                spriteRenderer.color = color;
        }
    }
}
