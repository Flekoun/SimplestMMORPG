
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using Firebase.Functions;
using Newtonsoft.Json;
using RoboRyanTron.Unite2017.Variables;
using simplestmmorpg.data;
using simplestmmorpg.playerData;
using TMPro;

using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu]
public class FirebaseCloudFunctionSO : ScriptableObject
{
    public AccountDataSO AccountDataSO;
    public StringVariable ServerSecret;
    FirebaseFunctions functions;

    public UnityAction<bool> OnCloudFunctionInProgress;
    public UnityAction OnCloudFunctionFinished;


    //public bool UseLocalHost = false;


    public void OnEnable()
    {
        functions = FirebaseFunctions.DefaultInstance;

        //#if UNITY_EDITOR
        //if (UseLocalHost)
        //    functions.UseFunctionsEmulator("localhost:5001");
        //#endif

    }


    private void CloudFunctionCalled(bool _hiddenScreenLock = false)
    {
        //  Debug.Log("showing");
        OnCloudFunctionInProgress?.Invoke(_hiddenScreenLock);
    }

    private void CloudFunctionFinished(string _result)
    {
        //    Debug.Log("hiding");

        if (_result.StartsWith("ERROR"))
        {
            Debug.Log(_result);
            UIManager.instance.SpawnErrorText(_result);
        }

        OnCloudFunctionFinished?.Invoke();
    }

    private void CloudFunctionFinished()
    {
        OnCloudFunctionFinished?.Invoke();
    }

    private async Task<CloudFunctionCallResult> CallCloudFunction(string _cloudFunctionName, Dictionary<string, object> _data)
    {
        //string result = await FirebaseFunctions.DefaultInstance.GetHttpsCallable(_cloudFunctionName).CallAsync(_data).ContinueWith(OnCloudFuntionResult);
        //CloudFunctionFinished(result);
        try
        {
            await CallCloudFunctionNew(_cloudFunctionName, _data);
            CloudFunctionFinished();
            return new CloudFunctionCallResult(true);
        }
        catch (Exception ex)
        {
            Debug.Log($"Cloud Function call failed with {ex.Message}");
            UIManager.instance.ImportantMessage.ShowMesssage(ex.Message, 4f);
            CloudFunctionFinished();
            return new CloudFunctionCallResult(ex.Message);
        }
    }


    private async Task CallCloudFunctionNew(string functionName, Dictionary<string, object> _data)
    {
        var function = functions.GetHttpsCallable(functionName);
        await function.CallAsync(_data);
    }


    public struct CloudFunctionCallResult
    {
        public bool Result;
        public string ErrorMessage;

        public CloudFunctionCallResult(string _errorMessage)
        {
            Result = false;
            ErrorMessage = _errorMessage;
        }
        public CloudFunctionCallResult(bool _result)
        {
            if (!_result)
                Debug.LogError("nemuzes to takhle pouzit");

            Result = true;
            ErrorMessage = "";
        }

    }


    public async void Test()
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();

