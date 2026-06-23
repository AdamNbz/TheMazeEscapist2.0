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

    [SerializeField] private int pathCount;
    private Queue<Node> currentPath = new();

    private PathFindingLogic pathfindingLogic;
    private bool isMoving = false;

    private void Start()
    {
        pathfindingLogic = new PathFindingLogic();
    }

    void OnEnable()
    {
        PlayerController.OnTurnMove += UpdateNewPath;
    }

    void OnDisable()
    {
        PlayerController.OnTurnMove -= UpdateNewPath;
    }

    private void UpdateNewPath()
    {
        currentPath.Clear();
        var path = pathfindingLogic.FindPathFromWorldPos(transform.position, player.position);
        Debug.Log($"Eye of the Storm: New path found with {path.Count} nodes.");
        foreach (var node in path)
        {
            Debug.Log($"Eye of the Storm: Adding node {node.position} to current path.");
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
}