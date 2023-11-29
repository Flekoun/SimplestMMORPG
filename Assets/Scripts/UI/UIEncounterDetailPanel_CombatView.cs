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


public class UIEncounterDetailPanel_CombatView : MonoBehaviour, IEncounterDetailPanel
{
    public AccountDataSO AccountDataSO;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public PrefabFactory PrefabFactory;

    public GameObject Model;
    public EncounterData Data;
    public UICombatMemberSkillsSpawner UISkillsSpawner;
    public UILocation UILocationEncounters;
    public UIEncounterEntry UIEncounterEntry;
    public UIEncountersSpawner UIEncountersSpawner;
    public TextMeshProUGUI DrawDeckCardsCountText;
    public TextMeshProUGUI DiscardDeckCardsCountText;

    public GameObject MyCombatToolsGO;

    // public UICombatBuffDescription UICombatBuffDescription;

    public GameObject CastingEffectGO;
    public GameObject UITopPanelGlobal;
    public GameObject UITopPanel;


    public TextMeshProUGUI TurnEndTimeText;
    public TextMeshProUGUI RestButtonText;


    public Button RestButton;
    public Image RestTweenEffect;

    public TweenEffects TweenEffects;

    public UnityAction<UIEncounterDetailPanel_CombatView> OnRefreshed;
    public ContentFitterRefresh ContentFitterRefresh;


    // private UICombatEntity SelectedCombatEntity;
    //private UICombatMemberSkillEntry SelectedSkill;

    private CombatMember MyCombatMemberData;



    //mam to tu proto aby po tom co se joines do encounteru byl nasledny refresh init.
    //protoze ja uz netusim jestli je to pak init nebo ne. Mam nasrany Join obrazovku i combat defakto v jedne obrazovce/skriptu, tomto, tak nepoznam rozdil
    private bool FlaggedForInitRefresh = false;

    private string oldChatText;



    public void OnEnable()
    {

    }

    public void OnDisable()
    {
    }

    public void Awake()
    {
        ///  UIEncounterEntry.OnCombatEntityBuffClicked += OnCombatEntityBuffClicked;

        UIEncountersSpawner.OnUIEnecounterEntryClicked += Show;
        //UISkillsSpawner.OnSkillClicked += SkillClicked;

        UISkillsSpawner.OnSkillDropedOnCombatEntity += SkillDropedOnCombatEntity;
        UISkillsSpawner.OnStartDrag += OnSkillStartDrag;

    }

    private void OnSkillStartDrag()
    {
        throw new NotImplementedException();
    }

    private async void SkillDropedOnCombatEntity(UICombatEntity _entity, UICombatMemberSkillEntry _skill)
    {
        var position = new Vector3(_entity.transform.position.x, _entity.transform.position.y, _entity.transform.position.z);
        Debug.Log("position.x " + position.x + "position.y " + position.y);
        if (_entity == null)
        {
            UIManager.instance.ImportantMessage.ShowMesssage("Choose your target!");
            return;
        }
        if (_skill.Data.manaCost < 0)
        {

            UIManager.instance.ImportantMessage.ShowMesssage("This skill is unplayable!");
            return;
        }

        var myUICombatEntity = UIEncounterEntry.GetUICombatEntityByUid(MyCombatMemberData.uid);

        //  myUICombatEntity.FloatingTextSpawner.Spawn("Casting...", Color.blue, myUICombatEntity.FloatingTextsParent);
        CastingEffectGO.gameObject.SetActive(true);

        var result = await FirebaseCloudFunctionSO.ApplySkillOnEncounter(Data.uid, _skill.Data.uid, _entity.Data.uid);
        if (result.Result)
        {
            Debug.Log("A position.x " + position.x + "position.y " + position.y);
            UIManager.instance.SpawnParticleEffectUIPosition(position);
            // myUICombatEntity.FloatingTextSpawner.Spawn("Casting Done!", Color.cyan, myUICombatEntity.FloatingTextsParent);
        }
        else
        {
            _skill.SkillFailedToBeCasted();
            myUICombatEntity.FloatingTextSpawner.Spawn("Casting Failed!", Color.red, myUICombatEntity.FloatingTextsParent);
        }
        CastingEffectGO.SetActive(false);
        //DeselectSelectedSkill();
    }

