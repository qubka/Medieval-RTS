using System;
using System.Collections.Generic;
using System.Linq;
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
    public AudioClip ambientSound;
    public AnimationCurve ambientCurve;
    
#pragma warning disable 108,114
    private AudioSource audio;
#pragma warning restore 108,114
    //private Transform worldTransform;
    private Transform camTransform;
    private CamController camController;

    private readonly Dictionary<Vector3, Sounds> clipTable = new Dictionary<Vector3, Sounds>(1000);
    //private readonly Dictionary<AudioSource, Vector3> playTable = new Dictionary<AudioSource, Vector3>();
    private readonly List<AudioSource> sources = new List<AudioSource>();
    private readonly List<AudioSource> available = new List<AudioSource>();
    
    protected override void Awake()
    {
        base.Awake();
        
        playRange *= playRange;
        soundRange *= soundRange;
        sources.Capacity = maxSounds;
        available.Capacity = maxSounds;
    }

    private void Start()
    {
        audio = Manager.cameraSources[2];
        audio.loop = true;
        audio.clip = ambientSound;
        audio.Play();
        
        camTransform = Manager.camTransform;
        camController = Manager.camController;
        
        for (var i = 0; i < maxSounds; i++) {
            var source = new GameObject("Audio shot").AddComponent<AudioSource>();
            source.dopplerLevel = dopplerLevel;
            source.spatialBlend = spatialBlend;
            source.volume = defaultVolume;
            source.loop = false;
            source.playOnAwake = false;
            sources.Add(source);
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
        audio.volume = ambientCurve.Evaluate(MathExtention.Clamp01(camController.DistToGround));
        
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
}