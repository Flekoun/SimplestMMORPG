using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using simplestmmorpg.adminToolsData;
using simplestmmorpg.data;
using UnityEngine.Events;

public class UIBaseDescriptionMetadata : UISelectableEntry
{
    public ImageIdDefinitionSOSet AllImageIdDefinitionSOSet;

    public TextMeshProUGUI ItemIdText;
    public Image PortraitImage;

    private BaseDescriptionMetadata Data;

    public UnityAction<UIBaseDescriptionMetadata> OnClicked;

    public override string GetUid()
    {
        return Data.id;
    }


    // Start is called before the first frame update
    public void Setup(BaseDescriptionMetadata _item)
    {
        Data = _item;
        ItemIdText.SetText(_item.title.EN);
        PortraitImage.sprite = AllImageIdDefinitionSOSet.GetDefinitionById(_item.imageId).Image;
    }


    public void Clicked()
    {
        OnClicked.Invoke(this);
    }
}
