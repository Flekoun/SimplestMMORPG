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
        public CombatEnemy[] enemies { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public CombatMember[] combatants { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string[] combatantList { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string[] watchersList { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public bool isFull { get; set; }

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




        //public bool AlreadyRestedThisTurn(CombatMember _combetMember)
        //{
        //    return _combetMember.restsCount> turnNumber;
        //}

        //public bool HasMoreRestsThanOtherCombatMembers(CombatMember _combetMember)
        //{
        //    foreach (var item in combatants)
        //    {
        //        if (item.restsCount < _combetMember.restsCount)
        //            return true;

        //    }
        //    return false;
        //}

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

        public CombatMember GetMyCombatMemberData(string _characterUid)
        {
            foreach (var item in combatants) //Get My CombatMember data
            {
                if (item.characterUid == _characterUid)
                    return item;
            }
            return null;
        }

        public CombatMember GetCombatMemeberByUid(string _uid)
        {
            foreach (var item in combatants)
            {
                if (item.characterUid == _uid)
                    return item;
            }
            return null;
        }
    }
    //[FirestoreProperty]
    //public Skill[] skills { get; set; }


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

        public abstract string GetPortraitId();

        public abstract string GetDisplayName();


    }

    [Serializable]
    [FirestoreData]
    public class CombatEnemy : CombatEntity
    {

        [field: SerializeField]
        [FirestoreProperty]
        public int damageAmountMin { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int damageAmountMax { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string enemyId { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public bool isRare { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string targetUid { get; set; }

        public override string GetPortraitId()
        {
            return enemyId;
        }

        public override string GetDisplayName()
        {
            if (Utils.GetMetadataForEnemy(enemyId) == null)
                return "Name not found";
            else
                return Utils.GetMetadataForEnemy(enemyId).title.GetText();
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
        public CombatSkill[] skillsInHand { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public CombatSkill[] skillsDrawDeck { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public CombatSkill[] skillsDiscardDeck { get; set; }

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
        public string characterPortrait { get; set; }

        public override string GetPortraitId()
        {
            return characterPortrait;
        }
        public override string GetDisplayName()
        {
            return displayName;
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


        //public string GetDescription()
        //{
        //    string result = "Unknown description";
        //    if (Utils.GetMetadataForSkill(buffId) != null)
        //    {

        //        var replacements = new List<(string key, string value)> { };

        //        int i = 0;
        //        foreach (var value in amounts)
        //        {
        //            replacements.Add((i.ToString(), value.ToString()));
        //            i++;
        //        }

        //        StringBuilder templateBuild = new StringBuilder(Utils.GetMetadataForSkill(buffId).description.EN);
        //        //  result = replacements.Aggregate(templateBuild, (s, t) => s.Replace($"{{{t.key}}}%", (int.Parse(t.value) * 100f).ToString())).ToString();

        //        result = replacements.Aggregate(templateBuild, (s, t) => s.Replace($"{{{t.key}}}%", "<color=\"yellow\">" + (((float.Parse(t.value)) * 100f).ToString()) + "%</color>")).ToString();
        //        result = replacements.Aggregate(templateBuild, (s, t) => s.Replace($"{{{t.key}}}", "<color=\"yellow\">" + t.value + "</color>")).ToString();

        //        result += " Lasts for " + durationTurns.ToString() + " turns";
        //    }

        //    return result;

        //}

        //public string GetTitle()
        //{
        //    if (Utils.GetMetadataForSkill(buffId) != null)
        //        return Utils.GetMetadataForSkill(buffId).title.EN;
        //    else
        //        return buffId;

        //}
    }




    [Serializable]
    [FirestoreData]
    public class CombatSkill : Skill
    {


        //[field: SerializeField]
        //[FirestoreProperty]
        //public string skillId { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public string characterClass { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public float agroChance { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public int manaCost { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public StatsSkill stats { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public bool alreadyUsed { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int handSlotIndex { get; set; }

        // [field: SerializeField]
        //  public SkillMetadata metadata { get { return Utils.GetMetadataForSkill(skillId); } }//{ get; set; }



        //public string GetDescription()
        //{
        //    string result = "Unknown description";
        //    if (metadata != null)
        //    {
        //        var replacements = new List<(string key, string value)> { };

        //        replacements.Add(("damage", "<color=\"red\">" + stats.damage_A.ToString() + "</color>"));


        //        replacements.Add(("healSelf", "<color=\"green\">" + stats.heal_A.ToString() + "</color>"));

        //        StringBuilder templateBuild = new StringBuilder(metadata.description.EN);
        //        result = replacements.Aggregate(templateBuild, (s, t) => s.Replace($"{{{t.key}}}", t.value)).ToString();


        //    }

        //    return result;

        //}

        //public string GetTitle()
        //{
        //    if (metadata != null)
        //        return metadata.title.EN;
        //    else
        //        return skillId;

        //}
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
        public int health { get; set; }

    }



}
