
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using Firebase.Functions;
using Newtonsoft.Json;
using simplestmmorpg.data;
using simplestmmorpg.playerData;
using TMPro;

using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu]
public class FirebaseCloudFunctionSO : ScriptableObject
{
    public AccountDataSO AccountDataSO;

    FirebaseFunctions functions;

    public UnityAction OnCloudFunctionInProgress;
    public UnityAction OnCloudFunctionFinished;

    public bool UseLocalHost = false;


    public void OnEnable()
    {
        functions = FirebaseFunctions.DefaultInstance;

        if (UseLocalHost)
            functions.UseFunctionsEmulator("localhost:5001");

    }


    private void CloudFunctionCalled()
    {
        //  Debug.Log("showing");
        OnCloudFunctionInProgress.Invoke();
    }

    private void CloudFunctionFinished(string _result)
    {
        //    Debug.Log("hiding");

        if (_result.StartsWith("ERROR"))
        {
            Debug.Log(_result);
            UIManager.instance.SpawnErrorText(_result);
        }

        OnCloudFunctionFinished.Invoke();
    }

    private void CloudFunctionFinished()
    {
        OnCloudFunctionFinished.Invoke();
    }

    private async Task CallCloudFunction(string _cloudFunctionName, Dictionary<string, object> _data)
    {
        string result = await FirebaseFunctions.DefaultInstance.GetHttpsCallable(_cloudFunctionName).CallAsync(_data).ContinueWith(OnCloudFuntionResult);
        CloudFunctionFinished(result);
    }

    public async void EnterDungeon(string _dungeonLocationId)
    {

        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["callerCharacterUid"] = AccountDataSO.CharacterData.uid;
        data["dungeonLocationId"] = _dungeonLocationId;

        Debug.Log("entering dungeon ... ");
        await CallCloudFunction("party-enterDungeon", data);
    }

    public async void PutContentOnAuctionHouse(string _contentType, string _contentToSellUid, int _contentSilverAmount, int _buyoutPrice, int _bidPrice)
    {
        CloudFunctionCalled();

        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["contentType"] = _contentType;
        data["contentToSell"] = _contentToSellUid;
        data["contentSilverAmount"] = _contentSilverAmount;
        data["buyoutPrice"] = _buyoutPrice;
        data["bidPrice"] = _bidPrice;

        Debug.Log("putting item on auction ... ");

        await CallCloudFunction("auction-putContentOnAuctionHouse", data);
        // CloudFunctionFinished(await FirebaseFunctions.DefaultInstance.GetHttpsCallable("auction-putContentOnAuctionHouse").CallAsync(data).ContinueWith(OnCloudFuntionResult));
        //CloudFunctionFinished();
    }

    public async void ClaimQuestgiverReward(string _questgiverUid)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["questgiverUid"] = _questgiverUid;

        Debug.Log("claiming questgiver  ... ");

