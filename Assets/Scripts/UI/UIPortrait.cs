using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class UIPortrait : UISelectableEntry
{
    public AccountDataSO AccountDataSO;
    public ImageIdDefinitionSOSet AllImageIdDefinitionSOSet;
    public Image PortraitImage;
    public Image CharacterClassImage;
    public Button Button;
    public Image LockedImage;
    public TextMeshProUGUI NameText;
    public UnityAction<UIPortrait> OnClicked;

    public string portraitId;
    public string characterClassId;

    public bool ShowAsPlayerPortrait = false;

    public void Awake()
    {
        if (ShowAsPlayerPortrait)
            AccountDataSO.OnCharacterDataChanged += OnCharacterDataChanged;
    }

    private void OnCharacterDataChanged()
    {
        if (ShowAsPlayerPortrait)
            SetPortrait(AccountDataSO.CharacterData.characterPortrait, AccountDataSO.CharacterData.characterClass);
    }


    // Start is called before the first frame update
    public void SetPortrait(string _imageId, string _characterClassId)
    {
        portraitId = _imageId;
        PortraitImage.sprite = AllImageIdDefinitionSOSet.GetDefinitionById(portraitId).Image;
        characterClassId = _characterClassId;
        CharacterClassImage.color = Utils.GetClassColor(characterClassId);
    }


    //// Start is called before the first frame update
    //public void SetCharacterClass(string _characterClassId)
    //{
    //    characterClassId = _characterClassId;
    //    CharacterClassImage.color = Utils.GetClassColor(characterClassId);
    //}

    // Start is called before the first frame update
    public void SetName(string _name)
    {
        if (NameText != null)
        {
            NameText.gameObject.SetActive(true);
            NameText.SetText(_name);

        }

    }

    // Update is called once per frame
    public void EnableAsButton()
    {
        Button.interactable = true;
        Button.targetGraphic.raycastTarget = true;
    }

    public void Clicked()
    {

        OnClicked?.Invoke(this);
    }

    public void SetLookAsUnavailable()
    {
        LockedImage.gameObject.SetActive(true);
    }

    public override string GetUid()
    {
        return portraitId;
    }
}
