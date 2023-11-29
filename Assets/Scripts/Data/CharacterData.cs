using System;
using System.Collections.Generic;
using System.Linq;
using Firebase.Firestore;
using simplestmmorpg.data;

using UnityEngine;
using UnityEngine.UIElements;


namespace simplestmmorpg.data
{
    [Serializable]
    [FirestoreData]
    public class CharacterData
    {
        //  [FirestoreProperty]
        //  public string UserUid { get; set; }
        //  [FirestoreProperty]
        //   public string Uid { get; set; }
        //  [field: SerializeField]
        //  [FirestoreProperty

        [field: SerializeField]
        [FirestoreProperty]
        public string uid { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string userUid { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string characterName { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string characterClass { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public Inventory inventory { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<Equip> equipment { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public Currency currency { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public CharacterStats stats { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public WorldPosition position { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<SimpleTally> monsterKills { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<string> questgiversClaimed { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string characterPortrait { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public characterTimestamps timestamps { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<WorldPosition> exploredPositions { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<SimpleTallyWithMax> professions { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public List<string> portraitsUnlocked { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<string> craftingRecipesUnlocked { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int lastClaimedGameDay { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<SimpleTally> pointsOfInterestMaxTierReached { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<PendingReward> pendingRewards { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int innHealhRestsCount { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public WorldPosition homeInn { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<WorldMapMemmoryEntry> worldMapMemmory { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<SimpleTally> foodEffects { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<CombatSkill> curses { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public List<WorldPosition> chapelsUsed { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<string> blesses { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<ChapelInfo> chapelInfo { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<VendorInfo> vendorInfo { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<WorldPosition> treasuresClaimed { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<string> dungeonsFinished { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int seasonNumber { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public bool isRetired { get; set; }




        public bool HasFinishedDungeon(string _dungeonId)
        {
            return dungeonsFinished.Contains(_dungeonId);
        }

        public bool IsTreasureClaimedOnMyPosition()
        {
            foreach (var treasure in treasuresClaimed)
            {
                if (treasure.pointOfInterestId == this.position.pointOfInterestId &&
                    treasure.locationId == this.position.locationId &&
                    treasure.zoneId == this.position.zoneId)
                {
                    return true;
                }
            }

            return false;
        }

        public int GetVendorGoodsPurchased(string _vendorId, string _vendorGoodId)
        {
            // Find the VendorInfo entry for the given vendorId
            VendorInfo vendor = vendorInfo.FirstOrDefault(v => v.vendorId == _vendorId);

            // If the vendor exists, find the good by its id and return its amount
            if (vendor != null)
            {
                ItemIdWithAmount good = vendor.goodsPurchased.FirstOrDefault(g => g.itemId == _vendorGoodId);
                if (good != null)
                {
                    return good.amount;
                }
            }

            // Return 0 if vendor or good is not found
            return 0;
        }



        public ChapelInfo GetChapelInfoOnMyPosition()
        {
            foreach (var info in chapelInfo)
            {
                if (info.worldPosition.pointOfInterestId == this.position.pointOfInterestId &&
                    info.worldPosition.locationId == this.position.locationId &&
                    info.worldPosition.zoneId == this.position.zoneId)
                {
                    return info;
                }
            }

            Debug.LogError("Could not find any chapel info for POI : " + this.position.pointOfInterestId);
            return null;
        }


        public bool hasBless(string _blessId)
        {
            return blesses.Contains(_blessId);
        }

        public WorldMapMemmoryEntry GetWorldMapMemmoryForWorldPosition(WorldPosition _position)
        {
            foreach (var memmory in worldMapMemmory)
            {
                if (memmory.worldPosition.pointOfInterestId == _position.pointOfInterestId &&
                    memmory.worldPosition.locationId == _position.locationId &&
                    memmory.worldPosition.zoneId == _position.zoneId)
                    return memmory;
            }

            return null;
        }

        public int GetUnclaimedPerksCount(int _gameDay)
        {
            int totalUnclaimed = 0;
            foreach (var perk in pendingRewards)
            {
                if (IsPerkClaimable(_gameDay, perk))
                    totalUnclaimed++;
            }

            return totalUnclaimed;
        }

        public bool IsPerkClaimable(int _gameDay, PendingReward _perk)
        {
            return _gameDay >= _perk.lastClaimGameDay + _perk.recurrenceInGameDays;
        }

        public int GetMaxTierReachedForPointOfInterest(string _pointOfInterestId)
        {
            foreach (var item in pointsOfInterestMaxTierReached)
            {
                if (item.id == _pointOfInterestId)

                    return item.count;

            }
            return -1;
        }


        public SimpleTallyWithMax GetProfessionById(string _professionId)
        {
            foreach (var item in professions)
            {
                if (item.id == _professionId)

                    return item;

            }
            return null;
        }

        public bool HasEnoughProfessionSkillToBeTrained(int _minSkillAmoutNeded, string _professionId)
        {
            if (_minSkillAmoutNeded == 0) return true;

            if (GetProfessionById(_professionId)?.count >= _minSkillAmoutNeded)
                return true;

            return false;
            //foreach (var item in professions)
            //{
            //    if (item.id == _professionId)
            //        if (item.count >= _minSkillAmoutNeded)
            //            return true;

            //}
            //return false;
        }


        public bool HasAlreadyThisOrMoreOfProfessionSkillToBeTrained(int _skillAmounToTrain, string _professionId)
        {
            if (GetProfessionById(_professionId)?.countMax >= _skillAmounToTrain)
                return true;

            return false;
        }



        public int GetTotalHealth(bool _ignoreFatigue)
        {
            int totalHp = this.stats.totalMaxHealth;

            if (!_ignoreFatigue)
                totalHp -= this.stats.healthBlockedByFatigue;//Utils.RoundToInt(totalHp * ((100 - currency.fatigue) / 100f));

            return totalHp;

        }

        public int GetHealthTakenByFatiguePenalty()
        {
            //Debug.Log("GetTotalHealth(true) - GetTotalHealth(false) : " + (GetTotalHealth(true) - GetTotalHealth(false)));
            return GetTotalHealth(true) - GetTotalHealth(false);

        }

        //public int GetTotalGearAttribute(Utils.EQUIP_ATTRIBUTES _attribute)
        //{
        //    int totalAmount = 0;
        //    foreach (var equip in equipment)
        //    {
        //        totalAmount += equip.attributes.GetAttributeAmount(_attribute);
        //    }
        //    //     Debug.Log(_attribute +" : " + totalAmount);

        //    return totalAmount;
        //}

        public int GetKillsForEnemyId(string _enemyId)
        {
            foreach (var item in monsterKills)
            {
                if (item.id == _enemyId)
                    return item.count;
            }

            return 0;
        }

        public bool IsQuestCompleted(Questgiver _questgiver)
        {
            bool areKillsFulfiled = true;
            foreach (var item in _questgiver.killsRequired)
            {
                if (GetKillsForEnemyId(item.id) < item.count)
                {
                    //     Debug.Log("nedokncil jsi! : mas " + (GetKillsForEnemyId(item.id)));

                    areKillsFulfiled = false;
                }
            }


            bool areItemsFulfiled = true;
            foreach (var item in _questgiver.itemsRequired)
            {
                if (inventory.GetAmountOfItemsInInventory(item.id) < item.count)
                {
                    //    Debug.Log("nedokncil jsi! : mas " + (inventory.GetAmountOfItemsInInventory(item.id)));

                    areItemsFulfiled = false;
                }
            }



            return areKillsFulfiled && areItemsFulfiled;
        }


        public bool IsChapelAtMyPositionAlreadyUsed()
        {

            foreach (var item in chapelInfo)
            {

                if (item.worldPosition.zoneId == position.zoneId && item.worldPosition.locationId == position.locationId && item.worldPosition.pointOfInterestId == position.pointOfInterestId)
                {
                    //   Debug.Log("ano pozice je stejna! " + _position.pointOfInterestId.ToString() + " = " + item.pointOfInterestId.ToString() + _position.locationId.ToString() + " = " + item.locationId.ToString() + _position.zoneId.ToString() + " = " + item.zoneId.ToString());
                    return item.used;
                }
            }

            Debug.LogError("Cant find any info about a chapel on a POI : " + position.pointOfInterestId);
            return false;

        }


        public bool IsWorldPositionExplored(WorldPosition _position)
        {
            //Debug.Log("pointOfInterestId:" + _position.pointOfInterestId);
            //Debug.Log("zoneId:" + _position.zoneId);
            //Debug.Log("locationId:" + _position.locationId);



            foreach (var item in exploredPositions)
            {
                //Debug.Log("pointOfInterestId_item:" + item.pointOfInterestId);
                //Debug.Log("zoneId_item:" + item.zoneId);
                //Debug.Log("locationId_item:" + item.locationId);

                if (item.zoneId == _position.zoneId && item.locationId == _position.locationId && item.pointOfInterestId == _position.pointOfInterestId)
                {
                    //   Debug.Log("ano pozice je stejna! " + _position.pointOfInterestId.ToString() + " = " + item.pointOfInterestId.ToString() + _position.locationId.ToString() + " = " + item.locationId.ToString() + _position.zoneId.ToString() + " = " + item.zoneId.ToString());
                    return true;
                }
            }
            return false;

        }

        public bool IsLocationExplored(string _locationId)
        {
            foreach (var item in exploredPositions)
            {
                if (item.locationId == _locationId)
                    return true;
            }

            return false;

        }
    }

    [Serializable]
    [FirestoreData]
    public class VendorInfo
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string vendorId { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<ItemIdWithAmount> goodsPurchased { get; set; }

    }

    [Serializable]
    [FirestoreData]
    public class ChapelInfo
    {
        [field: SerializeField]
        [FirestoreProperty]
        public WorldPosition worldPosition { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string blessId { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public bool used { get; set; }
    }


    [Serializable]
    [FirestoreData]
    public class WorldMapMemmoryEntry
    {
        [field: SerializeField]
        [FirestoreProperty]
        public WorldPosition worldPosition { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<string> specialPointsOfInterest { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public string typeId { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public int tiersCount { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string roomType { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int partySize { get; set; }
    }


    //[Serializable]
    //[FirestoreData]
    //public class ExploredPositions
    //{
    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public List<string> pointsOfInterest { get; set; }

    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public List<string> locations { get; set; }

    //}

    [Serializable]
    [FirestoreData]
    public class characterTimestamps
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string lastClaimTime { get; set; }

    }

    [Serializable]
    [FirestoreData]
    public class WorldPosition
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string locationId { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string zoneId { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public string pointOfInterestId { get; set; }



    }


    //[FirestoreProperty]
    //public Skill[] skills { get; set; }


    [Serializable]
    [FirestoreData]
    public class CharacterStats
    {
        [field: SerializeField]
        [FirestoreProperty]
        public int exp { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int expNeededToReachLastLevel { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int expNeededToReachNextLevel { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int level { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int baseHealth { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int baseMana { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int baseManaRegen { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int baseHealthRegen { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public float baseCritChance { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int baseResistence { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int baseDefense { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int baseDamagePower { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int currentHealth { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int healthBlockedByFatigue { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int skillDrawCount { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int equipBonusHealth { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int equipBonusMana { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int equipBonusManaRegen { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int equipBonusHealthRegen { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public float equipBonusCritChance { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int equipBonusResistence { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int equipBonusDefense { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int equipBonusDamagePower { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int otherBonusHealth { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int otherBonusMana { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int otherBonusManaRegen { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int otherBonusHealthRegen { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public float otherBonusCritChance { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int otherBonusResistence { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int otherBonusDefense { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int otherBonusDamagePower { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int totalMaxHealth { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int totalMaxMana { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int totalManaRegen { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int totalHealthRegen { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public float totalCritChance { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int totalResistence { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int totalDefense { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int totalDamagePower { get; set; }


        //[field: SerializeField]
        //[FirestoreProperty]
        //public int exp { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public int expNeededToReachNextLevel { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public int expNeededToReachLastLevel { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public int level { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public int baseHealth { get; set; }


        //[field: SerializeField]
        //[FirestoreProperty]
        //public int baseMana { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public int baseCritChance { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public int totalMaxMana { get; set; }


        //[field: SerializeField]
        //[FirestoreProperty]
        //public int totalMaxHealth { get; set; }


        //[field: SerializeField]
        //[FirestoreProperty]
        //public int currentHealth { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public int healthBlockedByFatigue { get; set; }

    }

    [Serializable]
    [FirestoreData]
    public class Currency
    {
        [field: SerializeField]
        [FirestoreProperty]
        public int gold { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int monsterEssence { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int time { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int timeMax { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int fatigue { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public int travelPointsMax { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public float travelPoints { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int scavengePoints { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int scavengePointsMax { get; set; }
    }





    [Serializable]
    [FirestoreData]
    public class InventoryBag
    {

        [field: SerializeField]
        [FirestoreProperty]
        public string uid { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string itemId { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public int capacity { get; set; }

    }


    [Serializable]
    [FirestoreData]
    public class Inventory
    {

        [field: SerializeField]
        [FirestoreProperty]
        public InventoryBag[] bags { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int capacityMax { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int capacityLeft { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<ContentContainer> content { get; set; }


        public int GetAmountOfItemsInInventory(string _itemId)
        {
            int totalAmount = 0;
            foreach (var item in content)
            {
                if (item.GetContent().itemId == _itemId)
                    totalAmount += item.GetContent().amount;
            }

            return totalAmount;
        }

        public bool IsItemInInventory(string _itemUid)
        {
            foreach (var item in content)
            {
                if (item.GetContent().uid == _itemUid)
                    return true;
            }

            return false;
        }
    }




    [Serializable]
    [FirestoreData]
    public class PendingReward
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string uid { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string rarity { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<ContentContainer> rewards { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<RandomEquip> rewardsRandomEquip { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<ItemIdWithAmount> rewardsGenerated { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public bool isInstantReward { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int recurrenceInGameDays { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int rewardAtSpecificGameDay { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int rewardAfterSpecificGameDay { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int charges { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public string specialEffectId { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int lastClaimGameDay { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int chargesClaimed { get; set; }

    }


}
