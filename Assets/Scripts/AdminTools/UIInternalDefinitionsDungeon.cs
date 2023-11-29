using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using simplestmmorpg.adminToolsData;
using UnityEngine.Events;

public class UIInternalDefinitionsDungeon : MonoBehaviour
{

    public PointOfInterestInternalDefinition Data;
    public TMP_InputField FloorMinInput;
    public TMP_InputField FloorMaxInput;
    public TMP_InputField IdInput;
    public TMP_InputField PartySizeInput;


    public UnityAction<UIInternalDefinitionsDungeon> OnClicked;

    // Start is called before the first frame update
    public void Setup(PointOfInterestInternalDefinition _data)
    {
        Data = _data;

        FloorMinInput.text = Data.floorMin.ToString();
        FloorMaxInput.text = Data.floorMax.ToString();
        PartySizeInput.text = Data.dungeon.partySize.ToString();
        IdInput.text = Data.id;


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

    public void OnPartySizeValueChanged(string _value)
    {
        Data.dungeon.partySize = int.Parse(_value);
    }
    public void OnIdValueChanged(string _value)
    {
        Data.id = _value;
    }

}
