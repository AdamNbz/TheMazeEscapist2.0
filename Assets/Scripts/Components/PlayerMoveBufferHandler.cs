using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerMoveBufferHandler
{
    private class BufferData
    {
        public Vector2Int direction;
        public float timeAdded;
    }

    private readonly Queue<BufferData> moveBuffer = new();
    [SerializeField] private float bufferTime = 0.5f;

    public void Update()
    {
        while (moveBuffer.Count > 0 && Time.time - moveBuffer.Peek().timeAdded > bufferTime)
            moveBuffer.Dequeue();
    }

    public void AddMove(Vector2Int direction)
    {
        Debug.Log($"Buffering move: {direction}");
        moveBuffer.Enqueue(new BufferData { direction = direction, timeAdded = Time.time });
    }

    public Vector2Int? GetBufferedMove()
    {
        if (moveBuffer.Count == 0) return null;
        return moveBuffer.Dequeue().direction;
    }
}