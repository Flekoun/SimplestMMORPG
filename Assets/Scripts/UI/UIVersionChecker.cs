using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIVersionChecker : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public GameObject Model;

    public void Awake()
    {
        AccountDataSO.OnClientVersionMatch += OnClientVersionMatch;
    }
    // Start is called before the first frame update
    void OnClientVersionMatch(bool _match)
    {
        Model.SetActive(!_match);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