    public void OnDestroy()
    {
        UIEncountersSpawner.OnUIEnecounterEntryClicked -= Show;
        //UISkillsSpawner.OnSkillClicked -= SkillClicked;
        //UISkillsSpawner.OnSkillHoldFinished -= SkillHoldFinished;
        UISkillsSpawner.OnSkillDropedOnCombatEntity -= SkillDropedOnCombatEntity;
        //   UIManager.instance.ContextInfoPanel.ShowContextCombatSkill
    }

    //private void OnCombatEntityBuffClicked(UIBuff _buff, UICombatEntity _entity)
    //{
    //    // UIManager.instance.sh
    //    // UICombatBuffDescription.SetData(_buff.Data);
    //}




    public void Show(EncounterData _data)
    {
        UILocationEncounters.Hide();

        //        string oldChatText = "";
        Data = _data;

        //AccountDataSO.OnCharacterDataChanged += Refresh;
        //AccountDataSO.OnEncounterDataChanged += Refresh;

        Model.SetActive(true);

        Refresh(true);

    }

    public void Hide()
    {
        CancelInvoke();

        //AccountDataSO.OnCharacterDataChanged -= Refresh;
        //AccountDataSO.OnEncounterDataChanged -= Refresh;
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
        bool PerkChoiceFinished = (Data.PendingPerksChoicesAmount() == 0);

        //  bool HasJoinedCombat = Data.IsParticipatingInCombat(AccountDataSO.CharacterData.uid);




        UIEncounterEntry.SetEncounter(Data, _initRefresh);




        if (IAmComabatantInThisEncounter && PerkChoiceFinished)
        {
            MyCombatMemberData = Data.GetCombatMemeberByUid(AccountDataSO.CharacterData.uid);
            DrawDeckCardsCountText.SetText(MyCombatMemberData.skillsDrawDeck.Count.ToString());
            DiscardDeckCardsCountText.SetText(MyCombatMemberData.skillsDiscardDeck.Count.ToString());
            //    CombatText.SetText(Utils.ReplacePlaceholdersInTextWithDescriptionFromMetadata(Data.combatLog));
            //OnCombatLogChanged.Invoke(Utils.ReplacePlaceholdersInTextWithDescriptionFromMetadata(Data.combatLog));
            UISkillsSpawner.Show(MyCombatMemberData);

            RestButton.interactable = !MyCombatMemberData.hasRested;

            if (!MyCombatMemberData.hasRested && !MyCombatMemberData.HasEnoughManaToCastAnySkill())
            {
                TweenEffects.HighlightEffect(RestTweenEffect, Color.yellow);
            }
            else
                TweenEffects.KillTween(RestTweenEffect.gameObject);

            if (!MyCombatMemberData.hasRested)
                if (MyCombatMemberData.skillsDrawDeck.Count >= 5)
                {
                    RestButtonText.SetText("End Turn");//(<color=\"yellow\">1% Fatigue</color>)")
                                                       // RestButton.colors = newC .normalColor = Color.white;
                }
                else
                {


                    RestButtonText.SetText("End Turn & Reshuffle");//(<color=\"red\"> -" + Utils.RoundToInt(healthTaken) + " HP</color> )");
                }
            else
                RestButtonText.SetText("Waiting for others to end turn...");

            UISkillsSpawner.gameObject.SetActive(Data.GetCombatMemeberByUid(AccountDataSO.CharacterData.uid).stats.health > 0);
            RestButton.gameObject.SetActive(Data.GetCombatMemeberByUid(AccountDataSO.CharacterData.uid).stats.health > 0);


            //if (!UIEncounterEntry.IsAnyItemSelected())
            //    UIEncounterEntry.ClickOnRandomEnemy();

            CancelInvoke();
            InvokeRepeating("RefreshTurnTimeLeft", 0f, 1f);

        }





        UITopPanel.SetActive(!(IAmComabatantInThisEncounter && PerkChoiceFinished));

        //  SituationDescriptionText.gameObject.SetActive(!IsParticipating);
        MyCombatToolsGO.SetActive(IAmComabatantInThisEncounter && PerkChoiceFinished);
        DiscardDeckCardsCountText.gameObject.SetActive(IAmComabatantInThisEncounter && PerkChoiceFinished);
        DrawDeckCardsCountText.gameObject.SetActive(IAmComabatantInThisEncounter && PerkChoiceFinished);
        UIEncounterEntry.ShowBasicInfoPanel(!(IAmComabatantInThisEncounter && PerkChoiceFinished));

        //if (AccountDataSO.CharacterData.currency.fatigue > 0)
        //    JoinButtonText.SetText(JoinButtonText.text + "<color=\"white\">You start with <color=\"red\">" + (AccountDataSO.CharacterData.GetTotalHealth(true) - AccountDataSO.CharacterData.GetTotalHealth(false)).ToString() + " HP </color>less due to Fatigue</color>");

        bool hasEnoughtTime = AccountDataSO.CharacterData.currency.time >= Data.joinPrice;
        if (IAmFounderOfThisEncounter)
            hasEnoughtTime = true;



        OnRefreshed?.Invoke(this);
        ContentFitterRefresh.RefreshContentFitters();
    }

