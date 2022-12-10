using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Firestore;
using Newtonsoft.Json;
using RoboRyanTron.Unite2017.Variables;

using simplestmmorpg.data;

using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu]
public class AccountDataSO : ScriptableObject
{
    //public CharacterData GetCharacterData()
    //{
    //    public object MyProperty { get; set; }
    //    return CharacterData;
    //}


    // public SkillsMetadata SkillsMetadata;
    public CharacterData CharacterData;
    public PlayerData PlayerData;
    public Party PartyData;
    public EncounterData RareEncounterData;
    public List<EncounterData> EncountersData;
    public List<EncounterResult> EncounterResultsData;
    public List<Vendor> VendorsData;
    public List<InboxItem> InboxData;
    public PartyInvite PartyInviteData;
    public Maps MapsData;
    public int PlayersOnline;
    public OtherMetadata OtherMetadataData;

//    public List<PartyFinderData> PartyFinderData;

    public UnityAction OnMapsChanged;
    public UnityAction OnDescriptionsMetadataChanged;
    public UnityAction OnOtherMetadataChanged;
    public UnityAction OnCharacterDataChanged;
    public UnityAction<CharacterData> OnCharacterDataChanged_OldData;
    public UnityAction OnPlayerDataChanged;
    public UnityAction<PlayerData> OnPlayerDataChanged_OldData;
    public UnityAction OnRareEncounterDataChanged;
    public UnityAction OnPartyFinderDataChanged;
    public UnityAction OnPartyDataChanged;
    public UnityAction OnEncounterDataChanged;
    public UnityAction OnEncounterResultsDataChanged;
    public UnityAction OnInboxDataChanged;
    public UnityAction OnPartyInviteDataChanged;
    public UnityAction OnVendorsDataChanged;
    public UnityAction OnEncounterLoadedFirstTime;
    public UnityAction OnCharacterLoadedFirstTime;
    public UnityAction OnPlayerDataLoadedFirstTime;
    public UnityAction OnSkillsMetadataLoadedFirstTime;

    public UnityAction OnWorldPositionChanged;
    public UnityAction OnWorldLocationChanged;



    [SerializeField] private StringVariable CharacterName;
    [SerializeField] private StringVariable CharacterUid;

    [SerializeField] private IntVariable Gold;
    [SerializeField] private IntVariable Silver;
    [SerializeField] private IntVariable Time;
    [SerializeField] private IntVariable Fatigue;
    [SerializeField] private StringVariable LocationName;
    [SerializeField] private StringVariable ZoneName;
    [SerializeField] private IntVariable OnlinePlayersCount;


    [NonSerialized]
    private bool DescriptionsMetadataChangedFirstTime = true;
    [NonSerialized]
    private bool EncountersDataChangedFirstTime = true;
    [NonSerialized]
    private bool CharacterDataChangedFirstTime = true;
    [NonSerialized]
    private bool PlayerDataDataChangedFirstTime = true;

    [NonSerialized]
    private WorldPosition LastWorldPosition = null;


    public void SetDescriptionsMetadata(DocumentSnapshot _skillsMetaSnapshot)
    {

        Utils.SetDescriptionMetadata(_skillsMetaSnapshot.ConvertTo<DescriptionsMetadata>());

        if (OnDescriptionsMetadataChanged != null)
            OnDescriptionsMetadataChanged.Invoke();

        if (DescriptionsMetadataChangedFirstTime)
            OnSkillsMetadataLoadedFirstTime.Invoke();

        DescriptionsMetadataChangedFirstTime = false;
    }


    public void SetOtherMetadata(DocumentSnapshot _snapshot)
    {
        OtherMetadataData = _snapshot.ConvertTo<OtherMetadata>();

        if (OnOtherMetadataChanged != null)
            OnOtherMetadataChanged.Invoke();
    }

    public void SetOnlinePlayersCount(long _amount)
    {
        PlayersOnline = (int)_amount;
        OnlinePlayersCount.SetValue((int)PlayersOnline);
    }



