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


public class UIEncounterDetailPanel : MonoBehaviour, IEncounterDetailPanel
{
    public AccountDataSO AccountDataSO;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public PrefabFactory PrefabFactory;

    public GameObject Model;
    public EncounterData Data;
    // public UICombatMemberSkillsSpawner UISkillsSpawner;
    public UILocation UILocationEncounters;
    public UIEncounterEntry UIEncounterEntry;
    public UIEncountersSpawner UIEncountersSpawner;
    // public TextMeshProUGUI DrawDeckCardsCountText;
    // public TextMeshProUGUI DiscardDeckCardsCountText;
    public GameObject JoinEncounterButtonGO;
    //  public GameObject MyCombatToolsGO;
    public UIInventory BonusLoot_UIInventory;
    public GameObject BonusLootGO;
    public GameObject PerkTitleGO;
    public GameObject CurseCountGO;
    public GameObject RetreatGO;
    public TextMeshProUGUI CurseCountText;
    public TextMeshProUGUI ChoosePerkTitleText;
    public UIPriceTimeLabel JoinCombat_UIPriceTimeLabel;

    public UICombatBuffDescription UICombatBuffDescription;

    public GameObject CastingEffectGO;
    public GameObject UITopPanelGlobal;
    public GameObject UITopPanel;
    public GameObject UIPerkOfferPrefab;
    public Transform PerkOffersParent;
    public Transform PerkOffersPanel;


    // public TextMeshProUGUI TurnEndTimeText;
    // public TextMeshProUGUI RestButtonText;
    public TextMeshProUGUI JoinButtonText;

    public Button JoinEncounterButton;


    // public Button RestButton;
    // public Image RestTweenEffect;

    //public TweenEffects TweenEffects;

    public UnityAction<UIEncounterDetailPanel> OnRefreshed;
    public ContentFitterRefresh ContentFitterRefresh;


    // private UICombatEntity SelectedCombatEntity;
    //  private UICombatMemberSkillEntry SelectedSkill;

    private CombatMember MyCombatMemberData;



    //mam to tu proto aby po tom co se joines do encounteru byl nasledny refresh init.
    //protoze ja uz netusim jestli je to pak init nebo ne. Mam nasrany Join obrazovku i combat defakto v jedne obrazovce/skriptu, tomto, tak nepoznam rozdil
    private bool FlaggedForInitRefresh = false;

    private string oldChatText;

    // private Coroutine turnTimeLeftCourotine;

    //private void TryToFixScrollReckGlitches()
    //{
    //    //StartCoroutine(Wait());

    //}


    //private IEnumerator Wait()
    //{
    //    yield return new WaitForSecondsRealtime(0.1f);
    //    // LayoutRebuilder.ForceRebuildLayoutImmediate(TransformToFix);
    //    Canvas.ForceUpdateCanvases();
    //}

    public void OnEnable()
    {

    }

    public void OnDisable()
    {
    }

    public void Awake()
    {
        UIEncounterEntry.OnCombatEntityBuffClicked += OnCombatEntityBuffClicked;
        //    UIEncounterEntry.OnCombatEntityClicked += OnCombatEntityClicked;
        UIEncountersSpawner.OnUIEnecounterEntryClicked += Show;
        //UISkillsSpawner.OnSkillClicked += SkillClicked;
        ////UISkillsSpawner.OnSkillHoldFinished += SkillHoldFinished;
        //UISkillsSpawner.OnSkillDropedOnCombatEntity += SkillDropedOnCombatEntity;
        //UISkillsSpawner.OnStartDrag += OnSkillStartDrag;

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
        // DeselectSelectedSkill();
    }

    public void OnDestroy()
    {
        UIEncountersSpawner.OnUIEnecounterEntryClicked -= Show;
        //UISkillsSpawner.OnSkillClicked -= SkillClicked;
        //UISkillsSpawner.OnSkillHoldFinished -= SkillHoldFinished;
        //UISkillsSpawner.OnSkillDropedOnCombatEntity -= SkillDropedOnCombatEntity;

    }

    private void OnCombatEntityBuffClicked(UIBuff _buff, UICombatEntity _entity)
    {
        UICombatBuffDescription.SetData(_buff.Data);
    }

    //private void OnCombatEntityClicked(UICombatEntity _entry)
    //{
    //    SelectedCombatEntity = _entry;
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

