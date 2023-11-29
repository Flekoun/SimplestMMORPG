using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Firestore;
using Mono.Cecil.Cil;
using simplestmmorpg.data;
using Unity.VisualScripting;
using UnityEngine;

public class QueryData : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    private DocumentSnapshot lastDocumentSnapshot = null;

    //public bool IsSetup = false;
    //public string LocationId = "";

    //nepouzivane
    //public void Setup(string _locationId, string _pointOfInterestId)
    //{
    //    IsSetup = true;
    //    LocationId = "_metadata_zones/DUNOTAR/locations/" + _locationId + "/pointsOfInterest/" + _pointOfInterestId + "/leaderboards";
    //}

    //public void Setup()
    //{
    //    IsSetup = true;
    //    LocationId = "leaderboards";
    //}

    public Task<CharacterData> GetCharacterData(string _characterUid)
    {


        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        // Retrieve the collection from the database
        DocumentReference collection = db.Collection("characters").Document(_characterUid);

        // Asynchronously retrieve the documents
        return collection.GetSnapshotAsync().ContinueWith<CharacterData>(task =>
        {
            List<CharacterPreview> entries = new List<CharacterPreview>();
            if (task.IsCompleted)
            {
                DocumentSnapshot snapshot = task.Result;
                // Iterate through the documents in the snapshot

                return snapshot.ConvertTo<CharacterData>();
            }
            else if (task.IsFaulted)
            {
                // There was an error
                return null;
            }
            return null;
        });
    }



    public Task<List<LeaderboardScoreEntry>> GetLeaderboardEntries(string _leaderboardId, bool _nextPage = false)
    {
        //if (!IsSetup)
        //    Debug.LogError("Nastav prvni lokaci leaderboardu!");

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        // Retrieve the collection from the database
        CollectionReference collection = db.Collection("leaderboards").Document(_leaderboardId).Collection("season" + AccountDataSO.GlobalMetadata.seasonNumber);

        // Order the documents by the "characterLevel" field in ascending order and limit the results to 100 documents
        Query query = null;
        if (_nextPage && lastDocumentSnapshot != null)
            query = collection.OrderByDescending("score").Limit(AccountDataSO.OtherMetadataData.leaderboardsPageSize).StartAfter(lastDocumentSnapshot);
        else
            query = collection.OrderByDescending("score").Limit(AccountDataSO.OtherMetadataData.leaderboardsPageSize);

        // Asynchronously retrieve the documents
        return query.GetSnapshotAsync().ContinueWith<List<LeaderboardScoreEntry>>(task =>
        {
            List<LeaderboardScoreEntry> entries = new List<LeaderboardScoreEntry>();
            if (task.IsCompleted)
            {
                QuerySnapshot snapshot = task.Result;
                // Iterate through the documents in the snapshot

                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    entries.Add(document.ConvertTo<LeaderboardScoreEntry>());
                    lastDocumentSnapshot = document;
                }

                return entries;
            }
            else if (task.IsFaulted)
            {
                // There was an error
                return null;
            }
            return null;
        });
    }

    public Task<LeaderboardScoreEntry> GetMyLeaderboardEntry(string _leaderboardId)
    {

        //if (!IsSetup)
        //    Debug.LogError("Nastav prvni lokaci leaderboardu!");

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        // Retrieve the collection from the database
        DocumentReference leaderboardDoc = db.Collection("leaderboards").Document(_leaderboardId).Collection("season" + AccountDataSO.GlobalMetadata.seasonNumber).Document(AccountDataSO.CharacterData.uid);

        // Asynchronously retrieve the documents
        return leaderboardDoc.GetSnapshotAsync().ContinueWith<LeaderboardScoreEntry>(task =>
        {
            DocumentSnapshot snapshot = task.Result;

            if (snapshot.Exists)
            {
                Debug.Log("doc existis:");
                return snapshot.ConvertTo<LeaderboardScoreEntry>();

            }
            else if (task.IsFaulted)
            {
                return null;
            }
            return null;
        }
        );

    }


    public Task<LeaderboardBaseData> GetLeaderboardBaseData(string _leaderboardId, bool _nextPage = false)
    {
        //Debug.Log("_leaderboardId:" + _leaderboardId);
        //if (!IsSetup)
        //    Debug.LogError("Nastav prvni lokaci leaderboardu!");

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        // Retrieve the collection from the database
        DocumentReference leaderboardDoc = db.Collection("leaderboards").Document(_leaderboardId);

        // Asynchronously retrieve the documents
        return leaderboardDoc.GetSnapshotAsync().ContinueWith<LeaderboardBaseData>(task =>
        {
            DocumentSnapshot snapshot = task.Result;

            if (snapshot.Exists)
            {
                Debug.Log("doc existis:");
                return snapshot.ConvertTo<LeaderboardBaseData>();

            }
            else if (task.IsFaulted)
            {
                return null;
            }
            return null;
        }
        );

    }
}
