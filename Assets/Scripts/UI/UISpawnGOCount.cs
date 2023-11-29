using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISpawnGOCount : MonoBehaviour
{
    public PrefabFactory PrefabFactory;
    public List<GameObject> PrefabToSpawn;
    public Transform Parent;
    // Start is called before the first frame update
    public void Spawn(int _amount, int index = 0, bool clearOld = true)
    {
        if (clearOld)
            Utils.DestroyAllChildren(Parent);

        for (int i = 0; i < _amount; i++)
            PrefabFactory.CreateGameObject(PrefabToSpawn[index], Parent);
    }



}
