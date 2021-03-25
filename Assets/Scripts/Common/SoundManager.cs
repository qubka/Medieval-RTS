using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class SoundManager : MonoBehaviour
{
    public int maxSounds = 50;
    [Range(0f, 1f)] public float dopplerLevel;
    [Range(0f, 1f)] public float spatialBlend = 1f;
    [Range(0f, 1f)] public float defaultVolume = 1f;
    [Header("Not Sq")]
    public float playRange = 22500f;
    public float soundRange = 100f;

    private Transform camTransform;
    
    private Dictionary<Vector3, (Sounds, int)> clipTable;
    private Dictionary<AudioSource, (Vector3, int)> playTable;
    
    private AudioSource[] sources;
    private List<AudioSource> availables;
    
    private void Start()
    {
        camTransform = Manager.camTransform;
        
        clipTable = new Dictionary<Vector3, (Sounds, int)>(1000);
        playTable = new Dictionary<AudioSource, (Vector3, int)>(maxSounds);

        sources = new AudioSource[maxSounds];
        availables = new List<AudioSource>(maxSounds);
        
        for (var i = 0; i < maxSounds; i++) {
            var source = new GameObject("Distant shot audio").AddComponent<AudioSource>();
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
            var type = sounds.name.GetHashCode();
            
            // Check current sounds
            foreach (var pair in clipTable) {
                // If we already have sound nearby
                if (Vector.DistanceSq(pair.Key, position) < soundRange) {
                    // If that sound is the same type, then skip
                    if (type == pair.Value.Item2) {
                        return;
                    }
                }
            }
            
            // Check playing sounds
            /*foreach (var pair in playTable) {
                // If we already have sound nearby
                if (Vector.DistanceSq(pair.Value.Item1, position) < soundRange) {
                    // If that sound is the same type, then skip
                    if (type == pair.Value.Item2) {
                        return;
                    }
                }
            }*/

            if (clipTable.ContainsKey(position)) {
                clipTable[position] = (sounds, type);
            } else {
                clipTable.Add(position, (sounds, type));
            }
        }
    }
    
    public void LateUpdate()
    {
        foreach (var source in sources) {
            if (!source.isPlaying) {
                availables.Add(source);
                playTable.Remove(source);
            }
        }

        var index = availables.Count - 1;
        if (index >= 0) {
            //var currentTime = AudioSettings.dspTime;
            
            var listener = camTransform.position;
            foreach (var pair in clipTable.OrderBy(p => Vector.DistanceSq(listener, p.Key))) {
                var pos = pair.Key;
                var (sounds, type) = pair.Value;
                
                var source = availables[index];
                source.transform.position = pos;
                source.clip = sounds.sounds.GetRandom();
                source.pitch = Random.Range(0.995f, 1.005f);

                playTable.Add(source, (pos, type));

                source.Play();

                index--;
                if (index <= 0)
                    break;
            }
            availables.Clear();
        }
        
        clipTable.Clear();
    }
}