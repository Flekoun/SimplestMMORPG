using System;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;


namespace simplestmmorpg.data
{

    //[Serializable]
    //[FirestoreData]
    //public class Maps
    //{
    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public List<Location> locations { get; set; }

    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public List<DijkstraMapVertex> worldMap { get; set; }

    //    public bool HasLocationAnyPointsOfInterest(string _locationId)
    //    {
    //        foreach (var item in locations)
    //        {
    //            if (item.id == _locationId)
    //                return true;
    //        }


    //        return false;
    //    }

    //    public Location GetLocationById(string _locationId)
    //    {
    //        foreach (var item in locations)
    //        {
    //            if (item.id == _locationId)
    //                return item;
    //        }

    //        Debug.LogError("Cant find location: " + _locationId);

    //        return null;
    //    }
    //}



    [Serializable]
    [FirestoreData]
    public class ScreenPoisitionWihtId
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string id { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public ScreenPosition screenPosition { get; set; }
    }

    [Serializable]
    [FirestoreData]
    public class Zone
    {
        [field: SerializeField]
        [FirestoreProperty]
        public List<DijkstraMapVertex> dijkstraMap { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<ScreenPoisitionWihtId> locationScreenPositions { get; set; }

        public ScreenPosition GetScreenPositionForLocationId(string _locationId)
        {
            foreach (var item in locationScreenPositions)
            {
                if (item.id == _locationId)
                    return item.screenPosition;
            }

            return null;
        }
    }

    [Serializable]
    [FirestoreData]
    public class Location
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string id { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string locationType { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<PointOfInterest> pointsOfInterest { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<DijkstraMapVertex> dijkstraMap { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public ScreenPosition screenPosition { get; set; }

        public PointOfInterest GetPointOfInterestById(string _id)
        {
            foreach (var item in pointsOfInterest)
            {
                if (item.id == _id)
                    return item;
            }

            Debug.LogError("No point of interesst with ID exists : " + _id);
            return null;
        }



        public Vector2 GetScreenPosition()
        {
            return screenPosition.ToVector2();
        }

        public List<ScreenPoisitionWihtId> GetScreenPositionsWithIds()
        {
            List<ScreenPoisitionWihtId> posList = new List<ScreenPoisitionWihtId>();

            foreach (var poi in pointsOfInterest)
            {
                var pos = new ScreenPoisitionWihtId();
                pos.id = poi.id;
                pos.screenPosition = poi.screenPosition;
                posList.Add(pos);
            }

            return posList;

        }
    }


    [Serializable]
    [FirestoreData]
    public class PointOfInterest
    {

        [field: SerializeField]
        [FirestoreProperty]
        public string id { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public ??? enemies { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public int exploreTimePrice { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string pointOfInterestType { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<QuestgiverMeta> questgivers { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<Vendor> vendors { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public List<Trainer> trainers { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<string> specials { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public ScreenPosition screenPosition { get; set; }

        public Vector2 GetScreenPosition()
        {
            return screenPosition.ToVector2();
        }
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
    [Serializable]
    [FirestoreData]
    public class ScreenPosition
    {
        [field: SerializeField]
        [FirestoreProperty]
        public float x { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public float y { get; set; }


        public Vector2 ToVector2()
        {
            return new Vector2(x, y);
        }

    }


}

