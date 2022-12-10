using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterTime : MonoBehaviour {

	public float DestroyDelay=5f;

	// Use this for initialization
	void Start () {

		StartCoroutine("DestroySelf");
	}

	private IEnumerator DestroySelf()
	{
		yield return new WaitForSecondsRealtime(DestroyDelay);
	//	Debug.Log("Ja se nicim!" + this.name);
		Destroy(this.gameObject);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnDisable()
    {
        Destroy(this.gameObject);
    }
}
