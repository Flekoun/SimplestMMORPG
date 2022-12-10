using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class UIWhatIsNew : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public TextMeshProUGUI BodyText;
    public GameObject Model;
    // Start is called before the first frame update
    public void Show()
    {
        var window = UIManager.instance.SpawnPromptPanel(AccountDataSO.OtherMetadataData.whatIsNew,"What is new", null, null);
        window.AcceptButtonText.SetText("Close");
        window.HideDeclineButton();

        // Model.gameObject.SetActive(true);
        //  BodyText.SetText(AccountDataSO.OtherMetadataData.whatIsNew);
    }

    public void ShowVotingTest()
    {
        var window = UIManager.instance.SpawnPromptPanel("Players will be able to vote on new features they want to see implemented or changes they want to see in game.\n\n Each player will have different \"Vote weight\" based on the amount of time they put in the game or other support they provided for the development of the game.", "Coming soon!", null, null);
        window.AcceptButtonText.SetText("Close");
        window.HideDeclineButton();

        // Model.gameObject.SetActive(true);
        //  BodyText.SetText(AccountDataSO.OtherMetadataData.whatIsNew);
    }

    public void ShowDiscord()
    {
     Application.OpenURL("https://discord.gg/2AUcDgRY88");
    }

    // Update is called once per frame
    public void Hide()
    {
        //  Model.gameObject.SetActive(false);
    }
}
