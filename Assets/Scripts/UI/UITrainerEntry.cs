using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using simplestmmorpg.data;
using UnityEngine.Events;

public class UITrainerEntry : MonoBehaviour
{
    public TextMeshProUGUI NameText;
  
    public Trainer Data;
    public UnityAction<UITrainerEntry> OnClicked;


    public void SetData(Trainer _data)
    {
        Data = _data;
        NameText.SetText(Utils.GetMetadataForTrainers(Data.id).title.GetText());
    }

    public void Clicked()
    {
        OnClicked.Invoke(this);
    }
}
