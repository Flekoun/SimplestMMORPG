using System;
using System.Collections.Generic;
using Firebase.Firestore;
using simplestmmorpg.data;

using UnityEngine;


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
        public ExploredPositions exploredPositions { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<SimpleTallyWithMax> professions { get; set; }


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

        public int GetTotalGearAttribute(Utils.EQUIP_ATTRIBUTES _attribute)
        {
            int totalAmount = 0;
            foreach (var equip in equipment)
            {
                totalAmount += equip.attributes.GetAttributeAmount(_attribute);
            }

            return totalAmount;
        }

        public int GetKillsForEnemyId(string _enemyId)
        {
            foreach (var item in monsterKills)
            {
                if (item.id == _enemyId)
                    return item.count;
            }

            return 0;
        }

        public bool IsQuestCompleted(QuestgiverMeta _questgiver)
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


        public bool IsPositionExplored(string _position)
        {
            foreach (var item in exploredPositions.pointsOfInterest)
            {
                if (item == _position)
                    return true;
            }

            foreach (var item in exploredPositions.locations)
            {
                if (item == _position)
                    return true;
            }

            return false;
        }
    }




    [Serializable]
    [FirestoreData]
    public class ExploredPositions
    {
        [field: SerializeField]
        [FirestoreProperty]
        public List<string> pointsOfInterest { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<string> locations { get; set; }

    }

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
        public int expNeededToReachNextLevel { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int expNeededToReachLastLevel { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int level { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public float healthMultiplier { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public float manaMultiplier { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int stamina { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int spirit { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int strength { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int intellect { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int agility { get; set; }



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
        public int silver { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int food { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int time { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int fatigue { get; set; }

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
    }



}
