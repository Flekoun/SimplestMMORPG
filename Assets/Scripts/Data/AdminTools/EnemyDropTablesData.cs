using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Firestore;
using simplestmmorpg.data;
using UnityEngine;

namespace simplestmmorpg.adminToolsData
{

    [Serializable]
    [FirestoreData]
    public class EnemyDropTablesData
    {
        [field: SerializeField]
        [FirestoreProperty]
        public List<EnemyDropTable> enemyDropTables { get; set; }
    }

    [Serializable]
    [FirestoreData]
    public class EnemyDropTable
    {
        [field: SerializeField]
        [FirestoreProperty]
        public List<DropTable> dropTables { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string enemyId { get; set; }
    }

    [Serializable]
    [FirestoreData]
    public class DropTable
    {
        [field: SerializeField]
        [FirestoreProperty]
        public int dropCountMax { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int dropCountMin { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<DropTableItem> dropTableItems { get; set; }
    }

    [Serializable]
    [FirestoreData]
    public class DropTableItem
    {
        [field: SerializeField]
        [FirestoreProperty]
        public int amount { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public double chanceToSpawn { get; set; } 

        //[field: SerializeField]
        //[FirestoreProperty]
        //public string contentType { get; set; } // Nullable for optional

        [field: SerializeField]
        [FirestoreProperty]
        public string itemId { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string rarity { get; set; } // Nullable for optional



        public virtual string GetImageId()
        {
            if (Utils.DescriptionsMetadata.DoesDescriptionMetadataForIdExist(this.itemId))
                return Utils.DescriptionsMetadata.GetDescriptionMetadataForId(this.itemId).imageId;

            return string.Empty;//"No Metadata for " + this.GetItemId();

        }
    }


}