    //private void OnPerkContentItemClicked(UIContentItem _item)
    //{
    //    UIManager.instance.ContextInfoPanel.ShowContentContainerDetail(_item.Data);
    //}

    private void RefreshTurnTimeLeft()
    {
        TurnEndTimeText.SetText(Data.GetTurnTimeTimeLeft());
    }

    //public void DeselectSelectedSkill()
    //{

    //    if (SelectedSkill == null)
    //        return;

    //    SelectedSkill.ShowAsSelected(false);
    //    SelectedSkill = null;

    //    UIManager.instance.ContextInfoPanel.Hide();

    //}

    //public void SkillClicked(UICombatMemberSkillEntry _skill)
    //{
    //    if (SelectedSkill == _skill)
    //    {
    //        DeselectSelectedSkill();
    //    }
    //    else
    //    {


    //        if (SelectedSkill != null)
    //            SelectedSkill.ShowAsSelected(false);

    //        SelectedSkill = _skill;
    //        SelectedSkill.ShowAsSelected(true);
    //        //   UIManager.instance.ContextInfoPanel.ShowContextCombatSkill(_skill.Data, MyCombatMemberData.stats.mana);
    //    }
    //}



    public void EndTurnClicked()
    {
        // SelectedSkillDetail.transform.parent.gameObject.SetActive(false);
        UIManager.instance.ContextInfoPanel.Hide();
        FirebaseCloudFunctionSO.RestEncounter(Data.uid);
    }

    public void JoinEncounterClicked()
    {

        FirebaseCloudFunctionSO.JoinEncounter(Data.uid);
        FlaggedForInitRefresh = true;

    }

    public void FleeFromEncounter()
    {
        int penalty = AccountDataSO.OtherMetadataData.constants.FLEE_FATIGUE_PENALTY + Data.GetCombatMemeberByUid(AccountDataSO.CharacterData.uid).deckShuffleCount * AccountDataSO.OtherMetadataData.constants.DECK_SHUFFLE_FATIGUE_PENALTY;
        string prompt = "";
        prompt = "Do you want to flee from combat? You will suffer <color=\"yellow\">" + penalty + "%</color> Fatigue penalty.";

        if (AccountDataSO.CharacterData.hasBless(Utils.BLESS.UNWEARIED))
            prompt = "Do you want to flee from combat? <s>You will suffer <color=\"yellow\">" + penalty + "%</color> Fatigue penalty</s> (" + Utils.DescriptionsMetadata.GetBlessesMetadata(Utils.BLESS.UNWEARIED).title.EN + ").";

        if (Data.GetCombatMemeberByUid(AccountDataSO.CharacterData.uid).stats.health == 0)
            prompt += "You will be revived at your home tavern but also suffer a random <color=\"pink\">Curse</color>.";

        UIManager.instance.SpawnPromptPanel(prompt, () =>//and flee to <color=\"yellow\">" + Utils.DescriptionsMetadata.GetPointsOfInterestMetadata(AccountDataSO.LocationData.graveyard).title.GetText() + "</color>", () =>
        {
            FirebaseCloudFunctionSO.FleeFromEncounter(Data.uid);
        }, null);

    }

    public void RetreatFromEncounter()
    {
        FirebaseCloudFunctionSO.RetreatFromEncounter(Data.uid);

    }

    //public void OnPerkOfferClicked(UIPerkOffer _perkOffer)
    //{
    //    FirebaseCloudFunctionSO.ChooseEncounterPerkOffer(Data.uid, _perkOffer.Data.uid);
    //}
}