        await CallCloudFunction("questgiver-claimQuestgiverReward", data);
        //await FirebaseFunctions.DefaultInstance.GetHttpsCallable("questgiver-claimQuestgiverReward").CallAsync(data).ContinueWith(OnCloudFuntionResult);
        //CloudFunctionFinished();
    }

    public async void ClaimInboxItem(string _inboxItemUid)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["inboxItemUid"] = _inboxItemUid;


        Debug.Log("claiming inbox item  ... ");
        await CallCloudFunction("inbox-claimInboxItem", data);
        //await FirebaseFunctions.DefaultInstance.GetHttpsCallable("inbox-claimInboxItem").CallAsync(data).ContinueWith(OnCloudFuntionResult);
        //CloudFunctionFinished();
    }

    public async void WorldMapTravel(string _locationId)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["locationId"] = _locationId;


        Debug.Log("traveling to location on world map  ... ");
        await CallCloudFunction("worldMap-travel", data);
        //await FirebaseFunctions.DefaultInstance.GetHttpsCallable("worldMap-travel").CallAsync(data).ContinueWith(OnCloudFuntionResult);
        //CloudFunctionFinished();
    }

    public async void PointOfInterestTravel(string _destinationPointOfInterestId)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["destinationPointOfInterestId"] = _destinationPointOfInterestId;


        Debug.Log("traveling to point of interest in location ... ");
        await CallCloudFunction("worldMap-travelToPoI", data);
        //await FirebaseFunctions.DefaultInstance.GetHttpsCallable("worldMap-travel").CallAsync(data).ContinueWith(OnCloudFuntionResult);
        //CloudFunctionFinished();
    }


    public async void ClaimTimePool()
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;

        Debug.Log("claiming time pool ... ");

        await CallCloudFunction("claimTimePool", data);
        //await FirebaseFunctions.DefaultInstance.GetHttpsCallable("claimTimePool").CallAsync(data).ContinueWith(OnCloudFuntionResult);
        //CloudFunctionFinished();
    }

    public async void EatFood(string _foodUid)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["consumanleUid"] = _foodUid;

        Debug.Log("eating food pool ... ");

        await CallCloudFunction("eatFood", data);
        //await FirebaseFunctions.DefaultInstance.GetHttpsCallable("eatFood").CallAsync(data).ContinueWith(OnCloudFuntionResult);
        //  CloudFunctionFinished();
    }



    public async void BidContentOnAuctionHouse(string _offerUid, int _bidAmount)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["offerUid"] = _offerUid;
        data["bidAmount"] = _bidAmount;

        Debug.Log("bidding on item on auction ... ");
        await CallCloudFunction("auction-bidContentOnAuctionHouse", data);
        // CloudFunctionFinished(await FirebaseFunctions.DefaultInstance.GetHttpsCallable("auction-bidContentOnAuctionHouse").CallAsync(data).ContinueWith(OnCloudFuntionResult));
    }

    public async void BuyoutContentOnAuctionHouse(string _offerUid)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["offerUid"] = _offerUid;

        Debug.Log("buyouting item from auction ... ");

        await CallCloudFunction("auction-buyoutContentOnAuctionHouse", data);

        // CloudFunctionFinished(await FirebaseFunctions.DefaultInstance.GetHttpsCallable("auction-buyoutContentOnAuctionHouse").CallAsync(data).ContinueWith(OnCloudFuntionResult));
    }

    public async void CollectGoldForMySoldContentOnAuctionHouse(string _offerUid)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["offerUid"] = _offerUid;

        Debug.Log("collecting gold for sold  auction ... ");

        await CallCloudFunction("auction-collectGoldForMySoldContentOnAuctionHouse", data);
        //await FirebaseFunctions.DefaultInstance.GetHttpsCallable("auction-collectGoldForMySoldContentOnAuctionHouse").CallAsync(data).ContinueWith(OnCloudFuntionResult);
        //CloudFunctionFinished();
    }

    public async void CollectContentForMyWonAuctionOnAuctionHouse(string _offerUid)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["offerUid"] = _offerUid;

        Debug.Log("collecting item for won  auction ... ");

        await CallCloudFunction("auction-collectContentForMyWonAuctionOnAuctionHouse", data);
        //await FirebaseFunctions.DefaultInstance.GetHttpsCallable("auction-collectContentForMyWonAuctionOnAuctionHouse").CallAsync(data).ContinueWith(OnCloudFuntionResult);
        //CloudFunctionFinished();
    }

    public async void CollectMyUnsoldContentOnAuctionHouse(string _offerUid)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["offerUid"] = _offerUid;

        Debug.Log("collecting content of unsold auction  ... ");

        await CallCloudFunction("auction-collectMyUnsoldContentOnAuctionHouse", data);
        //await FirebaseFunctions.DefaultInstance.GetHttpsCallable("auction-collectMyUnsoldContentOnAuctionHouse").CallAsync(data).ContinueWith(OnCloudFuntionResult);
        //CloudFunctionFinished();
    }


    public async void SellInventoryItems(List<string> _itemsTSellUids)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["inventoryItemsToSellEquipUids"] = _itemsTSellUids.ToArray();

        Debug.Log("selling bag items ... ");

        await CallCloudFunction("general2-sellInventoryItems", data);
        //await FirebaseFunctions.DefaultInstance.GetHttpsCallable("general2-sellInventoryItems").CallAsync(data).ContinueWith(OnCloudFuntionResult);

        //CloudFunctionFinished();

    }


    public async void BuyVendorItems(List<string> _itemToBuyUids, string _vendorUid)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["vendorUid"] = _vendorUid;
        data["vendorItemsToBuyUids"] = _itemToBuyUids.ToArray();

        Debug.Log("buying items from vendor ... ");

        await CallCloudFunction("vendor-buyVendorItems", data);
        //await FirebaseFunctions.DefaultInstance.GetHttpsCallable("vendor-buyVendorItems").CallAsync(data).ContinueWith(OnCloudFuntionResult);

        //CloudFunctionFinished();

    }


    public async void CreateCharacter(string _characterName, string _characterClass)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["playerUid"] = AccountDataSO.PlayerData.uid;
        data["characterName"] = _characterName;
        data["characterClass"] = _characterClass;

        Debug.Log("Createing Character ... ");

        await CallCloudFunction("createCharacter", data);
        //await FirebaseFunctions.DefaultInstance.GetHttpsCallable("createCharacter").CallAsync(data).ContinueWith(OnCloudFuntionResult);
        //CloudFunctionFinished();
    }


    public async void DeleteCharacter(string _characterToDeleteUid)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterToDeleteUid"] = _characterToDeleteUid;

        Debug.Log("Deleting Character ... ");

        await CallCloudFunction("deleteCharacter", data);
    }

    public async void ChangeEquip(List<string> _equipUids)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["equitToEquip"] = _equipUids.ToArray();

        Debug.Log("Equiping equipment ... ");
        await CallCloudFunction("equip-changeCharacterEquip", data);
        //await FirebaseFunctions.DefaultInstance.GetHttpsCallable("equip-changeCharacterEquip").CallAsync(data).ContinueWith(OnCloudFuntionResult);
        //CloudFunctionFinished();
    }

    public async void SelectWantItemInEncounterResult(string _encounterResultUid, string _wantedItemId)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["encounterResultUid"] = _encounterResultUid;
        data["wantedItemId"] = _wantedItemId;


        Debug.Log("Choosing wanted item ... ");
        await CallCloudFunction("encounterResult-selectWantItemInEncounterResult", data);
        //await FirebaseFunctions.DefaultInstance.GetHttpsCallable("encounterResult-selectWantItemInEncounterResult").CallAsync(data).ContinueWith(OnCloudFuntionResult);

        //CloudFunctionFinished();

    }


    public async void ForceEndWantItemPhase(string _encounterResultUid)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        //    data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["encounterResultUid"] = _encounterResultUid;
        //   data["wantedItemId"] = _wantedItemId;


        Debug.Log("Choosing wanted item ... ");
        await CallCloudFunction("encounterResult-forceEndWantItemPhase", data);


    }

    //public void FightEncounter(string _encounterId)
    //{
    //    CloudFunctionCalled();
    //    var data = new Dictionary<string, string>();
    //    data["characterUid"] = AccountDataSO.CharacterData.uid;
    //    data["encounterUid"] = _encounterId;




    //    Debug.Log("started ");

    //    FirebaseFunctions.DefaultInstance.GetHttpsCallable("encounter-fightEncounter").CallAsync(data).ContinueWith((task) =>
    //    {
    //        if (task.IsFaulted)
    //        {
    //            Debug.Log("fault");
    //            foreach (var inner in task.Exception.InnerExceptions)
    //            {
    //                if (inner is FunctionsException)
    //                {
    //                    var e = (FunctionsException)inner;
    //                    // Function error code, will be INTERNAL if the failure
    //                    // was not handled properly in the function call.
    //                    var code = e.ErrorCode;
    //                    Debug.Log(code);
    //                }
    //            }
    //        }
    //        else
    //        {
    //            //string result = task.Result.ToString();
    //            //  Debug.Log("RESULT:" + result);
    //        }

    //        Debug.Log("Fight Encounter RESULT: " + JsonConvert.SerializeObject(task.Result.Data, Formatting.Indented));

    //        return (string)task.Result.Data;
    //    });
    //}

    public async void ExplorePointOfInterest(PointOfInterestIdDefinition _pointOfInterest)
    {

        CloudFunctionCalled();
        Debug.Log("Creating your Encouter");
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        //    data["zoneId"] = AccountDataSO.CharacterData.position.zoneId;//_position.zoneId;
        //  data["locationId"] = AccountDataSO.CharacterData.position.locationId;//_position.locationId;
        data["pointOfInterestId"] = _pointOfInterest.Id;//_position.locationId;

        Debug.Log("CreateEncounter started ");
        await CallCloudFunction("encounter-explorePointOfInterest", data);

        //await FirebaseFunctions.DefaultInstance.GetHttpsCallable("encounter-createEncounter").CallAsync(data).ContinueWith(OnCloudFuntionResult);

        //CloudFunctionFinished();
        //FirebaseFunctions.DefaultInstance.GetHttpsCallable("encounter-createEncounter").CallAsync(data).ContinueWith((task) =>
        //{
        //    if (task.IsFaulted)
        //    {
        //        Debug.Log("fault");
        //        foreach (var inner in task.Exception.InnerExceptions)
        //        {
        //            if (inner is FunctionsException)
        //            {
        //                var e = (FunctionsException)inner;
        //                // Function error code, will be INTERNAL if the failure
        //                // was not handled properly in the function call.
        //                var code = e.ErrorCode;
        //                Debug.Log(code);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        //string result = task.Result.ToString();
        //        //  Debug.Log("RESULT:" + result);
        //    }

        //    Debug.Log("CreateEncounter : RESULT: " + JsonConvert.SerializeObject(task.Result.Data, Formatting.Indented));

        //    return (string)task.Result.Data;
        //});
    }

    public async void JoinEncounter(string _encounterId)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["encounterUid"] = _encounterId;

        Debug.Log("Joining encounter... ");

        //await FirebaseFunctions.DefaultInstance.GetHttpsCallable("encounter-joinEncounter").CallAsync(data).ContinueWith(OnCloudFuntionResult);

        //CloudFunctionFinished();

        Debug.Log("CreateEncounter started ");
        await CallCloudFunction("encounter-joinEncounter", data);

    }

    public async void FleeFromEncounter(string _encounterId)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["encounterUid"] = _encounterId;

        Debug.Log("Fleeing encounter... ");

        await CallCloudFunction("encounter-fleeFromEncounter", data);

        //await FirebaseFunctions.DefaultInstance.GetHttpsCallable("encounter-fleeFromEncounter").CallAsync(data).ContinueWith(OnCloudFuntionResult);

        //CloudFunctionFinished();

    }

    public async void AddSelfAsWatcher(string _encounterId)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["encounterUid"] = _encounterId;

        Debug.Log("Starting to watch at encounter... ");

        await CallCloudFunction("encounter-addSelfAsWatcher", data);

        //await FirebaseFunctions.DefaultInstance.GetHttpsCallable("encounter-addSelfAsWatcher").CallAsync(data).ContinueWith(OnCloudFuntionResult);

        //CloudFunctionFinished();
    }

    public async void CollectEncounterResultReward(string _encounterResultUid)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["encounterResultUid"] = _encounterResultUid;

        Debug.Log("Collecting encounter result Reward.. ");
        await CallCloudFunction("encounterResult-collectEncounterResultReward", data);

        //await FirebaseFunctions.DefaultInstance.GetHttpsCallable("encounterResult-collectEncounterResultReward").CallAsync(data).ContinueWith(OnCloudFuntionResult);

        //CloudFunctionFinished();
    }



    async public void ApplySkillOnEncounter(string _encounterId, int _skillSlotId, string _targetUid)
    {

        string task = await ApplySkillOnEncounterTask(_encounterId, _skillSlotId, _targetUid);

        CloudFunctionFinished(task);

        //  CombatLog combatLog = JsonConvert.DeserializeObject<CombatLog>(task);
        //        Debug.Log("Result Cloud call:X " + combatLog.entries[0]);
    }

    public Task<string> ApplySkillOnEncounterTask(string _encounterId, int _skillSlotId, string _targetUid)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["encounterUid"] = _encounterId;
        data["skillSlotId"] = _skillSlotId;
        data["targetUid"] = _targetUid;



        Debug.Log("applying skill to  encounter... ");

        return FirebaseFunctions.DefaultInstance.GetHttpsCallable("encounter-applySkillOnEncounter").CallAsync(data).ContinueWith((_task) =>
        {

            Debug.Log("sem tu");
            if (_task.IsFaulted)
            {
                string resultError = "ERROR " + _task.Exception.InnerException.Message;
                return resultError;

            }
            string json = JsonConvert.SerializeObject(_task.Result.Data, Formatting.Indented);
            return json;
        });



    }

    public async void ForceRestEncounter(string _encounterId, string _forceRestOnThisCharacterUid)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        //  data["userUid"] = AccountDataSO.CharacterData.userUid;
        data["forceRestOnThisCharacterUid"] = _forceRestOnThisCharacterUid;
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["encounterUid"] = _encounterId;


        Debug.Log("Resting for others... ");

        await CallCloudFunction("encounter-forceRestEncounter", data);
    }


    public async void RestEncounter(string _encounterId)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        //   data["userUid"] = AccountDataSO.CharacterData.userUid;
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["encounterUid"] = _encounterId;


        Debug.Log("Resting ... ");

        await CallCloudFunction("encounter-restEncounter", data);
    }




    public async void LeaveParty()
    {
        Debug.Log("LeaveParty");
        CloudFunctionCalled();

        var data = new Dictionary<string, object>();
        data["callerCharacterUid"] = AccountDataSO.CharacterData.uid;
        await CallCloudFunction("party-leaveParty", data);

    }


    public async void SendPartyInvite(string _invitedCharacterUid)
    {


        Debug.Log("Send Party Invite");
        CloudFunctionCalled();

        var data = new Dictionary<string, object>();
        data["callerCharacterUid"] = AccountDataSO.CharacterData.uid;
        data["callerCharacterName"] = AccountDataSO.CharacterData.characterName;
        data["invitedCharacterUid"] = _invitedCharacterUid;


        await CallCloudFunction("party-sendPartyInvite", data);

    }

    public async void AcceptPartyInvite(string _partyLeaderUid)
    {


        Debug.Log("Accept Party Invite");
        CloudFunctionCalled();

        var data = new Dictionary<string, object>();
        data["callerCharacterUid"] = AccountDataSO.CharacterData.uid;
        data["partyLeaderUid"] = _partyLeaderUid;


        await CallCloudFunction("party-acceptPartyInvite", data);

    }


    public async void DeclinePartyInvite(string _partyLeaderUid)
    {
        Debug.Log("Decline Party Invite");
        CloudFunctionCalled();

        var data = new Dictionary<string, object>();
        data["partyLeaderUid"] = _partyLeaderUid;


        await CallCloudFunction("party-declinePartyInvite", data);

    }










    public void CreateMiningEncounter()
    {
        CloudFunctionCalled();
        Debug.Log("Creating your Mining Encouter");
        var data = new Dictionary<string, string>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        //data["encounterUid"] = _encounterId;

        Debug.Log("started ");

        FirebaseFunctions.DefaultInstance.GetHttpsCallable("general2-createMiningEncounter").CallAsync(data).ContinueWith((task) =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("fault");
                foreach (var inner in task.Exception.InnerExceptions)
                {
                    if (inner is FunctionsException)
                    {
                        var e = (FunctionsException)inner;
                        // Function error code, will be INTERNAL if the failure
                        // was not handled properly in the function call.
                        var code = e.ErrorCode;
                        Debug.Log(code);
                    }
                }
            }
            else
            {
                //string result = task.Result.ToString();
                //  Debug.Log("RESULT:" + result);
            }

            Debug.Log("CreateEncounter : RESULT: " + JsonConvert.SerializeObject(task.Result.Data, Formatting.Indented));
            CloudFunctionFinished();
            return (string)task.Result.Data;
        });
    }


    public void CreateParty()
    {
        CloudFunctionCalled();
        Debug.Log("Creating party");
        var data = new Dictionary<string, string>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        //data["encounterUid"] = _encounterId;

        Debug.Log("started ");

        FirebaseFunctions.DefaultInstance.GetHttpsCallable("general2-createParty").CallAsync(data).ContinueWith((task) =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("fault");
                foreach (var inner in task.Exception.InnerExceptions)
                {
                    if (inner is FunctionsException)
                    {
                        var e = (FunctionsException)inner;
                        // Function error code, will be INTERNAL if the failure
                        // was not handled properly in the function call.
                        var code = e.ErrorCode;
                        Debug.Log(code);
                    }
                }
            }
            else
            {
                //string result = task.Result.ToString();
                //  Debug.Log("RESULT:" + result);
            }
            CloudFunctionFinished();
            Debug.Log("PartyResult : RESULT: " + JsonConvert.SerializeObject(task.Result.Data, Formatting.Indented));

            return (string)task.Result.Data;
        });
    }

    public void JoinParty(string _partyUid)
    {
        CloudFunctionCalled();
        Debug.Log("Joining party");
        var data = new Dictionary<string, string>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["partyUid"] = _partyUid;
        //data["encounterUid"] = _encounterId;

        Debug.Log("started ");

        FirebaseFunctions.DefaultInstance.GetHttpsCallable("general2-joinParty").CallAsync(data).ContinueWith((task) =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("fault");
                foreach (var inner in task.Exception.InnerExceptions)
                {
                    if (inner is FunctionsException)
                    {
                        var e = (FunctionsException)inner;
                        // Function error code, will be INTERNAL if the failure
                        // was not handled properly in the function call.
                        var code = e.ErrorCode;
                        Debug.Log(code);
                    }
                }
            }
            else
            {
                //string result = task.Result.ToString();
                //  Debug.Log("RESULT:" + result);
            }

            CloudFunctionFinished();
            Debug.Log("JoinPartyResult : RESULT: " + JsonConvert.SerializeObject(task.Result.Data, Formatting.Indented));

            return (string)task.Result.Data;
        });
    }

    public void AutoJoinParty()
    {
        CloudFunctionCalled();
        Debug.Log("Joining party");
        var data = new Dictionary<string, string>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;

        //data["encounterUid"] = _encounterId;

        Debug.Log("started ");

        FirebaseFunctions.DefaultInstance.GetHttpsCallable("general2-autoJoinParty").CallAsync(data).ContinueWith((task) =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("fault");
                foreach (var inner in task.Exception.InnerExceptions)
                {
                    if (inner is FunctionsException)
                    {
                        var e = (FunctionsException)inner;
                        // Function error code, will be INTERNAL if the failure
                        // was not handled properly in the function call.
                        var code = e.ErrorCode;
                        Debug.Log(code);
                    }
                }
            }
            else
            {
                //string result = task.Result.ToString();
                //  Debug.Log("RESULT:" + result);
            }

            CloudFunctionFinished();
            Debug.Log("JoinPartyResult : RESULT: " + JsonConvert.SerializeObject(task.Result.Data, Formatting.Indented));

            return (string)task.Result.Data;
        });
    }




    //public async void AttackPartyEnemy()
    //{
    //    CloudFunctionCalled();
    //    Debug.Log("AttackPartyEnemy");
    //    var data = new Dictionary<string, object>();
    //    data["characterUid"] = AccountDataSO.CharacterData.uid;
    //    data["partyUid"] = AccountDataSO.PartyData.uid;
    //    data["equipSlotId"] = 1;

    //    await FirebaseFunctions.DefaultInstance.GetHttpsCallable("general2-attackPartyEnemy").CallAsync(data).ContinueWith(OnCloudFuntionResult);

    //    CloudFunctionFinished();
    //}





    public async void GrantNewEquip()
    {
        CloudFunctionCalled();
        Debug.Log("GrantNewEquip");
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;


        await CallCloudFunction("equip-grantNewEquip", data);
    }


    private string OnCloudFuntionResult(Task<HttpsCallableResult> _task)
    {
//        Debug.Log("sem tu");
        if (_task.IsFaulted)
        {
            string resultError = "ERROR " + _task.Exception.InnerException.Message;
            return resultError;

        }
        string json = JsonConvert.SerializeObject(_task.Result.Data, Formatting.Indented);
        return json;

        //if (_task.IsFaulted)
        //{
        //    foreach (var inner in _task.Exception.InnerExceptions)
        //    {
        //        if (inner is FunctionsException)
        //        {
        //            var e = (FunctionsException)inner;
        //            // Function error code, will be INTERNAL if the failure
        //            // was not handled properly in the function call.
        //            var code = e.ErrorCode + " - " + e.Message;
        //            Debug.Log(code);
        //        }
        //    }
        //}

        //Debug.Log("Result Cloud call: " + (string)_task.Result.Data);
        //return (string)_task.Result.Data;


    }

    //public void GrantSkill()
    //{
    //    CloudFunctionCalled();
    //    Debug.Log("Grant skill..");
    //    var data = new Dictionary<string, string>();
    //    data["characterUid"] = AccountDataSO.CharacterData.uid;
    //    //data["encounterUid"] = _encounterId;


    //    FirebaseFunctions.DefaultInstance.GetHttpsCallable("skills-grantNewSkill").CallAsync(data).ContinueWith((task) =>
    //    {
    //        if (task.IsFaulted)
    //        {

    //            foreach (var inner in task.Exception.InnerExceptions)
    //            {
    //                if (inner is FunctionsException)
    //                {
    //                    var e = (FunctionsException)inner;
    //                    // Function error code, will be INTERNAL if the failure
    //                    // was not handled properly in the function call.
    //                    var code = e.ErrorCode + " - " + e.Message;
    //                    Debug.Log(code);
    //                }
    //            }
    //        }
    //        else
    //        {
    //            //string result = task.Result.ToString();
    //            //  Debug.Log("RESULT:" + result);
    //        }

    //        Debug.Log("GrantSkill : RESULT: " + JsonConvert.SerializeObject(task.Result.Data, Formatting.Indented));

    //        CloudFunctionFinished();
    //        return (string)task.Result.Data;
    //    });
    //}

    //public void EquipSkill(string _slotUid, int _slotId, int setId, string _replacedSlotUid)
    //{
    //    CloudFunctionCalled();
    //    Debug.Log("equip skill..");
    //    Debug.Log("equip skill._slotId. " + _slotId);
    //    var data = new Dictionary<string, object>();
    //    data["characterUid"] = AccountDataSO.CharacterData.uid;
    //    data["skillUid"] = _slotUid;
    //    data["equipSlotId"] = _slotId;
    //    data["equipSetId"] = setId;
    //    data["replacedSkillUid"] = _replacedSlotUid;
    //    //data["encounterUid"] = _encounterId;

    //    Debug.Log("started ");

    //    FirebaseFunctions.DefaultInstance.GetHttpsCallable("general2-equipSkill").CallAsync(data).ContinueWith((task) =>
    //    {
    //        if (task.IsFaulted)
    //        {
    //            Debug.Log("fault");
    //            foreach (var inner in task.Exception.InnerExceptions)
    //            {
    //                if (inner is FunctionsException)
    //                {
    //                    var e = (FunctionsException)inner;
    //                    // Function error code, will be INTERNAL if the failure
    //                    // was not handled properly in the function call.
    //                    var code = e.ErrorCode;
    //                    Debug.Log(code);
    //                }
    //            }
    //        }
    //        else
    //        {
    //            //string result = task.Result.ToString();
    //            //  Debug.Log("RESULT:" + result);
    //        }

    //        CloudFunctionFinished();
    //        Debug.Log("equipSkill : RESULT: " + JsonConvert.SerializeObject(task.Result.Data, Formatting.Indented));

    //        return (string)task.Result.Data;
    //    });
    //}

    //public void SearchForRandomEncounterFromOthers()
    //{
    //    CloudFunctionCalled();
    //    Debug.Log("Searching for Encouters by other");
    //    var data = new Dictionary<string, string>();
    //    data["characterUid"] = AccountDataSO.CharacterData.uid;


    //    FirebaseFunctions.DefaultInstance.GetHttpsCallable("encounter-searchForRandomEncounterFromOthers").CallAsync(data).ContinueWith((task) =>
    //    {
    //        if (task.IsFaulted)
    //        {
    //            Debug.Log("fault");
    //            foreach (var inner in task.Exception.InnerExceptions)
    //            {
    //                if (inner is FunctionsException)
    //                {
    //                    var e = (FunctionsException)inner;
    //                    // Function error code, will be INTERNAL if the failure
    //                    // was not handled properly in the function call.
    //                    var code = e.ErrorCode;
    //                    Debug.Log(code);
    //                }
    //            }
    //        }
    //        else
    //        {
    //            //string result = task.Result.ToString();
    //            //  Debug.Log("RESULT:" + result);
    //        }
    //        CloudFunctionFinished();
    //        Debug.Log("SearchForRandomEncounterFromOthers : RESULT: " + JsonConvert.SerializeObject(task.Result.Data, Formatting.Indented));

    //        return (string)task.Result.Data;
    //    });
    //}





}
