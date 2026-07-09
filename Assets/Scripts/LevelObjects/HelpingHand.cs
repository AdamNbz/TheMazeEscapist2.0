using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class HelpingHand : MonoBehaviour
{
    void Start()
    {
        // loop move up and down
        transform.DOMoveY(transform.position.y + 0.5f, 1f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }

    void OnEnable()
    {
        RotatableTerrainBlock.RotationStartedGlobal += OnRotationStarted;
    }

    void OnDisable()
    {
        RotatableTerrainBlock.RotationStartedGlobal -= OnRotationStarted;
    }

    private void OnRotationStarted()
    {
        // Disable the helping hand when rotation starts
        Destroy(gameObject);
    }
}