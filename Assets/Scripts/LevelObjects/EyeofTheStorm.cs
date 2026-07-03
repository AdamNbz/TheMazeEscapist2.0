using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class EyeofTheStorm : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private Transform player;
    [SerializeField] private Vector3 gridAnchor = new(0.5f, 0.5f, 0);

    private Queue<Node> currentPath = new();
    public static event Action OnTouchPlayer;

    private PathFindingLogic pathfindingLogic;
    private bool isMoving = false;

    private bool isWandering = true;

    private void Start()
    {
        pathfindingLogic = new PathFindingLogic();
    }

    void OnEnable()
    {
        PlayerController.OnTurnMove += UpdateNewPath;
        SpecialTile.OnSpecialTileInteracted += HandleTrashCollected;
    }

    void OnDisable()
    {
        PlayerController.OnTurnMove -= UpdateNewPath;
        SpecialTile.OnSpecialTileInteracted -= HandleTrashCollected;
    }

    private void UpdateNewPath()
    {
        currentPath.Clear();
        var path = pathfindingLogic.FindPathFromWorldPos(transform.position, isWandering ?
        GridManager.Instance.GetRandomWalkableCellPosition() : player.position);
        foreach (var node in path)
        {
            currentPath.Enqueue(node);
        }
    }

    private async UniTask MoveWithCurrentPath()
    {
        isMoving = true;
        while (currentPath.Count > 0)
        {
            var nextNode = currentPath.Dequeue();
            Vector3 targetPosition = GridManager.Instance.CellToWorld(nextNode.position);

            try
            {
                await transform.DOMove(targetPosition + gridAnchor, 1f / speed)
                    .SetEase(Ease.Linear).ToUniTask(cancellationToken: destroyCancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
        }
        isMoving = false;
    }

    void Update()
    {
        if (!isMoving && currentPath.Count > 0)
        {
            MoveWithCurrentPath().Forget();
        }
        else if (!isMoving && currentPath.Count == 0)
        {
            UpdateNewPath();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            OnTouchPlayer?.Invoke();
            // gameObject.SetActive(false);
            isWandering = true;
        }
    }

    void HandleTrashCollected(SpecialTile data)
    {
        if (data.Type == TileType.Trash || data.Type == TileType.StudentCard)
        {
            isWandering = false;
        }
    }
}