        // UITopPanelGlobal.SetActive(!HasJoinedCombat);
        PerkOffersPanel.gameObject.SetActive(AccountDataSO.CharacterData.IsWorldPositionExplored(Data.position) && !(IAmComabatantInThisEncounter && PerkChoiceFinished));
        PerkTitleGO.SetActive(Data.PendingPerksChoicesAmount() > 0 && AccountDataSO.CharacterData.IsWorldPositionExplored(Data.position));
        Utils.DestroyAllChildren(PerkOffersParent);

        foreach (var perkOffer in Data.perksOffers)
        {

            var UIPerkOffer = PrefabFactory.CreateGameObject<UIPerkOffer>(UIPerkOfferPrefab, PerkOffersParent);
            UIPerkOffer.Setup(perkOffer, Data);
            UIPerkOffer.OnChooseClicked += OnPerkOfferClicked;
            //  UIPerkOffer.OnContentItemClicked += OnPerkContentItemClicked;
            UIPerkOffer.CheckForShowingChooserPortrait(Data);
        }


        foreach (var perkOffer in Data.perksOffersRare)
        {
            var UIPerkOffer = PrefabFactory.CreateGameObject<UIPerkOffer>(UIPerkOfferPrefab, PerkOffersParent);
            UIPerkOffer.Setup(perkOffer, Data);
            UIPerkOffer.OnChooseClicked += OnPerkOfferClicked;
            //  UIPerkOffer.OnContentItemClicked += OnPerkContentItemClicked;
            UIPerkOffer.CheckForShowingChooserPortrait(Data);
        }


        UIEncounterEntry.SetEncounter(Data, _initRefresh);


        JoinCombat_UIPriceTimeLabel.gameObject.SetActive(false);
        // JoinCombat_UIPriceTimeLabel.gameObject.SetActive(!IAmFounderOfThisEncounter);
        //   JoinCombat_UIPriceTimeLabel.gameObject.SetActive(true);
        //  JoinCombat_UIPriceTimeLabel.SetPrice(Data.joinPrice);

        if (IAmComabatantInThisEncounter && PerkChoiceFinished)
        {
            MyCombatMemberData = Data.GetCombatMemeberByUid(AccountDataSO.CharacterData.uid);
            //DrawDeckCardsCountText.SetText(MyCombatMemberData.skillsDrawDeck.Count.ToString());
            //DiscardDeckCardsCountText.SetText(MyCombatMemberData.skillsDiscardDeck.Count.ToString());
            //    CombatText.SetText(Utils.ReplacePlaceholdersInTextWithDescriptionFromMetadata(Data.combatLog));
            //OnCombatLogChanged.Invoke(Utils.ReplacePlaceholdersInTextWithDescriptionFromMetadata(Data.combatLog));
            //UISkillsSpawner.Show(MyCombatMemberData);

            //RestButton.interactable = !MyCombatMemberData.hasRested;

            //if (!MyCombatMemberData.hasRested && !MyCombatMemberData.HasEnoughManaToCastAnySkill())
            //{
            //    TweenEffects.HighlightEffect(RestTweenEffect, Color.yellow);
            //}
            //else
            //    TweenEffects.KillTween(RestTweenEffect.gameObject);

            //if (!MyCombatMemberData.hasRested)
            //    if (MyCombatMemberData.skillsDrawDeck.Count >= 5)
            //    {
            //        RestButtonText.SetText("End Turn");//(<color=\"yellow\">1% Fatigue</color>)")
            //                                           // RestButton.colors = newC .normalColor = Color.white;
            //    }
            //    else
            //    {
            //        //Debug.Log(AccountDataSO.OtherMetadataData.constants.deckShuffleMaxHpPenalty);
            //        //Debug.Log(MyCombatMemberData.stats.healthMax);
            //        //Debug.Log(MyCombatMemberData.deckShuffleCount + 1);
            //        //float healthTaken = AccountDataSO.OtherMetadataData.constants.deckShuffleMaxHpPenalty * MyCombatMemberData.stats.healthMax * (MyCombatMemberData.deckShuffleCount + 1);
            //        //if (healthTaken >= MyCombatMemberData.stats.health)
            //        //    healthTaken = MyCombatMemberData.stats.health - 1;

            //        RestButtonText.SetText("End Turn & Reshuffle");//(<color=\"red\"> -" + Utils.RoundToInt(healthTaken) + " HP</color> )");
            //    }
            //else
            //    RestButtonText.SetText("Waiting for others to end turn...");

            //UISkillsSpawner.gameObject.SetActive(Data.GetCombatMemeberByUid(AccountDataSO.CharacterData.uid).stats.health > 0);
            //RestButton.gameObject.SetActive(Data.GetCombatMemeberByUid(AccountDataSO.CharacterData.uid).stats.health > 0);


            //if (!UIEncounterEntry.IsAnyItemSelected())
            //    UIEncounterEntry.ClickOnRandomEnemy();

            CancelInvoke();
            InvokeRepeating("RefreshTurnTimeLeft", 0f, 1f);

        }




