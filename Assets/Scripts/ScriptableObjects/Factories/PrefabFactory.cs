using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PrefabFactory : ScriptableObject
{



    public GameObject CreateGameObject(GameObject objectToSpawn, Transform parent)
    {
        GameObject objectSpawned = ((GameObject)Instantiate(objectToSpawn, parent));

        return objectSpawned;
    }


    public GameObject CreateGameObject(GameObject objectToSpawn, Vector2 position)
    {
        GameObject objectSpawned = ((GameObject)Instantiate(objectToSpawn, position, Quaternion.identity));

        return objectSpawned;
    }


    public T CreateGameObject<T>(GameObject objectToSpawn, Transform parent)
    {
      
        GameObject objectSpawned = ((GameObject)Instantiate(objectToSpawn, parent));

     
        return objectSpawned.GetComponent<T>();
    }

    public T CreateGameObject<T>(GameObject objectToSpawn, Transform parent, Vector2 position)
    {
        GameObject objectSpawned = ((GameObject)Instantiate(objectToSpawn, parent));
        objectSpawned.transform.position = position;

        return objectSpawned.GetComponent<T>();
    }

    public T CreateGameObject<T>(GameObject objectToSpawn, Vector2 position)
    {
        GameObject objectSpawned = ((GameObject)Instantiate(objectToSpawn, position, Quaternion.identity));

        return objectSpawned.GetComponent<T>();
    }

    public T CreateGameObject<T>(GameObject objectToSpawn, Vector2 position, Quaternion rotation)
    {
        GameObject objectSpawned = ((GameObject)Instantiate(objectToSpawn, position, rotation));

        return objectSpawned.GetComponent<T>();
    }

    public T CreateGameObject<T>(GameObject objectToSpawn, Vector2 position, Quaternion rotation, Transform parent)
    {
        GameObject objectSpawned = ((GameObject)Instantiate(objectToSpawn, position, rotation));
        objectSpawned.transform.SetParent(parent, false);
        return objectSpawned.GetComponent<T>();
    }


}
