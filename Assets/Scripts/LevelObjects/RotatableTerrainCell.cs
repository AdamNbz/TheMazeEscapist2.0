using UnityEngine;

public class RotatableTerrainCell : MonoBehaviour
{
    [SerializeField] private bool blocksMovement = true;

    public bool BlocksMovement => blocksMovement;
}
