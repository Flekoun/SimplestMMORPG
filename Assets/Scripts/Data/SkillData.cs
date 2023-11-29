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
    public class QualityAmounts //: ISerializationCallbackReceiver
    {

        [field: SerializeField]
        [FirestoreProperty]
        public List<float> amounts { get; set; }

    }


    [Serializable]
    [FirestoreData]
    public class Buff //: ISerializationCallbackReceiver
    {

        [field: SerializeField]
        [FirestoreProperty]
        public string buffId { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int durationTurns { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public List<QualityAmounts> quality { get; set; }



        public string GetDescription(int _equipQuality)
        {
            string result = "Unknown description";
            if (Utils.DescriptionsMetadata.GetSkillMetadata(buffId) != null)
            {

                var replacements = new List<(string key, string value)> { };

                int i = 0;
                foreach (var value in quality[_equipQuality].amounts)
                {
                    replacements.Add((i.ToString(), value.ToString()));
                    i++;
                }

                StringBuilder templateBuild = new StringBuilder(Utils.DescriptionsMetadata.GetSkillMetadata(buffId).description.EN);
                //  result = replacements.Aggregate(templateBuild, (s, t) => s.Replace($"{{{t.key}}}%", (int.Parse(t.value) * 100f).ToString())).ToString();

                result = replacements.Aggregate(templateBuild, (s, t) => s.Replace($"{{{t.key}}}%", "<color=\"yellow\">" + (((float.Parse(t.value)) * 100f).ToString()) + "%</color>")).ToString();
                result = replacements.Aggregate(templateBuild, (s, t) => s.Replace($"{{{t.key}}}", "<color=\"yellow\">" + t.value + "</color>")).ToString();

                result += " Lasts for <color=\"yellow\">" + durationTurns.ToString() + "</color> turns.";

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
    public class Skill //: ISerializationCallbackReceiver
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

        [field: SerializeField]
        [FirestoreProperty]
        public string rarity { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public string characterClass { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public int manaCost { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public List<QualityAmounts> quality { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public Buff buff { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public bool validTarget_AnyAlly { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public bool validTarget_Self { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public bool validTarget_AnyEnemy { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public int successSlots { get; set; }



        //  [field: SerializeField]
        //    public BaseMetadata metadata { get { return Utils.GetMetadataForSkill(skillId); } }//{ get; set; }


        public string GetDescription(int _equipQuality)
        {
            string result = "Unknown description";
            if (Utils.DescriptionsMetadata.GetSkillMetadata(skillId) != null)
            {

                var replacements = new List<(string key, string value)> { };

                int i = 0;
                foreach (var value in quality[_equipQuality].amounts)
                {
                    //                    Debug.Log("quality : " + _equipQuality);
                    replacements.Add((i.ToString(), value.ToString()));
                    i++;
                }

                StringBuilder templateBuild = new StringBuilder(Utils.DescriptionsMetadata.GetSkillMetadata(skillId).description.EN);
                //  result = replacements.Aggregate(templateBuild, (s, t) => s.Replace($"{{{t.key}}}%", (int.Parse(t.value) * 100f).ToString())).ToString();

                result = replacements.Aggregate(templateBuild, (s, t) => s.Replace($"{{{t.key}}}%", "<color=\"yellow\">" + (((float.Parse(t.value)) * 100f).ToString()) + "%</color>")).ToString();
                result = replacements.Aggregate(templateBuild, (s, t) => s.Replace($"{{{t.key}}}", "<color=\"yellow\">" + t.value + "</color>")).ToString();


                if (singleUse)
                    result += "<color=\"yellow\">" + " Single Use." + "</color>";
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

        //public string GetDescription()
        //{


        //    string result = "Unknown description";
        //    if (metadata != null)
        //    {


        //        var replacements = new List<(string key, string value)>{     };

        //     //   if (stats.damage_A != null)
        //            replacements.Add(("damage", stats.damage_A.ToString()));

        //    //    if (stats.heal_A != null)
        //            replacements.Add(("healSelf", stats.heal_A.ToString()));

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


        //public void OnAfterDeserialize()
        //{
        //    //if (stats.damage != null)
        //    //{
        //    //    stats.damage.totalAmount = stats.damage.baseAmount + (stats.damage.upgradeAmount * upgradesCount);
        //    //    Debug.Log(skillId + " : " + stats.damage.totalAmount);
        //    //}
        //    //if (stats.healSelf != null)
        //    //    stats.healSelf.totalAmount = stats.healSelf.baseAmount + (stats.healSelf.upgradeAmount * upgradesCount);

        //}

        //public void OnBeforeSerialize()
        //{
        //    //if (stats.damage != null)
        //    //{
        //    //    stats.damage.totalAmount = stats.damage.baseAmount + (stats.damage.upgradeAmount * upgradesCount);
        //    //    Debug.Log(skillId + " : " + stats.damage.totalAmount);
        //    //}
        //    //if (stats.healSelf != null)
        //    //    stats.healSelf.totalAmount = stats.healSelf.baseAmount + (stats.healSelf.upgradeAmount * upgradesCount);


        //}
    }






    //[Serializable]
    //[FirestoreData]
    //public struct Stat
    //{
    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public string displayName { get; set; }
    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public int level { get; set; }
    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public int exp { get; set; }
    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public int expMax { get; set; }


    //}

    //[Serializable]
    //[FirestoreData]
    //public struct DamageToEnemy
    //{
    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public int amount { get; set; }


    //}

    //[Serializable]
    //[FirestoreData]
    //public struct HealSelf
    //{
    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public int amount { get; set; }


    //}

    //[Serializable]
    //[FirestoreData]
    //public class WarriorSlam : Skill
    //{

    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public int damageAmount { get; set; }

    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public int healSelfAmount { get; set; }
    //}

    //[Serializable]
    //[FirestoreData]
    //public class WarriorSkills
    //{

    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public WarriorSlam WarriorSlam { get; set; }

    //}
}

