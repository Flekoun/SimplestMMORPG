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

        [field: SerializeField]
        [FirestoreProperty]
        public bool isPartyLeader { get; set; }


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
