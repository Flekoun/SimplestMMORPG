using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.EventSystems.EventTrigger;

public class UIPortraitChooser : UISelectableSpawner
{
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public AccountDataSO AccountDataSO;
    public PrefabFactory PrefabFactory;
    public GameObject PortratiPrefab;
    public Transform Parent;
    public GameObject Model;
    private UIPortrait lastlyClickedEntry;
    private UnityAction<string> OnPortraitChoosen;

    private List<UIPortrait> SpawnedPortraits = new List<UIPortrait>();

    public void Show(UnityAction<string> _onPortraitChoosen, string _characterClass, string _defaultPortraitChoosen = "")
    {

        OnPortraitChoosen = _onPortraitChoosen;

        SpawnedPortraits.Clear();
        Utils.DestroyAllChildren(Parent);
        foreach (var item in AccountDataSO.OtherMetadataData.GetPossiblePortraits(_characterClass))
        {
            var portrait = PrefabFactory.CreateGameObject<UIPortrait>(PortratiPrefab, Parent);
            SpawnedPortraits.Add(portrait);
            portrait.EnableAsButton();
            portrait.OnClicked += OnPortraitClicked;
            portrait.SetPortrait(item, _characterClass); ;
            if (!AccountDataSO.PlayerData.portraitsUnlocked.Contains(item))
                portrait.SetLookAsUnavailable();

            if (portrait.GetUid() == _defaultPortraitChoosen)
            {
                lastlyClickedEntry = portrait;
                ChoosePortrait();
            }

        }
        if (_defaultPortraitChoosen == "")
        {
            lastlyClickedEntry = SpawnedPortraits[0];
            ChoosePortrait();
        }

        Model.gameObject.SetActive(true);
    }

    private void OnPortraitClicked(UIPortrait _entry)
    {
        UIPromptWindow prompt;
        prompt = UIManager.instance.SpawnPromptPanel(Utils.DescriptionsMetadata.GetPortraitsMetadata(_entry.portraitId).description.GetText(), Utils.DescriptionsMetadata.GetPortraitsMetadata(_entry.portraitId).title.GetText(), ChoosePortrait, null);



        lastlyClickedEntry = _entry;

        if (!AccountDataSO.PlayerData.portraitsUnlocked.Contains(_entry.GetUid()))
            prompt.HideAcceptButton();

        if (base.GetSelectedEntry() == _entry)
            prompt.HideAcceptButton();

        prompt.SetAcceptButtonText("Choose");
        prompt.SetDeclineButtonText("Close");
    }

    private void ChoosePortrait()
    {
        base.OnUISelectableItemClicked(lastlyClickedEntry);
    }

    public void Hide()
    {
        //if (base.GetSelectedEntry().GetUid() != AccountDataSO.CharacterData.characterPortrait)
        //    FirebaseCloudFunctionSO.ChangeCharacterPortrait(base.GetSelectedEntry().GetUid());

        OnPortraitChoosen.Invoke(base.GetSelectedEntry().GetUid());

        Model.gameObject.SetActive(false);
    }

}
