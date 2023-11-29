using System;
using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.adminToolsData;
using simplestmmorpg.data;
using UnityEngine;
using UnityEngine.Events;

public class UIMapMakerPanel : MonoBehaviour
{
    public FirebaseCloudFunctionSO_Admin FirebaseCloudFunctionSO_Admin;
    public AccountDataSO AccountDataSO;

    public PrefabFactory PrefabFactory;
    public GameObject Model;
    public Transform Parent;



    public void Show()
    {

        Model.gameObject.SetActive(true);
     

    }


    private void Refresh()
    {

    }



    public void Hide()
    {
        Model.gameObject.SetActive(false);

    }


}
