using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class SoundManager : SingletonObject<SoundManager>
{
    [Header(("Normal"))]
    public int maxSounds = 50;
    [Range(0f, 1f)] public float dopplerLevel;
    [Range(0f, 1f)] public float spatialBlend = 1f;
    [Range(0f, 1f)] public float defaultVolume = 1f;
    [Space]
    public float playRange = 150f;
    public float soundRange = 10f;
    [Header(("Ambient"))]
    public Ambient[] ambients;

    //private Transform worldTransform;
    private Transform camTransform;
    private CamController camController;
    private AudioSource[] sources;
    private readonly List<AudioSource> available = new List<AudioSource>();
    private readonly Dictionary<Vector3, Sounds> clipTable = new Dictionary<Vector3, Sounds>(1000);
    //private readonly Dictionary<AudioSource, Vector3> playTable = new Dictionary<AudioSource, Vector3>();

    protected override void Awake()
    {
        base.Awake();
        
        playRange *= playRange;
        soundRange *= soundRange;
        sources = new AudioSource[maxSounds];
        available.Capacity = maxSounds;
    }

    private void Start()
    {
        var camera = Manager.mainCamera.gameObject;
        foreach (var ambient in ambients) {
            var source = camera.AddComponent<AudioSource>();
            source.dopplerLevel = 0f;
            source.loop = true;
            source.clip = ambient.sound;
            source.volume = 0f;
            source.Play();
            ambient.audio = source;
        }

        camTransform = Manager.camTransform;
        camController = Manager.camController;
        
        for (var i = 0; i < maxSounds; i++) {
            var source = new GameObject("Audio shot").AddComponent<AudioSource>();
            source.dopplerLevel = dopplerLevel;
            source.spatialBlend = spatialBlend;
            source.volume = defaultVolume;
            source.loop = false;
            source.playOnAwake = false;
            sources[i] = source;
        }
    }

    public void RequestPlaySound(Vector3 position, Sounds sounds)
    {
        var listener = camTransform.position;
        
        if (Vector.DistanceSq(listener, position) <= playRange) {
            //if (sounds.sounds.Count == 0) Debug.Log(sounds.name);
            
            // Check current sounds
            foreach (var pair in clipTable) {
                // If we already have sound nearby
                if (Vector.DistanceSq(pair.Key, position) < soundRange) {
                    // If that sound is the same type, then skip
                    if (sounds == pair.Value) {
                        return;
                    }
                }
            }
            
            // Check playing sounds
            /*foreach (var pair in playTable) {
                // If we already have sound nearby
                if (Vector.DistanceSq(pair.Value.Item1, position) < soundRange) {
                    // If that sound is the same type, then skip
                    if (type == pair.Value.id) {
                        return;
                    }
                }
            }*/

            if (clipTable.ContainsKey(position)) {
                clipTable[position] = sounds;
            } else {
                clipTable.Add(position, sounds);
            }
        }
    }

    public void LateUpdate()
    {
        var time = CampaignTime.Instance.TimeDelta;
        var volume = 1f - MathExtention.Clamp01(camController.DistToGround);

        foreach (var ambient in ambients) {
            ambient.Update(time, volume);
        }
        
        foreach (var source in sources) {
            if (!source.isPlaying) {
                available.Add(source);
                //playTable.Remove(source);
            }
        }

        var index = available.Count - 1;
        if (index >= 0) {
            //var currentTime = AudioSettings.dspTime;
            
            var listener = camTransform.position;
            foreach (var pair in clipTable.OrderBy(p => Vector.DistanceSq(listener, p.Key))) {
                var pos = pair.Key;
                var source = available[index];
                source.transform.position = pos;
                source.clip = pair.Value.Clip;
                source.pitch = Random.Range(0.995f, 1.005f);

                //playTable.Add(source, pos);

                source.Play();

                index--;
                if (index <= 0)
                    break;
            }
            available.Clear();
        }
        
        clipTable.Clear();
    }

    [Serializable]
    public class Ambient
    {
        [HideInInspector] public AudioSource audio;
        public AudioClip sound;
        public AnimationCurve curve;
        [MinTo(0f, 24f)] public Vector2 time;
        private float currentVelocity;

        public void Update(float delta, float volume)
        {
            if (delta >= time.x && delta <= time.y) {
                audio.volume = Mathf.SmoothDamp(audio.volume, curve.Evaluate(volume), ref currentVelocity, 1f);
            } else {
                audio.volume = Mathf.SmoothDamp(audio.volume, 0f, ref currentVelocity, 1f);
            }
        }
    }
}