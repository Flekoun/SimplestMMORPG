using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Firebase.Firestore;

using Unity.VisualScripting;
using UnityEngine;
using static Utils;


namespace simplestmmorpg.data
{

    [Serializable]
    [FirestoreData]
    public class LeaderboardScoreEntry
    {
        [field: SerializeField]
        [FirestoreProperty]
        public CharacterPreview character { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int score { get; set; }

    }


    [Serializable]
    [FirestoreData]
    public class LeaderboardBaseData
    {
        [field: SerializeField]
        [FirestoreProperty]
        public List<LeaderboardReward> rewards { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string timestampNextReset { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string resetInterval { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public string scoreType { get; set; }


        public LeaderboardReward GetRewardForRank(int _rank)
        {
            foreach (var item in rewards)
            {
                if (_rank >= item.rankMin && _rank <= item.rankMax)
                    return item;
            }

            return null;
        }

        public bool HasTimeToResetElapsed()
        {
            return Utils.GetNowInMillis() >= double.Parse(timestampNextReset);
        }

        //public string GetTimeToReset()
        //{
        //    double expireTimeInMilis = double.Parse(timestampNextReset);
        //    double nowInMilis = Utils.GetNowInMillis();
        //    double durationLeft = expireTimeInMilis - nowInMilis;
        //    //Debug.Log("nowInMilis: " + nowInMilis);
        //    //Debug.Log("expireTimeInMilis: " + expireTimeInMilis);
        //    //Debug.Log("durationLeft: " + durationLeft);


        //    double secondsLeft = durationLeft / 1000;
        //    double minutesLeft = durationLeft / 60000;
        //    double hoursLeft = durationLeft / 3600000;
        //    double daysLeft = durationLeft / (3600000 * 24);

        //    //return Math.Floor(daysLeft) + "d" + Math.Floor(hoursLeft) + "h : " + Math.Floor(minutesLeft) + "m :" + Math.Floor(secondsLeft) + "s";

        //    TimeSpan duration = TimeSpan.FromMilliseconds(durationLeft);

        //    string format = "";
        //    if (daysLeft > 1)
        //        format = "dd\\dhh\\hmm\\mss\\s";
        //    else if (hoursLeft > 1)
        //        format = "hh\\hmm\\mss\\s";
        //    else if (minutesLeft > 1)
        //        format = "mm\\mss\\s";
        //    else
        //        format = "ss\\s";

        //    return "resets in "+  duration.ToString(format);
        //}
    }


    [Serializable]
    [FirestoreData]
    public class LeaderboardReward
    {
        [field: SerializeField]
        [FirestoreProperty]
        public int rankMin { get; set; }
        [field: SerializeField]
        [FirestoreProperty]
        public int rankMax { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public List<ItemIdWithAmount> generatedContent { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<ContentContainer> content { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<RandomEquip> randomEquip { get; set; }


    }


}