        //if (IAmFounderOfThisEncounter)
        RetreatGO.SetActive(false);
        //else
        // RetreatGO.SetActive(IAmComabatantInThisEncounter && !PerkChoiceFinished);

        //  RetreatGO.SetActive(!IsParticipating);
        //    RetreatGO.SetActive(!IsParticipating);
        //else
        //    RetreatGO.SetActive(!IsParticipating);

        BonusLootGO.SetActive(Data.bonusLoot.Count > 0 && !IAmComabatantInThisEncounter);
        BonusLoot_UIInventory.Refresh(Data.bonusLoot);

        CurseCountGO.SetActive(Data.curseCount > 0);
        CurseCountText.SetText(Data.curseCount.ToString());

        UITopPanel.SetActive(!(IAmComabatantInThisEncounter && PerkChoiceFinished));
        JoinEncounterButtonGO.SetActive(!IAmComabatantInThisEncounter && Data.enemies.Count > 0);
        if (Data.PendingPerksChoicesAmount() > 0)
            ChoosePerkTitleText.SetText("Choose perk reward " + Data.PendingPerksChoicesAmount() + " more player");
        else
            ChoosePerkTitleText.SetText("Perk rewards chosen");
        //  SituationDescriptionText.gameObject.SetActive(!IsParticipating);
        //MyCombatToolsGO.SetActive(IAmComabatantInThisEncounter && PerkChoiceFinished);
        //DiscardDeckCardsCountText.gameObject.SetActive(IAmComabatantInThisEncounter && PerkChoiceFinished);
        //DrawDeckCardsCountText.gameObject.SetActive(IAmComabatantInThisEncounter && PerkChoiceFinished);
        UIEncounterEntry.ShowBasicInfoPanel(!(IAmComabatantInThisEncounter && PerkChoiceFinished));
        JoinButtonText.SetText("<b>Join!</b>");

        //if (AccountDataSO.CharacterData.currency.fatigue > 0)
        //    JoinButtonText.SetText(JoinButtonText.text + "<color=\"white\">You start with <color=\"red\">" + (AccountDataSO.CharacterData.GetTotalHealth(true) - AccountDataSO.CharacterData.GetTotalHealth(false)).ToString() + " HP </color>less due to Fatigue</color>");

        bool hasEnoughtTime = AccountDataSO.CharacterData.currency.time >= Data.joinPrice;
        if (IAmFounderOfThisEncounter)
            hasEnoughtTime = true;

        JoinEncounterButton.interactable = AccountDataSO.CharacterData.currency.fatigue <= 90 && hasEnoughtTime;


        if (AccountDataSO.CharacterData.currency.fatigue > 90)
            JoinButtonText.SetText(JoinButtonText.text + "<color=\"white\">Your are too fatigued! Rest until your fatigue is bellow 90% </color>");




