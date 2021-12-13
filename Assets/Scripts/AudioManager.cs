using UnityEngine.Audio;
using UnityEngine;
using System;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;
    [SerializeField] HexGrid grid;
    // Start is called before the first frame update
    void Start()
    {
        foreach (Sound s in sounds)
        {
            if (s.name == "Boat")
            {
                s.source = gameObject.AddComponent<AudioSource>();
                s.source.clip = s.clip;
                s.source.volume = s.volume;
                s.source.pitch = s.pitch;
                s.source.loop = s.loop;
            }
            else if (s.name == "Waves")
            {
                s.source = gameObject.AddComponent<AudioSource>();
                s.source.clip = s.clip;
                s.source.volume = s.volume;
                s.source.pitch = s.pitch;
                s.source.loop = s.loop;
                Play("Waves", 0);
            }
            else if (s.name == "Seagulls")
            {
                s.source = grid.gameObject.AddComponent<AudioSource>();
                s.source.clip = s.clip;
                s.source.volume = s.volume;
                s.source.pitch = s.pitch;
                s.source.loop = s.loop;
                Play("Seagulls", 300);
            }
            else
            {
                s.source = gameObject.AddComponent<AudioSource>();
                s.source.clip = s.clip;
                s.source.volume = s.volume;
                s.source.pitch = s.pitch;
                s.source.loop = s.loop;
            }
        }
    }

    public void Play(string soundName, float interval)
    {
        Sound s = Array.Find(sounds, sound => sound.name == soundName);
        if (s != null)
        {
            s.source.Play();
            if (interval > 0)
            {
                StartCoroutine(Repeat(interval, s.source));
            }
        }
    }

    public void Stop(string soundName)
    {
        Sound s = Array.Find(sounds, sound => sound.name == soundName);
        if (s != null)
        {
            s.source.Stop();
        }
    }

    IEnumerator Repeat(float interval, AudioSource source)
    {
        yield return new WaitForSeconds(interval);
        source.Play();
    }

    bool IsPlaying(string soundName)
    {
        Sound s = Array.Find(sounds, sound => sound.name == soundName);
        if (s.source.isPlaying)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}