    public void SetMaps(DocumentSnapshot _snapshot)
    {

        MapsData = _snapshot.ConvertTo<Maps>();

        if (OnMapsChanged != null)
            OnMapsChanged.Invoke();

        //if (SkillsMetadataChangedFirstTime)
        //    OnSkillsMetadataLoadedFirstTime.Invoke();

        //SkillsMetadataChangedFirstTime = false;
    }

    CharacterData oldDataCharacter = null;
    public void SetCharacterData(DocumentSnapshot _characterSnapshot)
    {
        var freshEntry = _characterSnapshot.ConvertTo<CharacterData>();

        // tady je issue ze toto nefacha a stejne ztracim referenci na inventory ...
        //   Utils.CopyPropertiesTo(freshEntry, CharacterData);


        //Utils.CopyPropertiesTo(freshEntry.inventory, CharacterData.inventory);

        //TODO: jen v tomhle poradi to funguje?! wtf... a to jen castecne? kdyz mam otevreny ten inventory a pak ziskam item a pak prodavam tak ty prodeje nejdou videt?
        //    Utils.CopyPropertiesTo(freshEntry.inventory, CharacterData.inventory);
        //    Utils.CopyPropertiesTo(freshEntry, CharacterData);


        CharacterData = _characterSnapshot.ConvertTo<CharacterData>();
        //   CharacterData.uid = _characterSnapshot.Id;


        CharacterUid.SetValue(CharacterData.uid);
        CharacterName.SetValue(CharacterData.characterName);

        Gold.SetValue(CharacterData.currency.gold);
        Silver.SetValue(CharacterData.currency.silver);
        Time.SetValue(CharacterData.currency.time);
        Fatigue.SetValue(CharacterData.currency.fatigue);
        LocationName.SetValue(Utils.GetMetadataForLocation(CharacterData.position.locationId).title.GetText());
        ZoneName.SetValue(CharacterData.position.zoneId);

        Debug.Log("New Data for CHARACTER Set");

        if (OnCharacterDataChanged_OldData != null)
            OnCharacterDataChanged_OldData.Invoke(oldDataCharacter);

        oldDataCharacter = _characterSnapshot.ConvertTo<CharacterData>();

        if (OnCharacterDataChanged != null)
            OnCharacterDataChanged.Invoke();


        if (CharacterDataChangedFirstTime)
        {
            if (OnCharacterLoadedFirstTime != null)
                OnCharacterLoadedFirstTime.Invoke();

            //LastWorldPosition = new WorldPosition();
            //LastWorldPosition.locationId = CharacterData.position.locationId;
            //LastWorldPosition.zoneId = CharacterData.position.zoneId;
            //LastWorldPosition.pointOfInterestId = CharacterData.position.pointOfInterestId;

            OnWorldPositionChanged.Invoke();
        }

        if (LastWorldPosition != null)
        {
            if (LastWorldPosition.locationId != CharacterData.position.locationId || LastWorldPosition.zoneId != CharacterData.position.zoneId || LastWorldPosition.pointOfInterestId != CharacterData.position.pointOfInterestId)
                OnWorldPositionChanged.Invoke();
        }

        if (LastWorldPosition != null)
        {
            if (LastWorldPosition.locationId != CharacterData.position.locationId)
                OnWorldLocationChanged.Invoke();
        }

        LastWorldPosition = new WorldPosition();
        LastWorldPosition.locationId = CharacterData.position.locationId;
        LastWorldPosition.zoneId = CharacterData.position.zoneId;
        LastWorldPosition.pointOfInterestId = CharacterData.position.pointOfInterestId;

        CharacterDataChangedFirstTime = false;

    }

