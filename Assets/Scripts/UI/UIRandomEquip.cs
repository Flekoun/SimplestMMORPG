using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;
using simplestmmorpg.data;

public class UIRandomEquip : MonoBehaviour
{
    public ImageIdDefinitionSOSet AllImageIdDefinitionSOSet;
    public EquipSlotDefinitionSOSet AllEquipSlotDefinitionSOSet;


    public Image EquipSlotImage;
    public Image RarityImage;

    public QuestgiverRewardRandomEquipsMeta Data = null;

    public UnityAction<UIRandomEquip> OnClicked;

    
    // Start is called before the first frame update
    public void SetData(QuestgiverRewardRandomEquipsMeta _data)
    {
        Data = _data;
       
        RarityImage.color = Utils.GetRarityColor(Data.rarity);
        Debug.Log("Data.equipSlotId: " + Data.equipSlotId);
        EquipSlotImage.sprite = AllEquipSlotDefinitionSOSet.GetDefinitionById(Data.equipSlotId).EquipSlotImage;

    }

    public void ButtonClicked()
    {
        if (OnClicked != null)
            OnClicked.Invoke(this);

    }

}
