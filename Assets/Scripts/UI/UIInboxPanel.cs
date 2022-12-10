using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInboxPanel : MonoBehaviour
{
    public UIInboxItemsSpawner UIInboxItemsSpawner;
    public GameObject Model;


    public void Show()
    {
        Model.SetActive(true);
        UIInboxItemsSpawner.Refresh();
    }


    public void Hide()
    {
        Model.SetActive(false);
    }
  
}
