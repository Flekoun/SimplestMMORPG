using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace simplestmmorpg.realtimeDatabaseData
{
    public enum CHANNEL_TYPE
    {
        ZONE,
        LOCATION,
        PARTY,

    }

    public class RealtimeDatabaseChatMessageData
    {
        public string characterUid;
        public string characterName;
        public int characterLevel;
        public string body;
        public string channelName;
        public CHANNEL_TYPE channelType;
    }
}