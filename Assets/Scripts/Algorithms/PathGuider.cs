using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PathGuider : MonoBehaviour
{
    [SerializeField] private bool toggleButton;
    [SerializeField] private PlayerController player;
    [SerializeField] private Vector3 gridAnchor = new(0.5f, 0.5f, 0);
    [SerializeField] private List<SpecialTile> specialTiles = new();
    [SerializeField] private List<TargetSpecialTile> targetSpecialTiles = new();
    [SerializeField] private Button findPathButton;
    private bool isFindingPath = false;
    private PathFindingLogic pathFindingHandler = null;
    private Sequence moveSequence;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false;
    }

    void OnEnable()
    {
        SpecialTile.OnSpecialTileInstantiated += HandleSpecialTileInstantiated;
        SpecialTile.OnSpecialTileInteracted += HandleSpecialTileInteracted;
        PlayerController.OnStartMoving += DisableButton;
        PlayerController.OnTurnMove += EnableButton;
    }

    void OnDisable()
    {
        SpecialTile.OnSpecialTileInstantiated -= HandleSpecialTileInstantiated;
        SpecialTile.OnSpecialTileInteracted -= HandleSpecialTileInteracted;
        PlayerController.OnStartMoving -= DisableButton;
        PlayerController.OnTurnMove -= EnableButton;
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
        pathFindingHandler ??= new PathFindingLogic();
        if (isFindingPath) return;

        transform.position = player.transform.position;
        moveSequence = DOTween.Sequence();

        ReloadTargetSpecialTiles();
        var currentPos = transform.position;

        isFindingPath = true;
        spriteRenderer.enabled = true;
        findPathButton.interactable = false;
        AudioManager.Instance.PlaySfx("path_finding", transform.position);

        foreach (var target in targetSpecialTiles)
        {
            var path = pathFindingHandler.FindPathFromPlayer(currentPos, target.tile);
            foreach (var node in path)
            {
                moveSequence.Append(transform.DOMove(GridManager.Instance.CellToWorld(node.position) + gridAnchor, 0.1f))
                    .SetEase(Ease.Linear);
            }
            moveSequence.AppendInterval(0.5f);
            currentPos = target.tile.transform.position;
        }


        moveSequence.OnComplete(() =>
        {
            isFindingPath = false;
            spriteRenderer.enabled = false;
            findPathButton.interactable = true;
        });
    }

    private void HandleSpecialTileInstantiated(SpecialTile data)
    {
        specialTiles.Add(data);
    }

    private void HandleSpecialTileInteracted(SpecialTile data)
    {
        if (data.Type == TileType.Trash || data.Type == TileType.StudentCard || data.Type == TileType.WinPoint)
        {
            if (specialTiles.Contains(data))
                specialTiles.Remove(data);
        }
    }

    private void ReloadTargetSpecialTiles()
    {
        pathFindingHandler ??= new PathFindingLogic();
        targetSpecialTiles.Clear();
        Debug.Log("Reloading target special tiles...");
        var trashCount = 0;
        foreach (var tile in specialTiles)
        {
            if (tile.Type == TileType.Trash || tile.Type == TileType.StudentCard || tile.Type == TileType.WinPoint)
            {
                if (tile.Type == TileType.Trash)
                    trashCount++;

                var path = pathFindingHandler.FindPathFromPlayer(player.transform.position, tile);

                Debug.Log($"Distance to {tile.Type} at {tile.transform.position}: {path?.Count ?? int.MaxValue}");
                targetSpecialTiles.Add(new TargetSpecialTile { tile = tile, distance = path?.Count ?? int.MaxValue });
            }
        }
        targetSpecialTiles.Sort((a, b) => a.distance.CompareTo(b.distance));
        if (trashCount > 0)
        {
            var recycleBin = specialTiles.Find(t => t.Type == TileType.RecycleBin);
            if (recycleBin != null)
            {
                var distance = pathFindingHandler.FindPathFromPlayer(player.transform.position, recycleBin)?.Count ?? int.MaxValue;
                bool inserted = false;
                for (int i = 0; i < targetSpecialTiles.Count; i++)
                {
                    if (trashCount <= 0 && targetSpecialTiles[i].distance > distance)
                    {
                        targetSpecialTiles.Insert(i, new TargetSpecialTile { tile = recycleBin, distance = distance });
                        inserted = true;
                        break;
                    }

                    if (targetSpecialTiles[i].tile.Type == TileType.Trash)
                        trashCount--;
                }
                if (!inserted)
                    targetSpecialTiles.Add(new TargetSpecialTile { tile = recycleBin, distance = distance });
            }
        }
    }

    public void DisableButton()
    {
        if (findPathButton != null)
            findPathButton.interactable = false;
    }

    public void EnableButton()
    {
        if (findPathButton != null && !isFindingPath)
            findPathButton.interactable = true;
    }
}

[Serializable]
public struct TargetSpecialTile
{
    public SpecialTile tile;
    public int distance;
}