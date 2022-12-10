using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.Events;

public class ClearFirestoreCache : MonoBehaviour
{

    // Start is called before the first frame update
    public void Clear()
    {

        var db = FirebaseFirestore.DefaultInstance;
       // db.TerminateAsync();
        db.ClearPersistenceAsync();
        OcCacheCleared.Invoke();


    }

    public UnityEvent OcCacheCleared;
}
