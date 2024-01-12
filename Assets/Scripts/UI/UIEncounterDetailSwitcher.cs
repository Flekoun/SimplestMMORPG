using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;

public class UIEncounterDetailSwitcher : MonoBehaviour
{

    public AccountDataSO AccountDataSO;
    public UIEncounterDetailPanel_CombatView UIEncounterDetailPanel_CombatView;
    public UIEncounterDetailPanel UIEncounterDetailPanel_MonsterEncounterView;
    public UIEncounterDetailPanel_DungeonView UIEncounterDetailPanel_DungeonView;
    public UIChatMessageSpawner UIChatMessageSpawner;
    private EncounterData Data;
    private IEncounterDetailPanel ActiveEncounterPanel = null;

    public void Show(EncounterData _data)
    {
        //        Debug.Log("SHOW!");
        AccountDataSO.OnCharacterDataChanged += Refresh;
        AccountDataSO.OnEncounterDataChanged += Refresh;

        Data = _data;


        ActivateCorrectPanel();

        //fActiveEncounterPanel.Show(Data);
    }

    private void ActivateCorrectPanel()
    {

        bool IAmComabatantInThisEncounter = Data.IsParticipatingInCombat(AccountDataSO.CharacterData.uid);
        bool IAmFounderOfThisEncounter = Data.foundByCharacterUid == AccountDataSO.CharacterData.uid;
        bool PerkChoiceFinished = true;//(Data.PendingPerksChoicesAmount() == 0);

        IEncounterDetailPanel newActivePanel = null;

        //pokud si joinuty ukazu combat view?
        if (IAmComabatantInThisEncounter && PerkChoiceFinished)
        {
            if (newActivePanel != UIEncounterDetailPanel_CombatView)
                UIChatMessageSpawner.ShowCombatLog();

            //  Debug.Log("UKAZUJU COMBAT VIEW");
            newActivePanel = UIEncounterDetailPanel_CombatView;


        }
        else //jinak podle toho co je to za encounter 
        {
            //            Debug.Log("UKAZUJU PERK VIEW");
            if (Data.encounterContext == Utils.ENCOUNTER_CONTEXT.PERSONAL)
                newActivePanel = UIEncounterDetailPanel_MonsterEncounterView;
            else if (Data.encounterContext == Utils.ENCOUNTER_CONTEXT.DUNGEON)
                newActivePanel = UIEncounterDetailPanel_DungeonView;

            //}
        }

        if (newActivePanel != ActiveEncounterPanel)
        {

            ActiveEncounterPanel?.Hide();
            ActiveEncounterPanel = newActivePanel;
        }

        ActiveEncounterPanel.Show(Data);

    }


    public void Hide()
    {


        //Debug.Log("HIDE!");
        AccountDataSO.OnCharacterDataChanged -= Refresh;
        AccountDataSO.OnEncounterDataChanged -= Refresh;

        if (ActiveEncounterPanel != null)
            ActiveEncounterPanel.Hide();

        ActiveEncounterPanel = null;
    }

    private void Refresh()
    {


        ActivateCorrectPanel();

        if (ActiveEncounterPanel != null)
        {
            //            Debug.Log("Volam refresh!");
            ActiveEncounterPanel.Refresh();
        }
        //    else
        //        Debug.Log("neVolam refresh!");
    }



}


public interface IEncounterDetailPanel
{
    public void Show(EncounterData _data);
    public void Hide();
    public void Refresh();
}
