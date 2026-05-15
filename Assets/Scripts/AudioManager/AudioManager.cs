using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[DisallowMultipleComponent]
public class AudioManager : Singleton<AudioManager>
{
    [Header("Pooling")]
    [SerializeField] private int _initialPoolSize = 16;
    [SerializeField] private int _maxPoolSize = 64;

    [Header("Sound Event")]
    [SerializeField] private List<AudioEvent> _sfxEventList = new List<AudioEvent>();
    [SerializeField] private List<AudioEvent> _musicEventList = new List<AudioEvent>();

    [Header("BGM")]
    [SerializeField] private AudioSource _backgroundMusicSource = null;

    private Dictionary<string, AudioEvent> _sfxLookupDict = new Dictionary<string, AudioEvent>();
    private Dictionary<string, AudioEvent> _musicLookupDict = new Dictionary<string, AudioEvent>();
    private List<PooledEmitter> _allEmitter = new List<PooledEmitter>();
    private Queue<PooledEmitter> _availableEmitter = new Queue<PooledEmitter>();
    private Dictionary<string, float> _cooldownByEmitterAndEvent = new Dictionary<string, float>();

    private void BuidAudioLookupDict(List<AudioEvent> soundEventList, Dictionary<string, AudioEvent> lookupDict)
    {
        lookupDict.Clear();

        foreach (AudioEvent soundEvent in soundEventList)
        {
            if (soundEvent == null || string.IsNullOrEmpty(soundEvent.Id))
                continue;

            string key = soundEvent.Id.Trim();

            if (lookupDict.ContainsKey(key))
            {
                Debug.LogWarning($"Duplicate sound event key detected: {key}. Keep first entry");
                continue;
            }

            lookupDict.Add(key, soundEvent);
        }
    }

    private void WarmPool()
    {
        for (int i = 0; i < _initialPoolSize; ++i)
        {
            CreateEmitter();
        }
    }

    private void CreateEmitter()
    {
        // Name emitter based on current quantity
        GameObject emitter = new GameObject($"Audio Emitter {_allEmitter.Count}");
        emitter.transform.SetParent(transform, false);

        AudioSource source = emitter.AddComponent<AudioSource>();
        source.playOnAwake = false;

        PooledEmitter pooledEmitter = new PooledEmitter
        {
            Source = source,
            ReleaseCoroutine = null
        };

        _allEmitter.Add(pooledEmitter);
        _availableEmitter.Enqueue(pooledEmitter);
    }

    protected override void Awake()
    {
        BuidAudioLookupDict(_sfxEventList, _sfxLookupDict);
        BuidAudioLookupDict(_musicEventList, _musicLookupDict);
        WarmPool();
    }

    private void Start()
    {
        //Debug.Log(_musicLookupDict["TestBGM"]);
        //PlayBGM("TestBGM");
    }

    private bool TryAcquireEmitter(out PooledEmitter emitter)
    {
        if (_availableEmitter.Count > 0)
        {
            emitter = _availableEmitter.Dequeue();
            return true;
        }

        if (_allEmitter.Count < _maxPoolSize)
        {
            CreateEmitter();
            emitter = _availableEmitter.Dequeue();
            return true;
        }

        emitter = null;
        return false;
    }

    private void ReleaseEmitter(PooledEmitter emitter)
    {
        if (emitter == null || emitter.Source == null)
        {
            return;
        }

        if (emitter.ReleaseCoroutine != null)
        {
            StopCoroutine(emitter.ReleaseCoroutine);
            emitter.ReleaseCoroutine = null;
        }

        emitter.Source.Stop();
        emitter.Source.clip = null;
        emitter.Source.transform.SetParent(transform, false);
        _availableEmitter.Enqueue(emitter);
    }

    public bool PlaySfx(string soundId, Vector3 position, GameObject emitterOwner = null, Transform followTarget = null)
    {
        if (string.IsNullOrEmpty(soundId))
            return false;

        if (!_sfxLookupDict.TryGetValue(soundId, out var soundEvent))
        {
            return false;
        }

        string cooldownKey = BuildCooldownKey(soundId, emitterOwner);
        if (IsCoolingDown(soundEvent, cooldownKey))
        {
            return false;
        }

        if (!soundEvent.GetRandomSoundVariation(out SoundVariation variation))
        {
            Debug.LogWarning($"AudioManager: Sound '{soundId}' has no valid AudioClip.");
            return false;
        }

        if (!TryAcquireEmitter(out PooledEmitter emitter))
            return false;

        ConfigureSource(emitter.Source, soundEvent, variation, position, false);
        emitter.Source.Play();
        emitter.ReleaseCoroutine = StartCoroutine(ReleaseWhenDone(emitter, followTarget));

        return true;
    }

    public void StopBGM()
    {
        if (_backgroundMusicSource == null)
            return;
        _backgroundMusicSource.Stop();
    }

    public void PlayBGM(string musicId)
    {
        if (string.IsNullOrEmpty(musicId))
        {
            Debug.LogWarning($"Music Id string can't be null or empty");
            return;
        }

        if (!_musicLookupDict.TryGetValue(musicId, out AudioEvent music))
        {
            Debug.LogWarning($"Music Id {musicId} not found in lookup");
            return;
        }

        if (!music.GetRandomSoundVariation(out SoundVariation variation))
        {
            Debug.LogWarning($"Variations for music Id {musicId} not found");
            return;
        }

        StopBGM();
        ConfigureSource(_backgroundMusicSource, music, variation, Vector3.zero, true);
        _backgroundMusicSource.Play();
    }

    private IEnumerator ReleaseWhenDone(PooledEmitter emitter, Transform followTarget)
    {
        AudioSource source = emitter.Source;
        if (source == null || source.clip == null)
        {
            ReleaseEmitter(emitter);
            yield break;
        }

        float duration = source.clip.length / Mathf.Max(0.01f, Mathf.Abs(source.pitch));
        float elapsed = 0f;

        while (elapsed < duration && source.isPlaying)
        {
            if (followTarget != null)
                source.transform.position = followTarget.position;

            elapsed += Time.deltaTime;
            yield return null;
        }

        ReleaseEmitter(emitter);
    }

    private void ConfigureSource(AudioSource source, AudioEvent soundEvent, SoundVariation variation, Vector3 position, bool isLoop)
    {
        source.outputAudioMixerGroup = soundEvent.MixerGroup;
        source.priority = soundEvent.Priority;
        source.spatialBlend = soundEvent.Is3D ? Mathf.Clamp01(soundEvent.SpatialBlend) : 0f;
        source.minDistance = soundEvent.MinDistance;
        source.maxDistance = soundEvent.MaxDistance;
        source.transform.position = position;

        source.pitch = variation.GetPitch();
        source.volume = Mathf.Clamp01(variation.GetVolume());
        source.clip = variation.Clip;
        source.loop = isLoop;
    }

    private string BuildCooldownKey(string soundId, GameObject emitterOwner)
    {
        int ownerId = emitterOwner != null? emitterOwner.GetInstanceID() : 0;
        return $"{ownerId}:{soundId}";
    }

    private bool IsCoolingDown(AudioEvent soundEvent, string cooldownKey)
    {
        if (soundEvent.CooldownSeconds <= 0f)
            return false;

        float now = Time.time;
        if (_cooldownByEmitterAndEvent.TryGetValue(cooldownKey, out float nextAllowedEmitTime) && now < nextAllowedEmitTime)
            return true;

        _cooldownByEmitterAndEvent[cooldownKey] = now + soundEvent.CooldownSeconds;
        return false;
    }
}
