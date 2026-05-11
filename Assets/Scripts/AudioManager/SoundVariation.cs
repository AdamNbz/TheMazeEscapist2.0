using UnityEngine;

[System.Serializable]
public class SoundVariation
{
    [SerializeField] private AudioClip _clip = null;
    [SerializeField] private Vector2 _volumeRange = new Vector2(-1f, 1f);
    [SerializeField] private Vector2 _pitchRange = new Vector2(-1f, 1f);

    public AudioClip Clip => _clip;
    public float GetVolume() => Random.Range(_volumeRange.x, _volumeRange.y);
    public float GetPitch() => Random.Range(_pitchRange.x, _pitchRange.y);
}
