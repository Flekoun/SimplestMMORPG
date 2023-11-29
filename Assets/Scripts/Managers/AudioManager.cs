using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class AudioManager : MonoBehaviour
{
    public AudioSource Music;
    public AudioSource Sound;
    //Here is a private reference only this class can access
    private static AudioManager _instance;

    //This is the public reference that other classes will use
    public static AudioManager instance
    {
        get
        {
            //If _instance hasn't been set yet, we grab it from the scene!
            //This will only happen the first time this reference is used.
            if (_instance == null)
                _instance = GameObject.FindObjectOfType<AudioManager>();
            return _instance;
        }
    }


    public void Play(AudioClipSO _clip)
    {
        if (_clip.IsSound)
            Sound.PlayOneShot(_clip.AudioClip);
        else
            Music.PlayOneShot(_clip.AudioClip);
    }



}
