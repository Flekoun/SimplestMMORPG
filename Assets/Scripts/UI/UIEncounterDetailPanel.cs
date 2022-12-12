using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIEncounterDetailPanel : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    //public MenuDataSO MenuDataSO;
    public GameObject Model;
    public EncounterData Data;
    public UICombatMemberSkillsSpawner UISkillsSpawner;
    public UILocationEncounters UILocationEncounters;
    public UIEncounterEntry UIEncounterEntry;
    public UIEncountersSpawner UIEncountersSpawner;
    public TextMeshProUGUI DeckCardsCountText;
    public GameObject JoinEncounterButtonGO;
    public GameObject MyCombatToolsGO;

   // public TextMeshProUGUI ChatText;
    public TextMeshProUGUI CombatText;

   /// <summary>
   /// public Image ChatButtonImage;
   /// </summary>
   // public Image CombatButtonImage;

    public GameObject LogsPanel;
    public GameObject CombatLogGO;
  //  public GameObject ChatLogGO;

    //public ScrollRect ChatScrollRect;

    public TextMeshProUGUI TurnEndTimeText;
    public TextMeshProUGUI RestButtonText;
    public TextMeshProUGUI JoinButtonText;


    public TextMeshProUGUI SituationDescriptionText;

    public RectTransform TransformToFix;

    public Button RestButton;

    public UISkill SelectedSkillDetail;

    private UICombatEntity SelectedCombatEntity;
    private UICombatMemberSkillEntry SelectedSkill;

    private CombatMember MyCombatMemberData;

    private bool CombatLogShown = true;
    private bool ChatLogShown = true;


    private string oldChatText;

    // private Coroutine turnTimeLeftCourotine;

    private void TryToFixScrollReckGlitches()
    {
        StartCoroutine(Wait());

    }

    private IEnumerator Wait()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        LayoutRebuilder.ForceRebuildLayoutImmediate(TransformToFix);
        Canvas.ForceUpdateCanvases();
    }

    public void OnEnable()
    {
        AccountDataSO.OnCharacterDataChanged += Refresh;
    }

    public void OnDisable()
    {
        AccountDataSO.OnCharacterDataChanged -= Refresh;
    }

    public void Awake()
    {
        UIEncounterEntry.OnCombatEntityClicked += OnCombatEntityClicked;
        UIEncountersSpawner.OnUIEnecounterEntryClicked += Show;
        UISkillsSpawner.OnUICombatMemberSkillEntryClicked += SkillClicked;
      
    }

    public void OnDestroy()
    {
        UIEncountersSpawner.OnUIEnecounterEntryClicked -= Show;
        UISkillsSpawner.OnUICombatMemberSkillEntryClicked -= SkillClicked;

    }

    private void OnCombatEntityClicked(UICombatEntity _entry)
    {
        SelectedCombatEntity = _entry;

    }



    public void Show(EncounterData _data)
    {

        UILocationEncounters.Hide();

        string oldChatText = "";
        Data = _data;

        AccountDataSO.OnEncounterDataChanged += Refresh;

        Model.SetActive(true);
        Refresh();
        TryToFixScrollReckGlitches();
    }

    public void Close()
    {
        CancelInvoke();

        AccountDataSO.OnEncounterDataChanged -= Refresh;
        Model.SetActive(false);

    }



    private void Refresh()
    {

        Debug.Log("Refreshing Encounter Detail Panel");

        UIEncounterEntry.SetEncounter(Data);//, null);

        if (!AccountDataSO.EncountersData.Contains(Data)) //pokud neni muj encounter v EncounterListu, musim byt smaazany z databaze, asi encounter skoncil
        {
            Close();
            UILocationEncounters.Show();
        }

        bool IsParticipating = Data.IsParticipatingInCombat(AccountDataSO.CharacterData.uid);
        if (IsParticipating)
        {

            MyCombatMemberData = Data.GetMyCombatMemberData(AccountDataSO.CharacterData.uid);
            DeckCardsCountText.SetText(MyCombatMemberData.skillsDrawDeck.Length + "/" + MyCombatMemberData.skillsDiscardDeck.Length);
            CombatText.SetText(Utils.ReplacePlaceholdersInTextWithDescriptionFromMetadata(Data.combatLog));
     //       ChatText.SetText(Utils.ReplacePlaceholdersInTextWithDescriptionFromMetadata(Data.chatLog));

            UISkillsSpawner.Show(MyCombatMemberData, this);

            //if (string.Compare(Data.chatLog, oldChatText) != 0) //pokud je novy zaznam v chatu scrolnem dolu
            //{
            //    StartCoroutine(PushToBottom());
            //}

            RestButton.interactable = !MyCombatMemberData.hasRested;
            if (!MyCombatMemberData.hasRested)
                RestButtonText.SetText("Rest");//(<color=\"yellow\">1% Fatigue</color>)")
            else
                RestButtonText.SetText("Waiting for others to rest...");

            UISkillsSpawner.gameObject.SetActive(Data.GetMyCombatMemberData(AccountDataSO.CharacterData.uid).stats.health > 0);
            RestButton.gameObject.SetActive(Data.GetMyCombatMemberData(AccountDataSO.CharacterData.uid).stats.health > 0);

            CancelInvoke();
            InvokeRepeating("RefreshTurnTimeLeft", 0f, 1f);
            //   if (Data.AlreadyRestedThisTurn(MyCombatMemberData))
            //    RestButtonText.SetText("Onslaught (<color=\"red\">5% Fatigue</color>)");
        }

        //  string descText = "";

        //if (Data.enemies.Length == 1)
        //    descText += "You see a single monster not far from you.";
        //else if (Data.enemies.Length == 2)
        //    descText += "You see a couple of monsters ahead";
        //else 
        //    descText += "You see a group of monsters neraby";


        //if (Data.enemies[0].level <= AccountDataSO.CharacterData.stats.level)
        //    descText += " It looks like if you stay focused you should be able to kill all by yourself.";
        //else 
        //    descText += " It looks like it could be a tought fight.";





        if (Data.enemies.Length == 1)
        {
            if (Data.enemies[0].level <= AccountDataSO.CharacterData.stats.level)
                SituationDescriptionText.SetText("You see a single monster not far from you. It looks like if you stay focused you should be able to kill all by yourself.");
            else
                SituationDescriptionText.SetText("You see a single monster not far from you. It looks like it could be a tought fight.?");
        }
        else
        {
            if (Data.enemies.Length == 2)
                SituationDescriptionText.SetText("You see a couple of monsters ahead. It wont be a fair fight.");
            else
                SituationDescriptionText.SetText("You see a group of monsters neraby. It would be very hard to kill then all by yourself.?");
        }

        if (Data.combatants.Length == 1)
        {
            SituationDescriptionText.SetText(SituationDescriptionText.text + " However it seems there is some other brave hero fighting them already.");
        }
        else if (Data.combatants.Length > 1)
            SituationDescriptionText.SetText(SituationDescriptionText.text + " However it seems there a group of heroes fighting them already.");

        SituationDescriptionText.SetText(SituationDescriptionText.text + " \n Do you want to engage?");


        LogsPanel.SetActive(IsParticipating);
        //ChatText.gameObject.SetActive(IsParticipating);
        JoinEncounterButtonGO.SetActive(!IsParticipating);
        SituationDescriptionText.gameObject.SetActive(!IsParticipating);
        MyCombatToolsGO.SetActive(IsParticipating);
        DeckCardsCountText.gameObject.SetActive(IsParticipating);



        JoinButtonText.SetText("<b>Join Combat!</b>\n");

        if (AccountDataSO.CharacterData.currency.fatigue > 0)
            JoinButtonText.SetText(JoinButtonText.text + "<color=\"white\">You will muster only " + (100 - AccountDataSO.CharacterData.currency.fatigue).ToString() + "% of your stats due to Fatigue</color>");

    }


    private void RefreshTurnTimeLeft()
    {
        TurnEndTimeText.SetText(Data.GetTurnTimeTimeLeft());
    }

    //IEnumerator PushToBottom()
    //{
    //    yield return new WaitForEndOfFrame();
    //    ChatScrollRect.verticalNormalizedPosition = 0;
    //    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)ChatScrollRect.transform);
    //}

    public void CancelSelectedSkill()
    {
        if (SelectedSkillDetail.Data != null)
        {
            SelectedSkillDetail.transform.parent.gameObject.SetActive(false);
            SelectedSkillDetail.Data = null;
        }
    }

    public void SkillClicked(UICombatMemberSkillEntry _skill)
    {
        if (SelectedSkillDetail.Data == _skill.Data)
        {
            CastSelectedSkill();

            //SelectedSkillDetail.transform.parent.gameObject.SetActive(false);
            //SelectedSkillDetail.Data = null;
            //return;
        }
        else
        {
            SelectedSkillDetail.transform.parent.gameObject.SetActive(true);
            if (SelectedSkill != null)
                SelectedSkill.ShowAsSelected(false);

            SelectedSkill = _skill;

            SelectedSkill.ShowAsSelected(true);

            SelectedSkillDetail.SetData(_skill.Data);
        }
        //FirebaseCloudFunctionSO.ApplySkillOnEncounter(Data.uid, _skill.Data.handSlotIndex, SelectedCombatEntity.Data.uid);
    }



    public void CastSelectedSkill()
    {
        //  SelectedSkillDetail.SetData(_skill.Data);
        //SelectedSkillDetail.gameObject.SetActive(true);
        if (SelectedCombatEntity == null)
        {
            UIManager.instance.ImportantMessage.ShowMesssage("Choose your target!");
            return;
        }

        FirebaseCloudFunctionSO.ApplySkillOnEncounter(Data.uid, SelectedSkill.Data.handSlotIndex, SelectedCombatEntity.Data.uid);

        CancelSelectedSkill();
    }

    public void EndTurnClicked()
    {
        FirebaseCloudFunctionSO.RestEncounter(Data.uid);
    }

    public void JoinEncounterClicked()
    {
        if (AccountDataSO.CharacterData.currency.food > 0)
            FirebaseCloudFunctionSO.JoinEncounter(Data.uid);
        else
            Debug.Log("Not Enough Food!");
    }

    public void FleeFromEncounter()
    {
        UIManager.instance.SpawnPromptPanel("Do you want to flee from combat? You will suffer <color=\"yellow\">5%</color> Fatigue penalty", () =>
        {
            FirebaseCloudFunctionSO.FleeFromEncounter(Data.uid);
        }, null);

    }





    //public void ToggleCombatLog()
    //{
    //    if (!CombatLogShown)
    //        CombatButtonImage.color = new Color(CombatButtonImage.color.r, CombatButtonImage.color.g, CombatButtonImage.color.b, 1);
    //    else
    //        CombatButtonImage.color = new Color(CombatButtonImage.color.r, CombatButtonImage.color.g, CombatButtonImage.color.b, 0.5f);

    //    CombatLogGO.SetActive(!CombatLogShown);
    //    CombatLogShown = !CombatLogShown;


    //}
    //public void ToggleChatLog()
    //{
    //    if (!ChatLogShown)
    //        ChatButtonImage.color = new Color(ChatButtonImage.color.r, ChatButtonImage.color.g, ChatButtonImage.color.b, 1);
    //    else
    //        ChatButtonImage.color = new Color(ChatButtonImage.color.r, ChatButtonImage.color.g, ChatButtonImage.color.b, 0.5f);

    //    ChatLogGO.SetActive(!ChatLogShown);
    //    ChatLogShown = !ChatLogShown;


    //}
}