        OnRefreshed?.Invoke(this);
        ContentFitterRefresh.RefreshContentFitters();
    }

    //private void OnPerkContentItemClicked(UIContentItem _item)
    //{
    //    UIManager.instance.ContextInfoPanel.ShowContentContainerDetail(_item.Data);
    //}

    //private void RefreshTurnTimeLeft()
    //{
    //    TurnEndTimeText.SetText(Data.GetTurnTimeTimeLeft());
    //}

    //public void DeselectSelectedSkill()
    //{
    //    //if (SelectedSkillDetail.Data != null)
    //    //{
    //    //    SelectedSkillDetail.transform.parent.gameObject.SetActive(false);
    //    //    SelectedSkillDetail.Data = null;
    //    //    SelectedSkill.ShowAsSelected(false);
    //    //    SelectedSkill = null;

    //    //}
    //    if (SelectedSkill == null)
    //        return;

    //    SelectedSkill.ShowAsSelected(false);
    //    SelectedSkill = null;

    //    UIManager.instance.ContextInfoPanel.Hide();

    //}

    //public void SkillHoldFinished(UICombatMemberSkillEntry _skill)
    //{
    //    SelectedSkill = _skill;
    //    CastSelectedSkill();
    ////}
    //public void SkillClicked(UICombatMemberSkillEntry _skill)
    //{
    //    if (SelectedSkill == _skill)
    //    {
    //        DeselectSelectedSkill();
    //    }
    //    else
    //    {
    //        //   SelectedSkillDetail.transform.parent.gameObject.SetActive(true);

    //        if (SelectedSkill != null)
    //            SelectedSkill.ShowAsSelected(false);

    //        SelectedSkill = _skill;
    //        SelectedSkill.ShowAsSelected(true);

    //        //    SelectedSkillDetail.SetData(_skill.Data, UISkillsSpawner);
    //        // UIManager.instance.ContextInfoPanel.ShowContextCombatSkill(_skill.Data, MyCombatMemberData.stats.mana);
    //    }
    //}



    //public async void CastSelectedSkill()
    //{
    //    if (SelectedCombatEntity == null)
    //    {
    //        UIManager.instance.ImportantMessage.ShowMesssage("Choose your target!");
    //        return;
    //    }
    //    if (SelectedSkill.Data.manaCost < 0)
    //    {

    //        UIManager.instance.ImportantMessage.ShowMesssage("This skill is unplayable!");
    //        return;
    //    }

    //    var myUICombatEntity = UIEncounterEntry.GetUICombatEntityByUid(MyCombatMemberData.uid);

    //    myUICombatEntity.FloatingTextSpawner.Spawn("Casting...", Color.blue, myUICombatEntity.FloatingTextsParent);
    //    CastingEffectGO.gameObject.SetActive(true);

    //    var result = await FirebaseCloudFunctionSO.ApplySkillOnEncounter(Data.uid, SelectedSkill.Data.uid, SelectedCombatEntity.Data.uid);
    //    if (result.Result)
    //    {
    //        myUICombatEntity.FloatingTextSpawner.Spawn("Casting Done!", Color.cyan, myUICombatEntity.FloatingTextsParent);
    //    }
    //    CastingEffectGO.SetActive(false);
    //    DeselectSelectedSkill();
    //}

    public void EndTurnClicked()
    {
        // SelectedSkillDetail.transform.parent.gameObject.SetActive(false);
        //    UIManager.instance.ContextInfoPanel.Hide();
        FirebaseCloudFunctionSO.RestEncounter(Data.uid);
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

    //public void FleeFromEncounter()
    //{
    //    int penalty = Utils.FLEE_FATIGUE_PENALTY + Data.GetCombatMemeberByUid(AccountDataSO.CharacterData.uid).deckShuffleCount;
    //    string prompt = "";
    //    prompt = "Do you want to flee from combat? You will suffer <color=\"yellow\">" + penalty + "%</color> Fatigue penalty ";

    //    if (AccountDataSO.CharacterData.hasBless(Utils.BLESS.UNWEARIED))
    //        prompt = "Do you want to flee from combat? <s>You will suffer <color=\"yellow\">" + penalty + "%</color> Fatigue penalty</s> (" + Utils.DescriptionsMetadata.GetBlessesMetadata(Utils.BLESS.UNWEARIED).title.EN + ")";

    //    UIManager.instance.SpawnPromptPanel(prompt, () =>//and flee to <color=\"yellow\">" + Utils.DescriptionsMetadata.GetPointsOfInterestMetadata(AccountDataSO.LocationData.graveyard).title.GetText() + "</color>", () =>
    //    {
    //        FirebaseCloudFunctionSO.FleeFromEncounter(Data.uid);
    //    }, null);

    //}

    //public void RetreatFromEncounter()
    //{
    //    FirebaseCloudFunctionSO.RetreatFromEncounter(Data.uid);

    //}

    public void OnPerkOfferClicked(UIPerkOffer _perkOffer)
    {
        FirebaseCloudFunctionSO.ChooseEncounterPerkOffer(Data.uid, _perkOffer.Data.uid);
    }
}
