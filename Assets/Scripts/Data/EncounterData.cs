using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Firebase.Firestore;

using UnityEngine;


namespace simplestmmorpg.data
{



    [Serializable]
    [FirestoreData]
    public class EncounterData
    {


        [field: SerializeField]
        [FirestoreProperty]
        public string uid { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string foundByCharacterUid { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int maxCombatants { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<CombatEnemy> enemies { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<CombatMember> combatants { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<string> combatantList { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<string> watchersList { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public bool isFull { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string encounterContext { get; set; }


        //[field: SerializeField]
        //[FirestoreProperty]
        //public int characterSlotsLeft { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string foundByName { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public string chatLog { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string combatLog { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int randomIndex { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string expireDate { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public WorldPosition position { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int turnNumber { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string expireDateTurn { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public CombatFlowEntry[] combatFlow { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<ContentContainer> bonusLoot { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int curseCount { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<PerkOfferDefinition> perksOffers { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<PerkOfferDefinition> perksOffersRare { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<PerkChoiceParticipant> perkChoices { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int joinPrice { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<string> forbiddenCharacterUids { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string typeOfOrigin { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int tier { get; set; }





        public int PendingPerksChoicesAmount()
        {
            if (perksOffers.Count == 0 && perksOffersRare.Count == 0) //je to ambush?
                return 0;

            return (combatants.Count - perkChoices.Count);
        }

        public bool IsPerkUidAmongPerkChoices(string _perkUid)
        {
            return (perkChoices.FirstOrDefault(perk => perk.choosenPerk != null && perk.choosenPerk.uid == _perkUid) != null);
        }

        public bool IsExpired()
        {
            double ExpireMilis = double.Parse(expireDate);
            double NowInMilis = Utils.GetNowInMillis();

            return (ExpireMilis - NowInMilis) <= 0;
        }

        public string GetTurnTimeTimeLeft()
        {
            double ExpireMilis = double.Parse(expireDateTurn);
            double NowInMilis = Utils.GetNowInMillis();


            double durationLeft = ExpireMilis - NowInMilis;

            double secondsLeft = durationLeft / 1000;


            if (secondsLeft > 0)
                return "<color=\"yellow\">" + Mathf.Ceil((float)secondsLeft) + "</color> sec left";
            else
                return "<color=\"yellow\">Waiting for all players to Rest</color>";

        }

        public bool HasTurnTimerExpired()
        {
            double ExpireMilis = double.Parse(expireDateTurn);
            double NowInMilis = Utils.GetNowInMillis();
            double durationLeft = ExpireMilis - NowInMilis;
            double secondsLeft = durationLeft / 1000;

            return secondsLeft <= 0;
        }


        public string GetTimeLeft()
        {
            double ExpireMilis = double.Parse(expireDate);
            double NowInMilis = Utils.GetNowInMillis();

            //Debug.Log("expire milis from server: "+ expireDate);
            //     Debug.Log("New milis on client: " + NowInMilis);

            double durationLeft = ExpireMilis - NowInMilis;

            //    Debug.Log("durationLeft : " + durationLeft);
            double minutesLeft = durationLeft / 60000;
            double hoursLeft = durationLeft / 3600000;


            //    Debug.Log("minutesLeft: " + minutesLeft + "hoursLeft: " + hoursLeft);

            if (minutesLeft < 0)
                return "Expired";
            if (minutesLeft < 2)
                return "Less 2 minutes left";
            if (minutesLeft < 10)
                return "Less 10 minutes left";
            else if (minutesLeft < 30)
                return "Less 30 minutes left";
            else if (minutesLeft < 60)
                return "Less than hour left";
            else
                return "Less than " + Mathf.Ceil((float)hoursLeft) + " hours left";




            // Return the time converted into UTC
            //    return DateTime.Parse(expireDate);
            // return DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(expireDate.ToString())).UtcDateTime;
        }


        public bool IsWatchingEncounter(string _characterUid)
        {
            foreach (var item in watchersList)
            {
                if (item == _characterUid)
                    return true;
            }

            return false;
        }


        public bool IsParticipatingInCombat(string _characterUid)
        {
            foreach (var item in combatants) //Get My CombatMember data
            {
                if (item.characterUid == _characterUid)
                    return true;
            }

            return false;
        }

        //public CombatMember GetMyCombatMemberData(string _characterUid)
        //{
        //    foreach (var item in combatants) //Get My CombatMember data
        //    {
        //        if (item.characterUid == _characterUid)
        //            return item;
        //    }
        //    return null;
        //}

        public CombatMember GetCombatMemeberByUid(string _uid)
        {
            foreach (var item in combatants)
            {
                if (item.characterUid == _uid)
                    return item;
            }
            return null;
        }


        public CombatEnemy GetCombatEnemyByUid(string _uid)
        {
            foreach (var item in enemies)
            {
                if (item.uid == _uid)
                    return item;
            }
            return null;
        }


        public CombatEntity GetCombatEntityByUid(string _uid)
        {
            var result = GetCombatMemeberByUid(_uid);
            if (result != null)
                return result;

            var resultEnemy = GetCombatEnemyByUid(_uid);
            if (resultEnemy != null)
                return resultEnemy;

            return null;
        }

        public int GetLivingCombatantsCount()
        {
            int count = 0;
            foreach (var item in combatants)
            {
                if (item.stats.health > 0)
                    count++;
            }
            return count;
        }
    }
    //[FirestoreProperty]
    //public Skill[] skills { get; set; }


    [Serializable]
    [FirestoreData]
    public class PerkChoiceParticipant
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string characterUid { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public PerkOfferDefinition choosenPerk { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string characterPortraitId { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string characterClass { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string characterName { get; set; }


    }

    [Serializable]
    [FirestoreData]
    public abstract class CombatEntity
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string uid { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string displayName { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<CombatBuff> buffs { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public CombatStats stats { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int level { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int blockAmount { get; set; }

        public abstract string GetCharacterClassId();

        public abstract string GetPortraitId();

        public abstract string GetDisplayName();


    }



    //[Serializable]
    //[FirestoreData]
    //public class Tier
    //{

    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public List<PerkOffer> perkOffers { get; set; }

    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public int entryTimePrice { get; set; }

    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public List<string> enemies { get; set; }

    //    //[field: SerializeField]
    //    //[FirestoreProperty]
    //    //public List<DropTable> dropTables { get; set; }

    //}








    [Serializable]
    [FirestoreData]
    public class PerkOfferDefinition
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string uid { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int timePrice { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public int stockLeft { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int stockClaimed { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string rarity { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int curseCount { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<SimpleTally> restrictionClass { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<SimpleTally> restictionProfession { get; set; }

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

        [field: SerializeField]
        [FirestoreProperty]
        public List<SimpleTally> specialEffectId { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string rarePerkGroupId { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public int successPrice { get; set; }

        public int StockRemaining()
        {
            if (stockLeft == -1)
                return 999;

            return stockLeft - stockClaimed;
        }

        public string GetDescription()
        {
            string result = "";
            foreach (var effect in specialEffectId)
            {


                string effectResult = "Unknown description";
                if (Utils.DescriptionsMetadata.GetPerkSpecialEffectsMetadata(effect.id) != null)
                {

                    var replacements = new List<(string key, string value)> { };

                    //int i = 0;
                    //foreach (var value in amounts)
                    //{
                    //    replacements.Add((i.ToString(), value.ToString()));
                    //    i++;
                    //}
                    replacements.Add(("0", effect.count.ToString()));

                    StringBuilder templateBuild = new StringBuilder(Utils.DescriptionsMetadata.GetPerkSpecialEffectsMetadata(effect.id).description.EN);
                    //  result = replacements.Aggregate(templateBuild, (s, t) => s.Replace($"{{{t.key}}}%", (int.Parse(t.value) * 100f).ToString())).ToString();

                    effectResult = replacements.Aggregate(templateBuild, (s, t) => s.Replace($"{{{t.key}}}%", "<color=\"yellow\">" + (((float.Parse(t.value)) * 100f).ToString()) + "%</color>")).ToString();
                    effectResult = replacements.Aggregate(templateBuild, (s, t) => s.Replace($"{{{t.key}}}", "<color=\"yellow\">" + t.value + "</color>")).ToString();

                }

                result += Utils.ReplacePlaceholdersInTextWithDescriptionFromMetadata(effectResult) + "\n";

            }
            return result;

        }

    }






    [Serializable]
    [FirestoreData]
    public class EnemyDefinitionsStatsMoveSet
    {

        [field: SerializeField]
        [FirestoreProperty]
        public List<EnemyDefinitionsStatsMoveSetSkill> skills { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public int turnMax { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int turnMin { get; set; }
    }


    [Serializable]
    [FirestoreData]
    public class EnemyDefinitionsStatsMoveSetSkill
    {

        [field: SerializeField]
        [FirestoreProperty]
        public List<int> amounts { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public float changeToCast { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string id { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int repeatedCastLimit { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string typeId { get; set; }
    }



    [Serializable]
    [FirestoreData]
    public class CombatEnemy : CombatEntity
    {


        [field: SerializeField]
        [FirestoreProperty]
        public string enemyId { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int mLevel { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public bool isRare { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string targetUid { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<SimpleTally> threatMetter { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<EnemyDefinitionsStatsMoveSet> moveSet { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public EnemyDefinitionsStatsMoveSetSkill nextSkill { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int repeatedCastCount { get; set; }



        public override string GetCharacterClassId()
        {
            return "ENEMY";
        }


        public override string GetPortraitId()
        {
            return enemyId;
        }

        public override string GetDisplayName()
        {
            if (Utils.DescriptionsMetadata.GetEnemyMetadata(enemyId) == null)
                return "Name not found";
            else
                return Utils.DescriptionsMetadata.GetEnemyMetadata(enemyId).title.GetText();
        }

    }


    [Serializable]
    [FirestoreData]
    public class CombatMember : CombatEntity
    {


        [field: SerializeField]
        [FirestoreProperty]
        public string characterUid { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string characterClass { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public List<CombatSkill> skillsInHand { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<CombatSkill> skillsDrawDeck { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<CombatSkill> skillsDiscardDeck { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int damageDone { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int healingDone { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public bool hasRested { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int deckShuffleCount { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<string> blesses { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public string characterPortrait { get; set; }

        public override string GetCharacterClassId()
        {
            return characterClass;
        }

        public override string GetPortraitId()
        {
            return characterPortrait;
        }
        public override string GetDisplayName()
        {
            return displayName;
        }


        public bool HasEnoughManaToCastAnySkill()
        {
            foreach (var item in skillsInHand)
            {
                if (item.manaCost <= stats.mana)
                {
                    Debug.Log("Mas manu : " + item.manaCost + " moje mana : " + stats.mana);

                    return true;
                }
            }

            return false;
        }



    }


    [Serializable]
    [FirestoreData]
    public class CombatBuff
    {

        //[field: SerializeField]
        //[FirestoreProperty]
        //public string uid { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string buffId { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int durationTurns { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int turnsLeft { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<float> amounts { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int rank { get; set; }

        public string GetDescription()
        {
            string result = "Unknown description";
            if (Utils.DescriptionsMetadata.GetSkillMetadata(buffId) != null)
            {

                var replacements = new List<(string key, string value)> { };

                int i = 0;
                foreach (var value in amounts)
                {
                    replacements.Add((i.ToString(), value.ToString()));
                    i++;
                }

                StringBuilder templateBuild = new StringBuilder(Utils.DescriptionsMetadata.GetSkillMetadata(buffId).description.EN);
                //  result = replacements.Aggregate(templateBuild, (s, t) => s.Replace($"{{{t.key}}}%", (int.Parse(t.value) * 100f).ToString())).ToString();

                result = replacements.Aggregate(templateBuild, (s, t) => s.Replace($"{{{t.key}}}%", "<color=\"yellow\">" + (((float.Parse(t.value)) * 100f).ToString()) + "%</color>")).ToString();
                result = replacements.Aggregate(templateBuild, (s, t) => s.Replace($"{{{t.key}}}", "<color=\"yellow\">" + t.value + "</color>")).ToString();

                result += " Lasts for " + durationTurns.ToString() + " turns";
            }

            return result;

        }

        public string GetTitle()
        {
            if (Utils.DescriptionsMetadata.GetSkillMetadata(buffId) != null)
                return Utils.DescriptionsMetadata.GetSkillMetadata(buffId).title.EN;
            else
                return buffId;

        }
    }






    [Serializable]
    [FirestoreData]
    public class CombatSkill
    {

        [field: SerializeField]
        [FirestoreProperty]
        public string skillId { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string skillGroupId { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public bool singleUse { get; set; }


        //[field: SerializeField]
        //[FirestoreProperty]
        //public string imageId { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string characterClass { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public int manaCost { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public List<float> amounts { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public CombatBuff buff { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public bool validTarget_AnyAlly { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public bool validTarget_Self { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public bool validTarget_AnyEnemy { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string rarity { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int quality { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public bool alreadyUsed { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string uid { get; set; }
        //  [field: SerializeField]
        //    public BaseMetadata metadata { get { return Utils.GetMetadataForSkill(skillId); } }//{ get; set; }


        public string GetDescription()
        {
            string result = "Unknown description";
            if (Utils.DescriptionsMetadata.GetSkillMetadata(skillId) != null)
            {

                var replacements = new List<(string key, string value)> { };

                int i = 0;
                foreach (var value in amounts)
                {
                    replacements.Add((i.ToString(), value.ToString()));
                    i++;
                }

                StringBuilder templateBuild = new StringBuilder(Utils.DescriptionsMetadata.GetSkillMetadata(skillId).description.EN);
                //  result = replacements.Aggregate(templateBuild, (s, t) => s.Replace($"{{{t.key}}}%", (int.Parse(t.value) * 100f).ToString())).ToString();

                result = replacements.Aggregate(templateBuild, (s, t) => s.Replace($"{{{t.key}}}%", "<color=\"yellow\">" + (((float.Parse(t.value)) * 100f).ToString()) + "%</color>")).ToString();
                result = replacements.Aggregate(templateBuild, (s, t) => s.Replace($"{{{t.key}}}", "<color=\"yellow\">" + t.value + "</color>")).ToString();

                if (singleUse)
                    result += "<color=\"yellow\">" + " Single Use." + "</color>";

                if (!validTarget_AnyAlly && !validTarget_AnyEnemy && !validTarget_Self)
                    result += "<color=\"yellow\">" + " Unplayable." + "</color>";

            }

            result = Utils.ReplacePlaceholdersInTextWithDescriptionFromMetadata(result);
            return result;

        }

        public string GetTitle()
        {
            if (Utils.DescriptionsMetadata.GetSkillMetadata(skillId) != null)
                return Utils.ReplacePlaceholdersInTextWithDescriptionFromMetadata(Utils.DescriptionsMetadata.GetSkillMetadata(skillId).title.EN);
            else
                return skillId;

        }





    }

    [Serializable]
    [FirestoreData]
    public class CombatStats
    {

        [field: SerializeField]
        [FirestoreProperty]
        public int manaMax { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int mana { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int healthMax { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int leastHealth { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int healthFatiguePenalty { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int health { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int activeBlockAmount { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int hpRegenTotal { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int critChanceTotal { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int damagePowerTotal { get; set; }

    }


    [Serializable]
    [FirestoreData]
    public class CombatFlowEntry
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string caster { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string target { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string effectId { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int amount { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public bool isPositive { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public bool isSpecialEffect { get; set; }

    }



}