    private PlayerData oldDataPlayer = null;
    public void SetPlayerData(DocumentSnapshot _snapshot)
    {

        PlayerData = _snapshot.ConvertTo<PlayerData>();

        if (OnPlayerDataChanged_OldData != null)
            OnPlayerDataChanged_OldData.Invoke(oldDataPlayer);

        oldDataPlayer = _snapshot.ConvertTo<PlayerData>();

        if (OnPlayerDataChanged != null)
            OnPlayerDataChanged.Invoke();

        if (PlayerDataDataChangedFirstTime)
            if (OnPlayerDataLoadedFirstTime != null)
                OnPlayerDataLoadedFirstTime.Invoke();

        PlayerDataDataChangedFirstTime = false;


    }


    public void SetRareEncounterData(DocumentSnapshot _snapshot)
    {

        RareEncounterData = _snapshot.ConvertTo<EncounterData>();
        RareEncounterData.uid = _snapshot.Id;

        if (OnRareEncounterDataChanged != null)
            OnRareEncounterDataChanged.Invoke();
    }

    public void SetPartyData(DocumentSnapshot _snapshot)
    {

        if (_snapshot == null)
            PartyData = null;
        else
            PartyData = _snapshot.ConvertTo<Party>();



        if (OnPartyDataChanged != null)
            OnPartyDataChanged.Invoke();
    }

    public bool IsInParty()
    {
        if (PartyData == null) return false;
        else if (PartyData.partyMembers == null) return false;
        else return true;
        
    }


    //public void SetPartyFinderData(QuerySnapshot _snapshot)
    //{
    //    PartyFinderData.Clear();
    //    foreach (var doc in _snapshot)
    //    {
    //        var entry = doc.ConvertTo<PartyFinderData>();
    //        PartyFinderData.Add(entry);
    //    }



    //    if (OnPartyFinderDataChanged != null)
    //        OnPartyFinderDataChanged.Invoke();

    //}

    //public bool HasPersonalEncounterCreated()
    //{
    //    foreach (var item in EncountersData)
    //    {
    //        if (item.foundByCharacterUid == CharacterData.uid)
    //            return true;
    //    }
    //    return false;
    //}


    public void SetEncounterData(QuerySnapshot _snapshot)
    {
        List<EncounterData> FreshEncounterDataFromDB = new List<EncounterData>();


        //tady delam woodoo abych zaznamy co jsou uz v listu jen updatnul a tim zachvoval refernce na tyto zanznamy a ne ze je vsechny smazu a na hradim novyma....
        foreach (var newItem in _snapshot)
        {
            var freshEntry = newItem.ConvertTo<EncounterData>();

            //  freshEntry.FillSkillMetadata(SkillsMetadata);

            //  freshEntry.uid = newItem.Id;
            FreshEncounterDataFromDB.Add(freshEntry);

            bool itemFound = false;
            foreach (var origEntry in EncountersData) //projdu stare data v listu a pokud ma fresh data prekopiruju je tam
            {
                if (origEntry.uid == freshEntry.uid)
                {
                    Utils.CopyPropertiesTo(freshEntry, origEntry);
                    itemFound = true;
                }

            }

            if (!itemFound)   //pokud sem zazanm nenasel musi to byt novy encounter, pridam si ho
                EncountersData.Add(freshEntry);

        }

        for (int i = EncountersData.Count - 1; i >= 0; i--)  //a apk jeste projud ty co nejsou ve fresh, musi byt teda smazane, smazu je
        {
            bool itemFound = false;
            foreach (var newItem in FreshEncounterDataFromDB)
            {
                if (newItem.uid == EncountersData[i].uid)
                    itemFound = true;
            }
            if (!itemFound)
                EncountersData.RemoveAt(i);


        }


        if (EncountersDataChangedFirstTime)
            if (OnEncounterLoadedFirstTime != null)
                OnEncounterLoadedFirstTime.Invoke();


        if (OnEncounterDataChanged != null)
            OnEncounterDataChanged.Invoke();

        EncountersDataChangedFirstTime = false;


    }


