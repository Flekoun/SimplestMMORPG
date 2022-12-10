using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SoundObject : ScriptableObject {


	public List<AudioClip> AudioClips;


	public void PlaySound(AudioSource source)
	{
		//Debug.Log("Hraju zvuk");
		AudioClip soundToPlay = AudioClips[Random.Range(0,AudioClips.Count)];
		source.clip=soundToPlay;
		source.Play();
	}

  
}
