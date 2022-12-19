using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using simplestmmorpg.data;
using UnityEngine.Events;

public class UIVendorEntry : MonoBehaviour
{
    public TextMeshProUGUI VendorNameText;
  
    public Vendor Data;
    public UnityAction<UIVendorEntry> OnClicked;


    public void SetData(Vendor _vendorData)
    {
        Data = _vendorData;
        VendorNameText.SetText(Utils.GetMetadataForVendors(Data.id).title.GetText());
    }

    public void Clicked()
    {
        OnClicked.Invoke(this);
    }
}