    public void SetEncounterResultsData(QuerySnapshot _snapshot)
    {
        List<EncounterResult> FreshEncounterResultDataFromDB = new List<EncounterResult>();


        //tady delam woodoo abych zaznamy co jsou uz v listu jen updatnul a tim zachvoval refernce na tyto zanznamy a ne ze je vsechny smazu a na hradim novyma....
        foreach (var newItem in _snapshot)
        {
            var freshEntry = newItem.ConvertTo<EncounterResult>();

            FreshEncounterResultDataFromDB.Add(freshEntry);

            bool itemFound = false;
            foreach (var origEntry in EncounterResultsData) //projdu stare data v listu a pokud ma fresh data prekopiruju je tam
            {
                if (origEntry.uid == freshEntry.uid)
                {
                    Utils.CopyPropertiesTo(freshEntry, origEntry);
                    itemFound = true;
                }

            }

            if (!itemFound)   //pokud sem zazanm nenasel musi to byt novy encounter, pridam si ho
                EncounterResultsData.Add(freshEntry);

        }

        for (int i = EncounterResultsData.Count - 1; i >= 0; i--)  //a apk jeste projud ty co nejsou ve fresh, musi byt teda smazane, smazu je
        {
            bool itemFound = false;
            foreach (var newItem in FreshEncounterResultDataFromDB)
            {
                if (newItem.uid == EncounterResultsData[i].uid)
                    itemFound = true;
            }
            if (!itemFound)
                EncounterResultsData.RemoveAt(i);


        }


        if (OnEncounterResultsDataChanged != null)
            OnEncounterResultsDataChanged.Invoke();
    }

    public void SetVendorsData(QuerySnapshot _snapshot)
    {
        List<Vendor> FreshVendorDataFromDB = new List<Vendor>();


        //tady delam woodoo abych zaznamy co jsou uz v listu jen updatnul a tim zachvoval refernce na tyto zanznamy a ne ze je vsechny smazu a na hradim novyma....
        foreach (var newItem in _snapshot)
        {
            var freshEntry = newItem.ConvertTo<Vendor>();

            FreshVendorDataFromDB.Add(freshEntry);

            bool itemFound = false;
            foreach (var origEntry in VendorsData) //projdu stare data v listu a pokud ma fresh data prekopiruju je tam
            {
                if (origEntry.uid == freshEntry.uid)
                {
                    Utils.CopyPropertiesTo(freshEntry, origEntry);
                    itemFound = true;
                }

            }

            if (!itemFound)   //pokud sem zazanm nenasel musi to byt novy encounter, pridam si ho
                VendorsData.Add(freshEntry);

        }

        for (int i = VendorsData.Count - 1; i >= 0; i--)  //a apk jeste projud ty co nejsou ve fresh, musi byt teda smazane, smazu je
        {
            bool itemFound = false;
            foreach (var newItem in FreshVendorDataFromDB)
            {
                if (newItem.uid == VendorsData[i].uid)
                    itemFound = true;
            }
            if (!itemFound)
                VendorsData.RemoveAt(i);

        }

        if (OnVendorsDataChanged != null)
            OnVendorsDataChanged.Invoke();
    }




    public void SetPartyInviteData(DocumentSnapshot _snapshot)
    {
        if (_snapshot == null)
            PartyInviteData = null;
        else
            PartyInviteData = _snapshot.ConvertTo<PartyInvite>();


        if (OnPartyInviteDataChanged != null)
            OnPartyInviteDataChanged.Invoke();
    }



    public void SetInboxData(QuerySnapshot _snapshot)
    {
        InboxData.Clear();
        foreach (var doc in _snapshot)
        {

            var entry = doc.ConvertTo<InboxItem>();
            // entry.uid = doc.Id;
            InboxData.Add(entry);

        }



        if (OnInboxDataChanged != null)
            OnInboxDataChanged.Invoke();
    }



    public bool EncountersContainsEncounterCreatedByMe()
    {
        foreach (var encounter in EncountersData)
        {
            if (encounter.foundByCharacterUid == CharacterData.uid)
                return true;
        }
        return false;
    }

    public bool EncountersContainsGroupEncounter()
    {
        foreach (var encounter in EncountersData)
        {
            if (encounter.maxCombatants == 3)
                return true;
        }
        return false;
    }




}
