using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
//    public Color Color;
    public TextMeshProUGUI Text;

    public void Awake()
    {
        
    }

    public void Show(string _text)
    {
        this.name = this.name + (Random.Range(0, 100000).ToString());
//        Debug.Log("Já žiju!" + this.name);
        Text.SetText(_text);
      //  StartCoroutine(Wait());
    }

    public void OnDestroy()
    {
//        Debug.Log("aaa umiram : " + this.name);
    }

    //public IEnumerator Wait()
    //{
    //    yield return new WaitForSecondsRealtime(3);
    //    Destroy(this.gameObject);
    //}
}
