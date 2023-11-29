using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingTextSpawner : MonoBehaviour
{
    public PrefabFactory PrefabFactory;
    public GameObject FloatingTextPrefab;


    public void Spawn(string _text, Color _color, Transform _spawnPoint)
    {
//        Debug.Log("SPawnuju floating text :" + _text + "jsem : " + this.gameObject.name);
        var floatingText = PrefabFactory.CreateGameObject<FloatingText>(FloatingTextPrefab, _spawnPoint);
        floatingText.Text.color = _color;
        floatingText.Show(_text);
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
