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
    public GameObject Model;

    private string SelectedClass = "";

    public void SetCharacterClass(string _class)
    {
        SelectedClass = _class;
        SelectedClassText.SetText(SelectedClass);
    }

    public void Show()
    {
        Model.gameObject.SetActive(true);
    }


    // Update is called once per frame
    public void Hide()
    {
        Model.gameObject.SetActive(false);
    }


    public void CreateCharacterClicked()
    {
        if (SelectedClass != "")
            FirebaseCloudFunctionSO.CreateCharacter(CharacterNameInput.text, SelectedClass);
    }

}
