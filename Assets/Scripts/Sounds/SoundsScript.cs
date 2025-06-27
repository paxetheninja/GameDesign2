using System;
using UnityEngine;

public class SoundsScript : MonoBehaviour
{
    public AudioClip gridPickup;
    public AudioClip gridPlace;
    public AudioClip gridRotate;
    public AudioClip doorOpen;

    public static SoundsScript Instance;

    private AudioSource _audioSource;
    
    private void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(gameObject); 
        } 
        else 
        { 
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        _audioSource = gameObject.GetComponent<AudioSource>();
    }

    public void SoundGridPickup()
    {
        MakeSound(gridPickup);
    }

    public void SoundGridPlace()
    {
        MakeSound(gridPlace);
    }
    
    public void SoundGridRotate()
    {
        MakeSound(gridRotate, 0.3f);
    }

    public void SoundDoorOpen()
    {
        MakeSound(doorOpen, 0.4f);
    }

    private void MakeSound(AudioClip originalClip, float volume=1.0f)
    {
        _audioSource.PlayOneShot(originalClip, volume);
    }
}
