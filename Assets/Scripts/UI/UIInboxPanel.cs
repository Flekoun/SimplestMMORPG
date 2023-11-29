using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInboxPanel : MonoBehaviour
{
    public UIInboxItemsSpawner UIInboxItemsSpawner;
    public GameObject Model;


    public void Show(bool _asPlayerInbox)
    {
        Model.SetActive(true);
        UIInboxItemsSpawner.Setup(_asPlayerInbox);
    }


    public void Hide()
    {
        Model.SetActive(false);
    }

}
