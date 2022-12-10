using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPortrait : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public ImageIdDefinitionSOSet AllImageIdDefinitionSOSet;
    public Image PortraitImage;
    public bool ShowAsPlayerPortrait = false;

    public void Awake()
    {
        AccountDataSO.OnCharacterDataChanged += OnCharacterDataChanged;
    }

    private void OnCharacterDataChanged()
    {
        if (ShowAsPlayerPortrait)
            SetPortrait(AccountDataSO.CharacterData.characterPortrait);
    }


    // Start is called before the first frame update
    public void SetPortrait(string _imageId)
    {
        PortraitImage.sprite = AllImageIdDefinitionSOSet.GetDefinitionById(_imageId).Image;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
