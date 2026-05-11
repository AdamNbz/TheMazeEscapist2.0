using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class AudioEvent
{
    [SerializeField] private string _id = "walk";
    [SerializeField] private AudioMixerGroup _mixerGroup = null;
    [SerializeField] private bool _is3D = true;
    [SerializeField] private float _spatialBlend = 1f;
    [SerializeField] private float _minDistance = 1f;
    [SerializeField] private float _maxDistance = 25f;
    [SerializeField] private int _priority = 128;
    [SerializeField] private float _cooldownSeconds = 0.05f;
    [SerializeField] private List<SoundVariation> _variationList = new List<SoundVariation>();

    public string Id => _id;
    public AudioMixerGroup MixerGroup => _mixerGroup;
    public bool Is3D => _is3D;
    public float SpatialBlend => _spatialBlend;
    public float MinDistance => _minDistance;
    public float MaxDistance => _maxDistance;
    public int Priority => _priority;
    public float CooldownSeconds => _cooldownSeconds;
    public IReadOnlyList<SoundVariation> SoundVariations => _variationList;

    public bool GetRandomSoundVariation(out SoundVariation soundVariation)
    {
        if (_variationList == null || _variationList.Count == 0)
        {
            soundVariation = null;
            return false;
        }

        int index = Random.Range(0, _variationList.Count - 1);
        soundVariation = _variationList[index];
        return soundVariation != null && soundVariation.Clip != null;
    }
}
