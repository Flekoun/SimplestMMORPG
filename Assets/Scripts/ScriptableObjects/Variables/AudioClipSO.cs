using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "XCOM/AudioClip")]
public class AudioClipSO : ScriptableObject
{
    [Tooltip("Otherwise its music")]
    public bool IsSound = true;
    public AudioClip AudioClip;

    public void Play()
    {
      
        AudioManager.instance.Play(this);
        
    }
}
