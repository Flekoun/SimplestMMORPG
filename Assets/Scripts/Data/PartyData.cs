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
    public class Party
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string uid { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string partyLeaderUid { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int partySizeMax { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<PartyMember> partyMembers { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public DungeonProgress dungeonProgress { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public List<SimpleStringDictionary> dungeonEnterConsents { get; set; }

        public bool IsPartyLeader(string _memberUid)
        {
            return _memberUid == this.partyLeaderUid;
        }
        public bool AreAllPartyMembersOnSameLocation(string _locationId)
        {
            foreach (var member in partyMembers)
            {
                if (member.position.locationId != _locationId)
                    return false;
            }

            return true;
        }

        public int GetNumberOfConsentsToEnterDungeon(string _dungeonId)
        {
            var consents = dungeonEnterConsents.Where(entry => entry.string1 == _dungeonId).ToList();
            return consents.Count;
        }

        public bool HasPartyMemberGaveConsentToEnterDungeon(string _dungeonId, string _characterUid)
        {
            return dungeonEnterConsents.FirstOrDefault(entry => entry.string1 == _dungeonId && entry.string2 == _characterUid) != null;
        }

    }


    [Serializable]
    [FirestoreData]
    public class DungeonProgress
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string dungeonId { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int partySize { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int tierReached { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int tiersMax { get; set; }


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
        public bool isEndlessDungeon { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public bool isFinalDungeon { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int characterLevelMax { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int characterLevelMin { get; set; }


    }



    [Serializable]
    [FirestoreData]
    public class PartyMember
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string uid { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string displayName { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string characterClass { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int level { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public WorldPosition position { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public bool isPartyLeader { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public bool isOnline { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string characterPortrait { get; set; }



    }



    [Serializable]
    [FirestoreData]
    public class PartyInvite
    {
        //[field: SerializeField]
        //[FirestoreProperty]
        //public string uid { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string partyLeaderUid { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string partyLeaderDisplayName { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string invitedCharacterUid { get; set; }

    }

}