        Debug.Log("test  ... ");
        await CallCloudFunction("utils-test", data);
    }

    public async void CheatTime()
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;

        Debug.Log("time cheat  ... ");
        await CallCloudFunction("utils-cheatTime", data);
    }


    public async Task<CloudFunctionCallResult> ExploreDungeon()
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;

        Debug.Log("exploreDungeon ... ");
        return await CallCloudFunction("encounter-exploreDungeon", data);
    }

    public async Task<CloudFunctionCallResult> retireCharacter(string _characterUid)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = _characterUid;

        Debug.Log("retire Character ... ");
        return await CallCloudFunction("retireCharacter", data);
    }

    //public async Task<CloudFunctionCallResult> ScavengePointsPurchase()
    //{
    //    CloudFunctionCalled();
    //    var data = new Dictionary<string, object>();
    //    data["characterUid"] = AccountDataSO.CharacterData.uid;

    //    Debug.Log("scavengePointsPurchase  ... ");
    //    return await CallCloudFunction("scavengePointsPurchase", data);
    //}

    public async Task<CloudFunctionCallResult> RestDeep(List<string> _foodSuppliesUids, List<int> _foodSuppliesAmounts)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["foodSuppliesUids"] = _foodSuppliesUids.ToArray();
        data["foodSuppliesAmounts"] = _foodSuppliesAmounts.ToArray();

        Debug.Log("resting deep  ... ");
        return await CallCloudFunction("restDeep", data);
    }


    public async Task<CloudFunctionCallResult> RetreatFromEncounter(string _encounterId)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["encounterUid"] = _encounterId;
        data["characterUid"] = AccountDataSO.CharacterData.uid;

        Debug.Log("retreating from encounter  ... ");
        return await CallCloudFunction("encounter-retreatFromEncounter", data);
    }


    public async Task<CloudFunctionCallResult> TradeWithVendor(List<string> _itemToBuyUids, List<int> _itemToBuyAmounts, List<string> _itemToSellUids, List<int> _itemToSellAmounts, string _vendorUid)
    {
        //Debug.Log("itemsToSellUids : " + _itemToSellUids);
        //foreach (var item in _itemToSellUids)
        //    Debug.Log(" itemUid : " + item);

        //Debug.Log("_itemToSellAmounts : " + _itemToSellAmounts);
        //foreach (var item in _itemToSellAmounts)
        //    Debug.Log(" itemAmount : " + item);

        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["vendorUid"] = _vendorUid;
        data["vendorItemsToBuyUids"] = _itemToBuyUids.ToArray();
        data["itemsToSellUids"] = _itemToSellUids.ToArray();
        data["vendorItemsToBuyAmounts"] = _itemToBuyAmounts.ToArray();
        data["itemsToSellAmounts"] = _itemToSellAmounts.ToArray();

        Debug.Log("trading items from vendor ... ");

        return await CallCloudFunction("vendor-tradeWithVendor", data);

    }

    public async Task<CloudFunctionCallResult> ChapelRemoveCurses()
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;

        Debug.Log("removing curses in chapel  ... ");
        return await CallCloudFunction("specials-chapelRemoveCurses", data);
    }

    public async Task<CloudFunctionCallResult> TreasureOpenWithKey()
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;

        Debug.Log("opening treasure with magic key  ... ");
        return await CallCloudFunction("specials-treasureOpenWithKey", data);
    }


    public async Task<CloudFunctionCallResult> TreasureOpenForCurse()
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;

        Debug.Log("opening treasure for curse  ... ");
        return await CallCloudFunction("specials-treasureOpenForCurse", data);
    }


    public async Task<CloudFunctionCallResult> TreasureOpenFree()
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;

        Debug.Log("opening treasure for free  ... ");
        return await CallCloudFunction("specials-treasureOpenFree", data);
    }


    public async Task<CloudFunctionCallResult> ChapelRecieveBless()
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;

        Debug.Log("gaining bless in chapel  ... ");
        return await CallCloudFunction("specials-chapelRecieveBless", data);
    }


    public async Task<CloudFunctionCallResult> ChapelGift()
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;

        Debug.Log("gaining gift in chapel  ... ");
        return await CallCloudFunction("specials-chapelGift", data);
    }



    public async Task<CloudFunctionCallResult> InnBind()
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;

        Debug.Log("binding in inn  ... ");
        return await CallCloudFunction("specials-innBind", data);
    }


    public async Task<CloudFunctionCallResult> InnCarriage(string _targetPoIId)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["targetInnId"] = _targetPoIId;

        Debug.Log("taking carriage in inn  ... ");
        return await CallCloudFunction("specials-innCarriage", data);
    }

    public async void InnHealthRestore()
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;

        Debug.Log("health ressting in inn  ... ");
        await CallCloudFunction("specials-innHealthRestore", data);
    }

    //kdyz je perk uid "" - znamena claimnout vsechny perky naraz
    public async Task<CloudFunctionCallResult> PendingRewardClaim(string _perkUid)
    {

        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["perkUid"] = _perkUid;

        Debug.Log("claiming perk ... ");
        return await CallCloudFunction("perks-pendingRewardClaim", data);
    }

    public Task CraftRecipe(string _recipeId)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["recipeId"] = _recipeId;



        Debug.Log("changing character portrait  ... ");
        return CallCloudFunction("crafting-craftRecipe", data);
    }

    public async Task<CloudFunctionCallResult> upgradeEquipQuality(string _equipToUpgradeUid)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["equipToUpgradeUid"] = _equipToUpgradeUid;

        Debug.Log("upgrading quality  ... ");
        return await CallCloudFunction("equip-upgradeEquipQuality", data);
    }

    public async void ChangeCharacterPortrait(string _portraitId)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["portraitId"] = _portraitId;

        Debug.Log("changing character portrait  ... ");
        await CallCloudFunction("changeCharacterPortrait", data);
    }



    public async void ChooseEncounterPerkOffer(string _encounterUid, string _perOfferUid)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["encounterUid"] = _encounterUid;
        data["perkOfferUid"] = _perOfferUid;

        Debug.Log("choosin perk offer   ... ");
        await CallCloudFunction("encounter-chooseEncounterPerkOffer", data);
    }


    public async void DropItem(List<string> _contentUids)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["contentUids"] = _contentUids.ToArray();

        Debug.Log("dropping item  ... ");
        await CallCloudFunction("dropItem", data);
    }

    public async Task<CloudFunctionCallResult> CheckForIntegrityOfCharacterData(string _characterUid)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterToCheckUid"] = _characterUid;

        Debug.Log("checking integrity of character  ... ");
        return await CallCloudFunction("checkForIntegrityOfCharacterData", data);
    }

    public async void TrainAtTrainer(string _trainerUid)
    {

        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["trainerUid"] = _trainerUid;

        Debug.Log("claiming gatherable ... ");
        await CallCloudFunction("trainer-trainAtTrainer", data);
    }

    public async void ClaimGatherable(string _gatherableUid)
    {

        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["gatherableUid"] = _gatherableUid;

        Debug.Log("claiming gatherable ... ");
        await CallCloudFunction("gatherables-claimGatherable", data);
    }

    public Task ClaimGatherableAsync(string _gatherableUid)
    {

        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["gatherableUid"] = _gatherableUid;

        Debug.Log("claiming gatherable ... ");
        return CallCloudFunction("gatherables-claimGatherable", data);
    }


    public async Task<CloudFunctionCallResult> EnterDungeon()
    {

        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["callerCharacterUid"] = AccountDataSO.CharacterData.uid;
        //    data["dungeonPointOfInterestId"] = _dungeonPoIId;

        Debug.Log("entering dungeon ... ");
        return await CallCloudFunction("party-enterDungeon", data);
    }


    public async void ExitDungeon()
    {

        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["callerCharacterUid"] = AccountDataSO.CharacterData.uid;

        Debug.Log("exiting dungeon ... ");
        await CallCloudFunction("party-exitDungeon", data);
    }

    public async void PutContentOnAuctionHouse(string _contentToSellUid, int _buyoutPrice, int _bidPrice, int _amount)
    {


        CloudFunctionCalled();

        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["contentToSell"] = _contentToSellUid;
        data["amount"] = _amount;
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

    public async Task<CloudFunctionCallResult> ClaimInboxItem(string _inboxItemUid)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["inboxItemUid"] = _inboxItemUid;


        Debug.Log("claiming inbox item  ... ");
        return await CallCloudFunction("inbox-claimInboxItem", data);
        //await FirebaseFunctions.DefaultInstance.GetHttpsCallable("inbox-claimInboxItem").CallAsync(data).ContinueWith(OnCloudFuntionResult);
        //CloudFunctionFinished();
    }

    public async Task<CloudFunctionCallResult> ClaimPlayerInboxItem(string _inboxItemUid)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["playerUid"] = AccountDataSO.PlayerData.uid;
        data["inboxItemUid"] = _inboxItemUid;


        Debug.Log("claiming player inbox item  ... ");
        return await CallCloudFunction("inbox-claimPlayerInboxItem", data);
        //await FirebaseFunctions.DefaultInstance.GetHttpsCallable("inbox-claimInboxItem").CallAsync(data).ContinueWith(OnCloudFuntionResult);
        //CloudFunctionFinished();
    }

    public async void WorldMapTravel(string _locationId)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["locationId"] = _locationId;
        data["serverSecret"] = ServerSecret.Value;


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
        data["serverSecret"] = ServerSecret.Value;

        Debug.Log("traveling to point of interest in location ... ");
        await CallCloudFunction("worldMap-travelToPoI", data);
        //await FirebaseFunctions.DefaultInstance.GetHttpsCallable("worldMap-travel").CallAsync(data).ContinueWith(OnCloudFuntionResult);
        //CloudFunctionFinished();
    }


    public async void ClaimNewDay()
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;

        Debug.Log("claiming new day... ");

        await CallCloudFunction("claimNewDay", data);
        //await FirebaseFunctions.DefaultInstance.GetHttpsCallable("claimTimePool").CallAsync(data).ContinueWith(OnCloudFuntionResult);
        //CloudFunctionFinished();
    }

    public async Task<CloudFunctionCallResult> ConsumeConsumable(string _foodUid)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["consumanleUid"] = _foodUid;

        Debug.Log("consuming consumable ... ");

        return await CallCloudFunction("consumeConsumable", data);
        //await FirebaseFunctions.DefaultInstance.GetHttpsCallable("eatFood").CallAsync(data).ContinueWith(OnCloudFuntionResult);
        //  CloudFunctionFinished();
    }


    public async Task<CloudFunctionCallResult> UseTeleportScroll(string _targetPoI, string _teleportScrollUid)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["targetPoI"] = _targetPoI;
        data["teleportScrollUid"] = _teleportScrollUid;

        Debug.Log("using teleport scroll ... ");

        return await CallCloudFunction("useTeleportScroll", data);

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


    //public async void SellInventoryItems(List<string> _itemsTSellUids)
    //{
    //    CloudFunctionCalled();
    //    var data = new Dictionary<string, object>();
    //    data["characterUid"] = AccountDataSO.CharacterData.uid;
    //    data["inventoryItemsToSellEquipUids"] = _itemsTSellUids.ToArray();

    //    Debug.Log("selling bag items ... ");

    //    await CallCloudFunction("sellInventoryItems", data);
    //    //await FirebaseFunctions.DefaultInstance.GetHttpsCallable("general2-sellInventoryItems").CallAsync(data).ContinueWith(OnCloudFuntionResult);

    //    //CloudFunctionFinished();

    //}


    //public async void BuyVendorItems(List<string> _itemToBuyUids, string _vendorUid)
    //{
    //    CloudFunctionCalled();
    //    var data = new Dictionary<string, object>();
    //    data["characterUid"] = AccountDataSO.CharacterData.uid;
    //    data["vendorUid"] = _vendorUid;
    //    data["vendorItemsToBuyUids"] = _itemToBuyUids.ToArray();

    //    Debug.Log("buying items from vendor ... ");

    //    await CallCloudFunction("vendor-buyVendorItems", data);
    //    //await FirebaseFunctions.DefaultInstance.GetHttpsCallable("vendor-buyVendorItems").CallAsync(data).ContinueWith(OnCloudFuntionResult);

    //    //CloudFunctionFinished();

    //}




    public async void CreateCharacter(string _characterName, string _characterClass, string _characterPortrait)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["playerUid"] = AccountDataSO.PlayerData.uid;
        data["characterName"] = _characterName;
        data["characterClass"] = _characterClass;
        data["characterPortrait"] = _characterPortrait;

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


    public async Task<CloudFunctionCallResult> ExploreMonsters()
    {

        CloudFunctionCalled();

        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;

        // data["pointOfInterestId"] = _pointOfInterestId;//_position.locationId;

        Debug.Log("CreateEncounter started ");
        return await CallCloudFunction("encounter-exploreMonsters", data);


    }

    public async void JoinEncounter(string _encounterId)
    {
        CloudFunctionCalled();
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["encounterUid"] = _encounterId;

//        Debug.Log("Joining encounter... ");

        //await FirebaseFunctions.DefaultInstance.GetHttpsCallable("encounter-joinEncounter").CallAsync(data).ContinueWith(OnCloudFuntionResult);

        //CloudFunctionFinished();

        Debug.Log("Join Encounter... ");
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



    //async public void ApplySkillOnEncounter(string _encounterId, string _uid, string _targetUid)
    //{
    //    string task = await ApplySkillOnEncounterTask(_encounterId, _uid, _targetUid);

    //    CloudFunctionFinished(task);

    //    //  CombatLog combatLog = JsonConvert.DeserializeObject<CombatLog>(task);
    //    //        Debug.Log("Result Cloud call:X " + combatLog.entries[0]);
    //}

    //public Task<string> ApplySkillOnEncounterTask(string _encounterId, string _uid, string _targetUid)
    //{
    //    CloudFunctionCalled();
    //    var data = new Dictionary<string, object>();
    //    data["characterUid"] = AccountDataSO.CharacterData.uid;
    //    data["encounterUid"] = _encounterId;
    //    data["uid"] = _uid;
    //    data["targetUid"] = _targetUid;



    //    Debug.Log("applying skill to  encounter... ");

    //    return FirebaseFunctions.DefaultInstance.GetHttpsCallable("encounter-applySkillOnEncounter").CallAsync(data).ContinueWith((_task) =>
    //    {

    //        //            Debug.Log("sem tu");
    //        if (_task.IsFaulted)
    //        {
    //            string resultError = "ERROR " + _task.Exception.InnerException.Message;
    //            return resultError;

    //        }
    //        string json = JsonConvert.SerializeObject(_task.Result.Data, Formatting.Indented);
    //        return json;
    //    });



    //}

    public async Task<CloudFunctionCallResult> ApplySkillOnEncounter(string _encounterId, List<string> _skillUids, string _targetUid)
    {
        CloudFunctionCalled(true);
        var data = new Dictionary<string, object>();
        data["characterUid"] = AccountDataSO.CharacterData.uid;
        data["encounterUid"] = _encounterId;
        data["skillUids"] = _skillUids;
        data["targetUid"] = _targetUid;

        return await CallCloudFunction("encounter-applySkillOnEncounter", data);
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


        //        Debug.Log("Resting ... ");

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


    public async Task<CloudFunctionCallResult> SendPartyInvite(string _invitedCharacterUid)
    {


        Debug.Log("Send Party Invite");
        CloudFunctionCalled();

        var data = new Dictionary<string, object>();
        data["callerCharacterUid"] = AccountDataSO.CharacterData.uid;
        data["callerCharacterName"] = AccountDataSO.CharacterData.characterName;
        data["invitedCharacterUid"] = _invitedCharacterUid;


        return await CallCloudFunction("party-sendPartyInvite", data);

    }

    public async Task<CloudFunctionCallResult> AcceptPartyInvite(string _partyLeaderUid)
    {


        Debug.Log("Accept Party Invite");
        CloudFunctionCalled();

        var data = new Dictionary<string, object>();
        data["callerCharacterUid"] = AccountDataSO.CharacterData.uid;
        data["partyLeaderUid"] = _partyLeaderUid;


        return await CallCloudFunction("party-acceptPartyInvite", data);

    }


    public async Task<CloudFunctionCallResult> DeclinePartyInvite(string _partyLeaderUid)
    {
        Debug.Log("Decline Party Invite");
        CloudFunctionCalled();

        var data = new Dictionary<string, object>();
        data["partyLeaderUid"] = _partyLeaderUid;


        return await CallCloudFunction("party-declinePartyInvite", data);

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


        await CallCloudFunction("utils-grantNewEquip", data);
    }


    private string OnCloudFuntionResult(Task<HttpsCallableResult> _task)
    {
        //        Debug.Log("sem tu");
        if (_task.IsFaulted)
        {
            string resultError = _task.Exception.InnerException.Message;//"ERROR " + _task.Exception.InnerException.Message;
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
