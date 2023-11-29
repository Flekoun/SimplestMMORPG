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
        if (Utils.DescriptionsMetadata.DoesDescriptionMetadataForIdExist(Data.id))
            VendorNameText.SetText(Utils.DescriptionsMetadata.GetVendorsMetadata(Data.id).title.GetText());
        else
            VendorNameText.SetText(Data.id);
    }

    public void Clicked()
    {
        OnClicked.Invoke(this);
    }
}
