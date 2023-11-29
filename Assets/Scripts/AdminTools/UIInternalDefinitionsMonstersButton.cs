using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using simplestmmorpg.adminToolsData;
using UnityEngine.Events;

public class UIInternalDefinitionsMonstersButton : MonoBehaviour
{

    public PointOfInterestInternalDefinition Data;
    public TMP_InputField FloorMinInput;
    public TMP_InputField FloorMaxInput;
    public TMP_InputField RarePerksIdInput;
    public TMP_InputField IdInput;
    public TMP_InputField PartySizeInput;


    public UnityAction<UIInternalDefinitionsMonstersButton> OnClicked;

    // Start is called before the first frame update
    public void Setup(PointOfInterestInternalDefinition _data)
    {
        Data = _data;

        FloorMinInput.text = Data.floorMin.ToString();
        FloorMaxInput.text = Data.floorMax.ToString();
        RarePerksIdInput.text = Data.monsters.perkOffersRareId;
        IdInput.text = Data.id;
        PartySizeInput.text = Data.monsters.partySize.ToString();


    }

    // Update is called once per frame
    public void Clicked()
    {
        OnClicked?.Invoke(this);
    }

    public void OnFloorMinInputValueChanged(string _value)
    {
        Data.floorMin = int.Parse(_value);
    }

    public void OnFloorMaxInputValueChanged(string _value)
    {
        Data.floorMax = int.Parse(_value);
    }

    public void OnRarePerksIdValueChanged(string _value)
    {
        Data.monsters.perkOffersRareId = _value;
    }
    public void OnIdValueChanged(string _value)
    {
        Data.id = _value;
    }

    public void OnPartySizeValueChanged(string _value)
    {
        Data.monsters.partySize = int.Parse(_value);
    }

}
