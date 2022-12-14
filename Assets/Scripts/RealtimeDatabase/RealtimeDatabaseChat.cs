using Firebase.Database;
using simplestmmorpg.realtimeDatabaseData;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RealtimeDatabaseChat : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public TMP_InputField ChatMessageInput;
    public UnityAction<RealtimeDatabaseChatMessageData> OnNewZoneChatMessageAdded;
    public UnityAction<RealtimeDatabaseChatMessageData> OnNewLocationChatMessageAdded;
    public UnityAction<RealtimeDatabaseChatMessageData> OnNewPartyMessageAdded;

    public UnityAction OnZoneChatChannelChanged;
    public UnityAction OnLocationChatChannelChanged;
    public UnityAction OnPartyChannelChanged;

    private DatabaseReference ZoneChatChannelRefrence;
    private DatabaseReference LocationChatChannelRefrence;
    private DatabaseReference PartyChatChannelReference;


    public const int MESSAGE_RETRIEVE_COUNT = 20;

    public CHANNEL_TYPE ActiveChannel = CHANNEL_TYPE.LOCATION;



    private void OnEnable()
    {
        ChatMessageInput.onTouchScreenKeyboardStatusChanged.AddListener(ProcessDonePressed);
    }

    private void OnDisable()
    {
        ChatMessageInput.onTouchScreenKeyboardStatusChanged.RemoveListener(ProcessDonePressed);
    }

    void ProcessDonePressed(TouchScreenKeyboard.Status newStatus)
    {
        if (newStatus == TouchScreenKeyboard.Status.Done)
        {
            Debug.Log("Done Pressed");
            SendMessage();
        }
    }

    public void SetLocationChatActive()
    {
        ActiveChannel = CHANNEL_TYPE.LOCATION;
    }

    public void SetPartyChatActive()
    {
        ActiveChannel = CHANNEL_TYPE.PARTY;
    }

    public void SetZoneChatActive()
    {
        ActiveChannel = CHANNEL_TYPE.ZONE;
    }

    private DatabaseReference GetMessageChannelOnCurrentLocation()
    {
        Debug.Log("REF:" + FirebaseDatabase.DefaultInstance.RootReference

        .Child(AccountDataSO.CharacterData.position.zoneId)
        .Child(AccountDataSO.CharacterData.position.locationId)
        .Child("messages").Reference);

        return FirebaseDatabase.DefaultInstance.RootReference
           .Child(AccountDataSO.CharacterData.position.zoneId)
           .Child(AccountDataSO.CharacterData.position.locationId)
           .Child("messages");


    }

    private DatabaseReference GetMessageChannelOnCurrentZone()
    {
        return FirebaseDatabase.DefaultInstance.RootReference
           .Child(AccountDataSO.CharacterData.position.zoneId)
           .Child("messages");

    }


    private DatabaseReference GetPartyChannel()
    {
        //return FirebaseDatabase.DefaultInstance.RootReference
        //   .Child(AccountDataSO.CharacterData.position.zoneId)
        //   .Child("messages");

        return FirebaseDatabase.DefaultInstance.RootReference
        .Child("chatChannels")
        .Child("partyChannles")
        .Child(AccountDataSO.PartyData.uid)
        .Child("messages");

    }


    public void Awake()
    {
        AccountDataSO.OnCharacterLoadedFirstTime += OnCharacterWorldLocationChanged;
        AccountDataSO.OnWorldLocationChanged += OnCharacterWorldLocationChanged;
        AccountDataSO.OnPartyDataChanged += OnPartyDataChanged;
    }

    private Query lastQuerryParty = null;
    private void OnPartyDataChanged()
    {
        if (AccountDataSO.PartyData == null)
            return;


        //pokud charakter zmenil lokaci, zmenime chat channel
        DatabaseReference newPartyChatChannelReference = GetPartyChannel();

        if (PartyChatChannelReference == null)  //prvni spusteni zadny chat neni jeste, proto nejde porovnat rozdily, musim projit a zacit poslouchat na chatu
        {
            PartyChatChannelReference = FirebaseDatabase.DefaultInstance.RootReference;//refrerence dorite nekam aby ten if prosel a ty dva se sobe nerovnali....hnus ja vim...
            newPartyChatChannelReference = GetPartyChannel();
        }


        if (newPartyChatChannelReference.Key != PartyChatChannelReference.Key)
        {
            if (OnPartyChannelChanged != null)
                OnPartyChannelChanged.Invoke();


            //if (PartyChatChannelReference != FirebaseDatabase.DefaultInstance.RootReference)
            //    PartyChatChannelReference.ChildAdded -= OnPartyChatChannel_ChildAdded;

            //PartyChatChannelReference = newPartyChatChannelReference;
            //PartyChatChannelReference.ChildAdded += OnPartyChatChannel_ChildAdded;



            if (lastQuerryParty != null)
                lastQuerryParty.ChildAdded -= OnPartyChatChannel_ChildAdded;

            PartyChatChannelReference = newPartyChatChannelReference;
            lastQuerryParty = PartyChatChannelReference.LimitToLast(MESSAGE_RETRIEVE_COUNT);
            lastQuerryParty.ChildAdded += OnPartyChatChannel_ChildAdded;

        }




        //if (newPartyChatChannelReference != PartyChatChannelReference)
        //{
        //    if (OnPartyChannelChanged != null)
        //        OnPartyChannelChanged.Invoke();

        //    if (PartyChatChannelReference != null)
        //        PartyChatChannelReference.ChildAdded -= OnPartyChatChannel_ChildChanged;

        //    PartyChatChannelReference = newPartyChatChannelReference;
        //    PartyChatChannelReference.ChildAdded += OnPartyChatChannel_ChildChanged;

        //}

    }

    private Query lastQuerryLocation = null; //protoze abych se mohl odsuscribnout listenera musism si drzet referenci, nefacah si vytvorit znovu querry (LimitToLast) vytvori to asi novou referenci
    private Query lastQuerryZone = null;
    private void OnCharacterWorldLocationChanged()
    {
//        Debug.Log("-------------LOKACE ZMENENA START!");
        //pokud charakter zmenil lokaci, zmenime chat channel
        DatabaseReference newLocationMessageChannelReference = GetMessageChannelOnCurrentLocation();

        if (LocationChatChannelRefrence == null)  //prvni spusteni zadny chat neni jeste, proto nejde porovnat rozdily, musim projit a zacit poslouchat na chatu
        {
            LocationChatChannelRefrence = FirebaseDatabase.DefaultInstance.RootReference;//refrerence dorite nekam aby ten if prosel a ty dva se sobe nerovnali....hnus ja vim...
            newLocationMessageChannelReference = GetMessageChannelOnCurrentLocation();
        }

      //  Debug.Log("LocationChatChannelRefrence: " + LocationChatChannelRefrence.Reference);
     //   Debug.Log("newLocationMessageChannelReference: " + newLocationMessageChannelReference.Reference);

        if (newLocationMessageChannelReference.Reference != LocationChatChannelRefrence.Reference)
        {
      //      Debug.Log("-------------LOKACE ZMENENA OPRaVDU SEM SEM JINAM MMENIM CHAT!");
            if (OnLocationChatChannelChanged != null)
                OnLocationChatChannelChanged.Invoke();

            if (lastQuerryLocation != null)
                lastQuerryLocation.ChildAdded -= LocationPositionChatChannel_ChildAdded;

            LocationChatChannelRefrence = newLocationMessageChannelReference;
            lastQuerryLocation = LocationChatChannelRefrence.LimitToLast(MESSAGE_RETRIEVE_COUNT);
            lastQuerryLocation.ChildAdded += LocationPositionChatChannel_ChildAdded;



        }


        DatabaseReference newZoneMessageChannelReference = GetMessageChannelOnCurrentZone();

        if (ZoneChatChannelRefrence == null)  //prvni spusteni zadny chat neni jeste, proto nejde porovnat rozdily, musim projit a zacit poslouchat na chatu
        {
            ZoneChatChannelRefrence = FirebaseDatabase.DefaultInstance.RootReference; //refrerence dorite nekam aby ten if prosel a ty dva se sobe nerovnali....hnus ja vim... 
            newZoneMessageChannelReference = GetMessageChannelOnCurrentZone();
        }

        if (newZoneMessageChannelReference.Key != ZoneChatChannelRefrence.Key)
        {
            if (OnZoneChatChannelChanged != null)
                OnZoneChatChannelChanged.Invoke();


            //if (ZoneChatChannelRefrence != FirebaseDatabase.DefaultInstance.RootReference)
            //    ZoneChatChannelRefrence.ChildAdded -= ZonePositionChatChannel_ChildAdded;


            //ZoneChatChannelRefrence = newZoneMessageChannelReference;
            //ZoneChatChannelRefrence.ChildAdded += ZonePositionChatChannel_ChildAdded;


            if (lastQuerryZone != null)
                lastQuerryZone.ChildAdded -= ZonePositionChatChannel_ChildAdded;

            ZoneChatChannelRefrence = newZoneMessageChannelReference;
            lastQuerryZone = ZoneChatChannelRefrence.LimitToLast(MESSAGE_RETRIEVE_COUNT);
            lastQuerryZone.ChildAdded += ZonePositionChatChannel_ChildAdded;

        }
    }

    // Start is called before the first frame update
    //void Start()
    //{
    //    messageChannelReference = GetMessageChannelOnCurrentLocation();
    //    messageChannelReference.ChildAdded += Reference_ChildChanged;
    //}

    private RealtimeDatabaseChatMessageData ParseChatMessage(ChildChangedEventArgs e)
    {
        RealtimeDatabaseChatMessageData recievedChatMessage = new RealtimeDatabaseChatMessageData();
        recievedChatMessage.body = e.Snapshot.Child("body").Value.ToString();

        try
        {
            recievedChatMessage.characterLevel = int.Parse(e.Snapshot.Child("characterLevel").Value.ToString());

        }
        catch (System.Exception ex)
        {
            Debug.LogError("Chyba pri parsovani toho debiliho chatu : " + e.Snapshot.Child("characterLevel").Exists);
            Debug.LogError("Body : " + e.Snapshot.Child("body").Value.ToString());
        }
        recievedChatMessage.characterName = e.Snapshot.Child("characterName").Value.ToString();
        recievedChatMessage.characterUid = e.Snapshot.Child("characterUid").Value.ToString();
        recievedChatMessage.channelName = e.Snapshot.Child("channelName").Value.ToString();
        recievedChatMessage.channelType = (CHANNEL_TYPE)int.Parse(e.Snapshot.Child("channelType").Value.ToString());

        return recievedChatMessage;
    }

    private void OnPartyChatChannel_ChildAdded(object sender, ChildChangedEventArgs e)
    {
        RealtimeDatabaseChatMessageData recievedChatMessage = ParseChatMessage(e);
        //Debug.Log("Nova chat message: " + e.Snapshot.Child("body").Value.ToString());
        //RealtimeDatabaseChatMessageData recievedChatMessage = new RealtimeDatabaseChatMessageData();
        //recievedChatMessage.body = e.Snapshot.Child("body").Value.ToString();
        //recievedChatMessage.characterName = e.Snapshot.Child("characterName").Value.ToString();
        //recievedChatMessage.characterUid = e.Snapshot.Child("characterUid").Value.ToString();
        //recievedChatMessage.channelName = e.Snapshot.Child("channelName").Value.ToString();
        //recievedChatMessage.channelType = (CHANNEL_TYPE)int.Parse(e.Snapshot.Child("channelType").Value.ToString());

        if (OnNewPartyMessageAdded != null)
            OnNewPartyMessageAdded.Invoke(recievedChatMessage);
        // ChatText.SetText(ChatText.text + "\n[" + e.Snapshot.Child("characterName").Value.ToString() + "] " + e.Snapshot.Child("body").Value.ToString());
    }



    private void LocationPositionChatChannel_ChildAdded(object sender, ChildChangedEventArgs e)
    {
        //        Debug.Log("-FUK-");
        RealtimeDatabaseChatMessageData recievedChatMessage = ParseChatMessage(e);
        //RealtimeDatabaseChatMessageData recievedChatMessage = new RealtimeDatabaseChatMessageData();
        //recievedChatMessage.body = e.Snapshot.Child("body").Value.ToString();
        //recievedChatMessage.characterName = e.Snapshot.Child("characterName").Value.ToString();
        //recievedChatMessage.characterUid = e.Snapshot.Child("characterUid").Value.ToString();
        //recievedChatMessage.channelName = e.Snapshot.Child("channelName").Value.ToString();
        //recievedChatMessage.channelType = (CHANNEL_TYPE)int.Parse(e.Snapshot.Child("channelType").Value.ToString());


        if (OnNewLocationChatMessageAdded != null)
            OnNewLocationChatMessageAdded.Invoke(recievedChatMessage);
    }

    private void ZonePositionChatChannel_ChildAdded(object sender, ChildChangedEventArgs e)
    {
        RealtimeDatabaseChatMessageData recievedChatMessage = ParseChatMessage(e);//new RealtimeDatabaseChatMessageData();
        //recievedChatMessage.body = e.Snapshot.Child("body").Value.ToString();
        //recievedChatMessage.characterName = e.Snapshot.Child("characterName").Value.ToString();
        //recievedChatMessage.characterUid = e.Snapshot.Child("characterUid").Value.ToString();
        //recievedChatMessage.channelName = e.Snapshot.Child("channelName").Value.ToString();
        //recievedChatMessage.channelType = (CHANNEL_TYPE)int.Parse(e.Snapshot.Child("channelType").Value.ToString());


        if (OnNewZoneChatMessageAdded != null)
            OnNewZoneChatMessageAdded.Invoke(recievedChatMessage);
    }

    public void SendMessage()
    {
        if (ChatMessageInput.text == "")
            return;

        if (ActiveChannel == CHANNEL_TYPE.LOCATION)
        {

            RealtimeDatabaseChatMessageData user = new RealtimeDatabaseChatMessageData();
            user.characterUid = AccountDataSO.CharacterData.uid;
            user.characterName = AccountDataSO.CharacterData.characterName;
            user.characterLevel = AccountDataSO.CharacterData.stats.level;

            user.body = ChatMessageInput.text;
            user.channelName = AccountDataSO.CharacterData.position.locationId;
            user.channelType = CHANNEL_TYPE.LOCATION;
            string json = JsonUtility.ToJson(user);

            //   GetMessageChannelOnCurrentLocation()
            LocationChatChannelRefrence
                     .Push()
                  .SetRawJsonValueAsync(json).ContinueWith(task =>
              {
                  if (task.IsCompleted) Debug.Log("Success!!"); else Debug.Log("Fail!");

              });

            //LocationChatChannelRefrence.SetValueAsync()
            //LocationChatChannelRefrence.LimitToFirst(1).GetValueAsync().ContinueWith(task=>
            //{
            //    task.Result..RemoveValueAsync();
            //});


        }

        else if (ActiveChannel == CHANNEL_TYPE.PARTY)
        {
            RealtimeDatabaseChatMessageData user = new RealtimeDatabaseChatMessageData();
            user.characterUid = AccountDataSO.CharacterData.uid;
            user.characterName = AccountDataSO.CharacterData.characterName;
            user.characterLevel = AccountDataSO.CharacterData.stats.level;
            user.body = ChatMessageInput.text;
            user.channelName = "Party";
            user.channelType = CHANNEL_TYPE.PARTY;
            string json = JsonUtility.ToJson(user);

            PartyChatChannelReference
                .Push()
                .SetRawJsonValueAsync(json).ContinueWith(task =>
                {
                    if (task.IsCompleted) Debug.Log("Success!!"); else Debug.Log("Fail!");

                });
        }
        else if (ActiveChannel == CHANNEL_TYPE.ZONE)
        {
            RealtimeDatabaseChatMessageData user = new RealtimeDatabaseChatMessageData();
            user.characterUid = AccountDataSO.CharacterData.uid;
            user.characterName = AccountDataSO.CharacterData.characterName;
            user.characterLevel = AccountDataSO.CharacterData.stats.level;
            user.body = ChatMessageInput.text;
            user.channelName = AccountDataSO.CharacterData.position.zoneId;
            user.channelType = CHANNEL_TYPE.ZONE;
            string json = JsonUtility.ToJson(user);

            //   GetMessageChannelOnCurrentLocation()
            ZoneChatChannelRefrence
                     .Push()
                  .SetRawJsonValueAsync(json).ContinueWith(task =>
                  {
                      if (task.IsCompleted) Debug.Log("Success!!"); else Debug.Log("Fail!");

                  });
        }

        ChatMessageInput.text = "";

        //public void ReadData()
        //{
        //    reference.Child("User").Child(NameToReadInput.text).GetValueAsync().ContinueWith(task =>
        //    {
        //        if (task.IsCompleted)
        //        {
        //            Debug.Log("Success!!");
        //            DataSnapshot snapshot = task.Result;
        //            Debug.Log(snapshot.Child("UserName").Value.ToString());
        //            Debug.Log(snapshot.Child("Email").Value.ToString());
        //        }
        //        else Debug.Log("Fail!");
        //    });
        //}
    }
}
