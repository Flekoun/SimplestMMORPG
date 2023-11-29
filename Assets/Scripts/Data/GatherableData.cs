using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Firebase.Firestore;

using Unity.VisualScripting;
using UnityEngine;

namespace simplestmmorpg.data
{

    [Serializable]
    [FirestoreData]
    public class Gatherable
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string uid { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<SimpleTally> professionNeeded { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string gatherableType { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public WorldPosition position { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string rarity { get; set; }


        public bool HasEnoughtSkillToGatherThis(List<SimpleTallyWithMax> _professionsSkills)
        {
            foreach (var profNeeded in professionNeeded)
            {
                bool hasEnoughtSkill = false;
                foreach (var profHave in _professionsSkills)
                {
                    if (profHave.id == profNeeded.id)
                        if (profHave.count >= profNeeded.count)
                            hasEnoughtSkill = true;
                }

                if (!hasEnoughtSkill)
                    return false;
            }

            return true;
        }

        public string SkillsNeededToGatherThis()
        {
            string result = "";

            foreach (var item in professionNeeded)
            {
                result += item.count + " " + item.id +" ";
            }

            return result;
        }
    }


}
