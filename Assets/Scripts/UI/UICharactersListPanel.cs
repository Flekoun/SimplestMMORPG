using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using static UnityEngine.EventSystems.EventTrigger;
using UnityEngine.UI;

public class UICharactersListPanel : MonoBehaviour
{
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public AccountDataSO AccountDataSO;
    public UICharacterPreviewSpawner UICharacterPreviewSpawner;
    public ListenOnAccountData ListenOnAccountData;
    public Button EnterWorldButton;
    public Button DeleteCharacterButton;
    public GameObject OMGTohlejeHurzaBlockerGO;
    public GameObject Model;

    public void Awake()
    {
        AccountDataSO.OnSkillsMetadataLoadedFirstTime += OnMetaDataLoaded;
        AccountDataSO.OnPlayerDataLoadedFirstTime += Show;
        AccountDataSO.OnPlayerDataChanged += Refresh;
        AccountDataSO.OnCharacterLoadedFirstTime += CharacterLoaded;
      

        UICharacterPreviewSpawner.OnEntryClicked += OnCharacterPreviewEntryClicked;

    }

    private void OnCharacterPreviewEntryClicked(UICharacterPreviewEntry _entry)
    {
        RefreshButtonsInteractibility();
    }

    private void OnMetaDataLoaded()
    {

        ListenOnAccountData.StartListeningOnPlayer();
    }

    public void Show()
    {
        //   ListenOnAccountData.StopListeningOnPlayer();

        OMGTohlejeHurzaBlockerGO.SetActive(false);
        Model.gameObject.SetActive(true);
        Refresh();

        //  Refresh();
    }

    private void Refresh()
    {
        Debug.Log("PLAYER DATA CHANGED!");
        UICharacterPreviewSpawner.Spawn();
        RefreshButtonsInteractibility();
    }

    private void RefreshButtonsInteractibility()
    {
        EnterWorldButton.interactable = UICharacterPreviewSpawner.IsAnyItemSelected();
        DeleteCharacterButton.interactable = UICharacterPreviewSpawner.IsAnyItemSelected();
    }
    //private void Refresh()
    //{
    //    UICharacterPreviewSpawner.Spawn();
    //}

  

    // Update is called once per frame
    public void Hide()
    {
        Model.gameObject.SetActive(false);
    }

    private void CharacterLoaded()
    {
        OnCharacterLoaded.Invoke();
    }


    public void EnterWorldClicked()
    {
        ListenOnAccountData.StartListeningOnCharacter(UICharacterPreviewSpawner.GetSelectedEntry().GetUid());
    }

    public void DeleteCharacterClicked()
    {
        FirebaseCloudFunctionSO.DeleteCharacter(UICharacterPreviewSpawner.GetSelectedEntry().GetUid());
    }

    public UnityEvent OnCharacterLoaded;

}
