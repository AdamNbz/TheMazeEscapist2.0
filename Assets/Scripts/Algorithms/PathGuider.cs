using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PathGuider : MonoBehaviour
{
    [SerializeField] private bool toggleButton;
    [SerializeField] private PlayerController player;
    [SerializeField] private Vector3 gridAnchor = new(0.5f, 0.5f, 0);
    [SerializeField] private List<SpecialTile> specialTiles = new();
    private bool isFindingPath = false;
    private PathFindingHandler pathFindingHandler;
    private Sequence moveSequence;

    void Start()
    {
        pathFindingHandler = new PathFindingHandler();
    }

    void OnEnable()
    {
        SpecialTile.OnSpecialTileInstantiated += HandleSpecialTileInstantiated;
    }

    void OnDisable()
    {
        SpecialTile.OnSpecialTileInstantiated -= HandleSpecialTileInstantiated;
    }

    void Update()
    {
        if (toggleButton)
        {
            toggleButton = false;
            FindPath();
        }
    }

    public void FindPath()
    {
        if (isFindingPath || specialTiles.Count == 0) return;
        Debug.Log("Finding path...");
        var path = pathFindingHandler.FindPathFromPlayer(player.transform.position, specialTiles[0]);
        isFindingPath = true;
        transform.position = player.transform.position;
        moveSequence = DOTween.Sequence();

        if (path == null)
        {
            Debug.Log("No path found!");
            isFindingPath = false;
            return;
        }

        foreach (var node in path)
            moveSequence.Append(transform.DOMove(GridManager.Instance.CellToWorld(node.position) + gridAnchor, 0.1f)
                .SetEase(Ease.Linear));
        moveSequence.OnComplete(() => isFindingPath = false);
    }

    private void HandleSpecialTileInstantiated(SpecialTile data)
    {
        specialTiles.Add(data);
    }

    private void HandleTrashCollected(Trash trash)
    {
        if (specialTiles.Contains(trash))
            specialTiles.Remove(trash);
    }

    
}