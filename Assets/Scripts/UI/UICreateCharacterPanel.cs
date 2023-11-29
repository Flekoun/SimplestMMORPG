using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UICreateCharacterPanel : MonoBehaviour
{

    public AccountDataSO AccountDataSO;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public TMP_InputField CharacterNameInput;
    public TextMeshProUGUI SelectedClassText;
    public UIPortrait UIPortrait;
    public UIPortraitChooser UIPortraitChooser;
    public GameObject Model;
    public GameObject CharacterDetailGO;

    private string SelectedClass = "";
    private string SelectedPortrait = "";

    public void SetCharacterClass(string _class)
    {
        SelectedClass = _class;
        SelectedClassText.SetText(SelectedClass);
        CharacterDetailGO.SetActive(true);
        OnPortraitChoosen("CHARACTER_PORTRAIT_DEFAULT");

    }

    //public void SetCharacterPortrait(string _portraitId)
    //{
    //    SelectedPortrait = _portraitId;
    //    UIPortrait.SetPortrait(_portraitId, SelectedClass);

    //}

    public void Show()
    {
        Model.gameObject.SetActive(true);
    }


    // Update is called once per frame
    public void Hide()
    {
        Model.gameObject.SetActive(false);
    }

    public void ChangePortraitClicked()
    {
        UIPortraitChooser.Show(OnPortraitChoosen, SelectedClass, SelectedPortrait);
    }

    private void OnPortraitChoosen(string _portraitId)
    {
        SelectedPortrait = _portraitId;
        UIPortrait.SetPortrait(_portraitId, SelectedClass);
    }

    public void CreateCharacterClicked()
    {
        if (SelectedClass != "")
            FirebaseCloudFunctionSO.CreateCharacter(CharacterNameInput.text, SelectedClass, SelectedPortrait);
    }

}
