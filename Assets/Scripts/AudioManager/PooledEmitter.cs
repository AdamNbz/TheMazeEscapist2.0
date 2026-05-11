using UnityEngine;

public class PooledEmitter
{
    public AudioSource Source = null;
    public Coroutine ReleaseCoroutine = null;
}
