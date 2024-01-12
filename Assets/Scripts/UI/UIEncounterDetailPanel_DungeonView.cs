using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
using System.ComponentModel.Composition.Primitives;
using UnityEngine.Events;
using System;


public class UIEncounterDetailPanel_DungeonView : MonoBehaviour, IEncounterDetailPanel
{
    public AccountDataSO AccountDataSO;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public PrefabFactory PrefabFactory;

    public GameObject Model;
    public EncounterData Data;

    public UILocation UILocationEncounters;
    public UIEncounterEntry UIEncounterEntry;
    public UIEncountersSpawner UIEncountersSpawner;

    public Button JoinEncounterButton;
    public GameObject JoinEncounterButtonGO;
    public TextMeshProUGUI JoinButtonText;

    public GameObject RetreatGO;
    //public UISpawnGOCount UISpawnGOCount_DungeonFloors;
    public UIPriceTimeLabel JoinCombat_UIPriceTimeLabel;


    public GameObject UITopPanelGlobal;
    public GameObject UITopPanel;



    public UnityAction<UIEncounterDetailPanel_DungeonView> OnRefreshed;
    public ContentFitterRefresh ContentFitterRefresh;


    //mam to tu proto aby po tom co se joines do encounteru byl nasledny refresh init.
    //protoze ja uz netusim jestli je to pak init nebo ne. Mam nasrany Join obrazovku i combat defakto v jedne obrazovce/skriptu, tomto, tak nepoznam rozdil
    private bool FlaggedForInitRefresh = false;

    private string oldChatText;



    public void Awake()
    {
        UIEncountersSpawner.OnUIEnecounterEntryClicked += Show;

    }

    public void OnDestroy()
    {
        UIEncountersSpawner.OnUIEnecounterEntryClicked -= Show;
    }


    public void Show(EncounterData _data)
    {
        UILocationEncounters.Hide();

        Data = _data;

        Model.SetActive(true);

        Refresh(true);

    }

    public void Hide()
    {
        CancelInvoke();

        Model.SetActive(false);
        UITopPanelGlobal.SetActive(true);

    }

    public void Refresh()
    {
        Refresh(false);

    }


    private void Refresh(bool _initRefresh)
    {

        if (FlaggedForInitRefresh)
        {
            _initRefresh = true;
            FlaggedForInitRefresh = false;
        }

        if (!AccountDataSO.EncountersData.Contains(Data)) //pokud neni muj encounter v EncounterListu, musim byt smaazany z databaze, asi encounter skoncil
        {
            this.Data = null;
            Hide();
            UILocationEncounters.Show();
            OnRefreshed?.Invoke(this);
            return;
        }

        bool IAmComabatantInThisEncounter = Data.IsParticipatingInCombat(AccountDataSO.CharacterData.uid);
        bool IAmFounderOfThisEncounter = Data.foundByCharacterUid == AccountDataSO.CharacterData.uid;
        bool PerkChoiceFinished = true;// (Data.PendingPerksChoicesAmount() == 0);


        UIEncounterEntry.SetEncounter(Data, _initRefresh);

        JoinCombat_UIPriceTimeLabel.gameObject.SetActive(false);

        RetreatGO.SetActive(IAmComabatantInThisEncounter && !PerkChoiceFinished);

        UITopPanel.SetActive(!(IAmComabatantInThisEncounter && PerkChoiceFinished));
        JoinEncounterButtonGO.SetActive(!IAmComabatantInThisEncounter && Data.enemies.Count > 0);

        UIEncounterEntry.ShowBasicInfoPanel(!(IAmComabatantInThisEncounter && PerkChoiceFinished));
        JoinButtonText.SetText("<b>Join!</b>");

        //if (AccountDataSO.CharacterData.currency.fatigue > 0)
        //    JoinButtonText.SetText(JoinButtonText.text + "<color=\"white\">You start with <color=\"red\">" + (AccountDataSO.CharacterData.GetTotalHealth(true) - AccountDataSO.CharacterData.GetTotalHealth(false)).ToString() + " HP </color>less due to Fatigue</color>");

        bool hasEnoughtTime = AccountDataSO.CharacterData.currency.time >= Data.joinPrice;
        if (IAmFounderOfThisEncounter)
            hasEnoughtTime = true;

        JoinEncounterButton.interactable = hasEnoughtTime;//AccountDataSO.CharacterData.currency.fatigue <= 50 && hasEnoughtTime;


        //if (AccountDataSO.CharacterData.currency.fatigue > 50)
        //    JoinButtonText.SetText(JoinButtonText.text + "<color=\"white\">Your are too fatigued! Rest until your fatigue is bellow 50% </color>");




        OnRefreshed?.Invoke(this);
        ContentFitterRefresh.RefreshContentFitters();
    }

    public void JoinEncounterClicked()
    {
        //  if (AccountDataSO.CharacterData.currency.food > 0)
        // {
        FirebaseCloudFunctionSO.JoinEncounter(Data.uid);
        FlaggedForInitRefresh = true;
        // }
        //  else
        //    Debug.Log("Not Enough Food!");
    }

    public void RetreatFromEncounter()
    {
        FirebaseCloudFunctionSO.RetreatFromEncounter(Data.uid);

    }

}
