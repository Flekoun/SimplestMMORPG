using System;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;


namespace simplestmmorpg.data
{
  
    [Serializable]
    [FirestoreData]
    public class Maps
    {
        [field: SerializeField]
        [FirestoreProperty]
        public List<LocationMap> locations { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<DijkstraMapVertex> worldMap { get; set; }

        public bool HasLocationAnyPointsOfInterest(string _locationId)
        {
            foreach (var item in locations)
            {
                if (item.locationId == _locationId)
                    return true;
            }


            return false;
        }

        public LocationMap GetLocationById(string _locationId)
        {
            foreach (var item in locations)
            {
                if (item.locationId == _locationId)
                    return item;
            }

            Debug.LogError("Cant find location: " + _locationId);

            return null;
        }
    }

    [Serializable]
    [FirestoreData]
    public class LocationMap
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string locationId { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string locationType { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public List<string> pointsOfInterest { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<DijkstraMapVertex> dijkstraMap { get; set; }



    }


    

    [Serializable]
    [FirestoreData]
    public class DijkstraMapVertex
    {
      
        [field: SerializeField]
        [FirestoreProperty]
        public string id { get; set; }
      
        [field: SerializeField]
        [FirestoreProperty]
        public List<DijkstraMapNode> nodes { get; set; }


    }

    [Serializable]
    [FirestoreData]
    public class DijkstraMapNode
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string idOfVertex { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int weight { get; set; }


    }


}

