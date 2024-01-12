using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.realtimeDatabaseData;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using simplestmmorpg.data;

public class UIChatMessageSpawner : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public PrefabFactory PrefabFactory;
    public GameObject Prefab;
    //public Transform Parent_ZoneMessages;
    //public Transform Parent_LocationMessages;
    //public Transform Parent_PartyMessages;
    public RealtimeDatabaseChat RealtimeDatabaseChat;
    public RectTransform ScrollRectZone;
    public RectTransform ScrollRectLocation;
    public RectTransform ScrollRectParty;
    public RectTransform ScrollRectCombatLog;

    public Button PartyChannelButton;
    public Button ZoneChannelButton;
    public Button LocationChannelButton;
    public Button CombatLogButton;

    public TextMeshProUGUI LocationChannelText;

    public GameObject NewMessageBadgeGOParty;
    public GameObject NewMessageBadgeGOLocation;
    public GameObject NewMessageBadgeGOZone;
    public GameObject NewMessageBadgeGOCombatLog;
    public GameObject ChannelPanel;

    public Color ColorActiveChatButton;
    public Color ColorInActiveChatButton;

    public UnityAction<UIChatEntry> OnChatEntryClicked;

    private List<UIChatEntry> EntriesList_Zone = new List<UIChatEntry>();
    private List<UIChatEntry> EntriesList_Location = new List<UIChatEntry>();
    private List<UIChatEntry> EntriesList_Party = new List<UIChatEntry>();

    public TextMeshProUGUI CombatLogText;

    public UIEncounterDetailPanel_CombatView UIEncounterDetailPanel_CombatView;

    private IEnumerator Wait()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        LayoutRebuilder.ForceRebuildLayoutImmediate(ScrollRectZone);
        LayoutRebuilder.ForceRebuildLayoutImmediate(ScrollRectLocation);
        LayoutRebuilder.ForceRebuildLayoutImmediate(ScrollRectParty);
        Canvas.ForceUpdateCanvases();
    }

    public void Start()
    {
        ShowLocationChat();
    }
    // Start is called before the first frame update
    void Awake()
    {
        UIEncounterDetailPanel_CombatView.OnRefreshed += UpdateCombatLog; //mam to tu proto ze potrebuji ziskat ten jeden encounter spravny, ktery mas defakto ted aktivni v ui...


        RealtimeDatabaseChat.OnNewZoneChatMessageAdded += OnNewZoneMessage;
        RealtimeDatabaseChat.OnNewLocationChatMessageAdded += OnNewLocationMessage;
        RealtimeDatabaseChat.OnNewPartyMessageAdded += OnNewPartyMessage;

        RealtimeDatabaseChat.OnZoneChatChannelChanged += ClearLocationChat;
        RealtimeDatabaseChat.OnLocationChatChannelChanged += ClearLocationChat;
        RealtimeDatabaseChat.OnPartyChannelChanged += ClearPartyChat;

        AccountDataSO.OnPartyDataChanged += OnPartyDataChanged;
        AccountDataSO.OnWorldLocationChanged += OnWorldLocationChanged;
        AccountDataSO.OnCharacterLoadedFirstTime += OnWorldLocationChanged;
    }

    private void OnWorldLocationChanged()
    {
        LocationChannelText.SetText(Utils.DescriptionsMetadata.GetLocationsMetadata(AccountDataSO.CharacterData.position.locationId).descriptionData.title.GetText());
    }

    public void ShowPartyChat()
    {
        ScrollRectParty.parent.gameObject.SetActive(true);
        ScrollRectLocation.parent.gameObject.SetActive(false);
        ScrollRectZone.parent.gameObject.SetActive(false);
        ScrollRectCombatLog.parent.gameObject.SetActive(false);

        PartyChannelButton.targetGraphic.color = ColorActiveChatButton;
        LocationChannelButton.targetGraphic.color = ColorInActiveChatButton;
        ZoneChannelButton.targetGraphic.color = ColorInActiveChatButton;
        CombatLogButton.targetGraphic.color = ColorInActiveChatButton;
        //    Utils.SetAlphaColorFromGivenImage(LocationChannelButton.targetGraphic, 0.5f);
        //  Utils.SetAlphaColorFromGivenImage(ZoneChannelButton.targetGraphic, 0.5f);


        NewMessageBadgeGOParty.gameObject.SetActive(false);

        StartCoroutine(Wait());
    }

    public void ShowLocationChat()
    {
        Debug.Log("EH?2");
        ScrollRectParty.parent.gameObject.SetActive(false);
        ScrollRectZone.parent.gameObject.SetActive(false);
        ScrollRectLocation.parent.gameObject.SetActive(true);
        ScrollRectCombatLog.parent.gameObject.SetActive(false);

        PartyChannelButton.targetGraphic.color = ColorInActiveChatButton;
        LocationChannelButton.targetGraphic.color = ColorActiveChatButton;
        ZoneChannelButton.targetGraphic.color = ColorInActiveChatButton;
        CombatLogButton.targetGraphic.color = ColorInActiveChatButton;

        //Utils.SetAlphaColorFromGivenImage(PartyChannelButton.targetGraphic, 0.5f);
        //Utils.SetAlphaColorFromGivenImage(LocationChannelButton.targetGraphic, 1f);
        //Utils.SetAlphaColorFromGivenImage(ZoneChannelButton.targetGraphic, 0.5f);

        NewMessageBadgeGOLocation.gameObject.SetActive(false);

        StartCoroutine(Wait());
    }

    public void ShowZoneChat()
    {
        ScrollRectParty.parent.gameObject.SetActive(false);
        ScrollRectZone.parent.gameObject.SetActive(true);
        ScrollRectLocation.parent.gameObject.SetActive(false);
        ScrollRectCombatLog.parent.gameObject.SetActive(false);

        PartyChannelButton.targetGraphic.color = ColorInActiveChatButton;
        LocationChannelButton.targetGraphic.color = ColorInActiveChatButton;
        ZoneChannelButton.targetGraphic.color = ColorActiveChatButton;
        CombatLogButton.targetGraphic.color = ColorInActiveChatButton;

        NewMessageBadgeGOZone.gameObject.SetActive(false);

        StartCoroutine(Wait());
    }

    public void ShowCombatLog()
    {
     //   Debug.Log("EH?");
        ScrollRectParty.parent.gameObject.SetActive(false);
        ScrollRectZone.parent.gameObject.SetActive(false);
        ScrollRectLocation.parent.gameObject.SetActive(false);
        ScrollRectCombatLog.parent.gameObject.SetActive(true);

        PartyChannelButton.targetGraphic.color = ColorInActiveChatButton;
        LocationChannelButton.targetGraphic.color = ColorInActiveChatButton;
        ZoneChannelButton.targetGraphic.color = ColorInActiveChatButton;
        CombatLogButton.targetGraphic.color = ColorActiveChatButton;

        NewMessageBadgeGOCombatLog.gameObject.SetActive(false);

        StartCoroutine(Wait());
    }

    public void UpdateCombatLog(UIEncounterDetailPanel_CombatView _encounterDetail)
    {
        if (_encounterDetail.Data != null)

        {
            CombatLogText.SetText(Utils.ReplacePlaceholdersInTextWithDescriptionFromMetadata(_encounterDetail.Data.combatLog));
            CombatLogButton.gameObject.SetActive(true);

        }
        else
        {
            CombatLogButton.gameObject.SetActive(false);
            ShowZoneChat();
        }
    }


    public void OnPartyDataChanged()
    {
        PartyChannelButton.gameObject.SetActive(AccountDataSO.PartyData != null);
    }

    private void ClearLocationChat()
    {
        for (int i = EntriesList_Location.Count - 1; i >= 0; i--)
        {

            if (EntriesList_Location[i].Data.channelType == CHANNEL_TYPE.LOCATION)
            {
                UIChatEntry entry = EntriesList_Location[i];
                EntriesList_Location.RemoveAt(i);

                Destroy(entry.gameObject);
            }
        }
    }

    private void ClearZoneChat()
    {
        for (int i = EntriesList_Zone.Count - 1; i >= 0; i--)
        {

            if (EntriesList_Zone[i].Data.channelType == CHANNEL_TYPE.ZONE)
            {
                UIChatEntry entry = EntriesList_Zone[i];
                EntriesList_Zone.RemoveAt(i);

                Destroy(entry.gameObject);
            }
        }
    }

    private void ClearPartyChat()
    {
        for (int i = EntriesList_Party.Count - 1; i >= 0; i--)
        {

            if (EntriesList_Party[i].Data.channelType == CHANNEL_TYPE.PARTY)
            {
                UIChatEntry entry = EntriesList_Party[i];
                EntriesList_Party.RemoveAt(i);

                Destroy(entry.gameObject);
            }
        }
    }

    private void OnNewLocationMessage(RealtimeDatabaseChatMessageData _msg)
    {
        var msg = PrefabFactory.CreateGameObject<UIChatEntry>(Prefab, ScrollRectLocation);
        msg.Setup(_msg);
        EntriesList_Location.Add(msg);
        msg.OnClicked += OnEntryClicked;

        if (RealtimeDatabaseChat.ActiveChannel != CHANNEL_TYPE.LOCATION)
            NewMessageBadgeGOLocation.gameObject.SetActive(true);

        StartCoroutine(Wait());
    }

    private void OnNewZoneMessage(RealtimeDatabaseChatMessageData _msg)
    {
        var msg = PrefabFactory.CreateGameObject<UIChatEntry>(Prefab, ScrollRectZone);
        msg.Setup(_msg);
        EntriesList_Zone.Add(msg);
        msg.OnClicked += OnEntryClicked;

        if (RealtimeDatabaseChat.ActiveChannel != CHANNEL_TYPE.ZONE)
            NewMessageBadgeGOZone.gameObject.SetActive(true);

        StartCoroutine(Wait());
    }

    private void OnNewPartyMessage(RealtimeDatabaseChatMessageData _msg)
    {
        var msg = PrefabFactory.CreateGameObject<UIChatEntry>(Prefab, ScrollRectParty);
        msg.Setup(_msg);
        EntriesList_Party.Add(msg);
        msg.OnClicked += OnEntryClicked;

        if (RealtimeDatabaseChat.ActiveChannel != CHANNEL_TYPE.PARTY)
            NewMessageBadgeGOParty.gameObject.SetActive(true);

        StartCoroutine(Wait());
    }

    private void OnEntryClicked(UIChatEntry _entry)
    {
        OnChatEntryClicked.Invoke(_entry);
    }



}