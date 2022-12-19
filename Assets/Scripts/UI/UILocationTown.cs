using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILocationTown : MonoBehaviour
{
//    public ListenOnVendors ListenOnVendors;
    public LocationIdDefinition LocationIdDefinition;
    public ZoneIdDefinition ZoneIdDefinition;
    public GameObject Model;
    // Start is called before the first frame update
    public void Show()
    {
      //  ListenOnVendors.StartListening(LocationIdDefinition.Id, ZoneIdDefinition.Id);
        Model.gameObject.SetActive(true);
    }

    // Update is called once per frame
    public void Hide()
    {
     //   ListenOnVendors.StopListening();
        Model.gameObject.SetActive(false);
    }
}
