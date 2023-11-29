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



    //[Serializable]
    //[FirestoreData]
    //public class ScreenPoisitionWihtId
    //{
    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public string id { get; set; }

    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public ScreenPosition screenPosition { get; set; }
    //}




    [Serializable]
    [FirestoreData]
    public class Zone
    {
        //[field: SerializeField]
        //[FirestoreProperty]
        //public List<DijkstraMapVertex> dijkstraMap { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public DijkstraMap dijkstraMap { get; set; }


        public DijkstraMapVertex GetDijkstraMapVertexById(string _id)
        {
            foreach (var item in dijkstraMap.exportMap)
            {
                if (item.id == _id)
                    return item;
            }

            Debug.LogWarning("SPATNE NASTAVENA DIJKSTRA ASI : " + _id);
            return null;
        }

        //public ScreenPosition GetScreenPositionForLocationId(string _locationId)
        //{
        //    foreach (var item in locationScreenPositions)
        //    {
        //        if (item.id == _locationId)
        //            return item.screenPosition;
        //    }

        //    return null;
        //}
    }

    [Serializable]
    [FirestoreData]
    public class DijkstraMap
    {
        [field: SerializeField]
        [FirestoreProperty]
        public List<DijkstraMapVertex> exportMap { get; set; }
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
        public DijkstraMap dijkstraMap { get; set; }
        // public List<DijkstraMapVertex> dijkstraMap { get; set; }




        [field: SerializeField]
        [FirestoreProperty]
        public string graveyard { get; set; }

        //public PointOfInterest GetPointOfInterestById(string _id)
        //{
        //    foreach (var item in pointsOfInterest)
        //    {
        //        if (item.id == _id)
        //            return item;
        //    }

        //    Debug.LogError("No point of interesst with ID exists : " + _id);
        //    return null;
        //}

        public DijkstraMapVertex GetDijkstraMapVertexById(string _id)
        {
            //            Debug.LogWarning("GetDijkstraMapVertexById : " + _id);
            foreach (var item in dijkstraMap.exportMap)
            {
                if (item.id == _id)
                    return item;
            }

            //            Debug.LogWarning("SPATNE NASTAVENA DIJKSTRA ASI : " + _id);
            return null;
        }


        //public Vector2 GetScreenPosition()
        //{
        //    return screenPosition.ToVector2();
        //}

        //public List<ScreenPoisitionWihtId> GetScreenPositionsWithIds()
        //{
        //    List<ScreenPoisitionWihtId> posList = new List<ScreenPoisitionWihtId>();

        //    foreach (var poi in pointsOfInterest)
        //    {
        //        var pos = new ScreenPoisitionWihtId();
        //        pos.id = poi.id;
        //        pos.screenPosition = poi.screenPosition;
        //        posList.Add(pos);
        //    }

        //    return posList;

        //}
    }


    //[Serializable]
    //[FirestoreData]
    //public class RewardRecurring
    //{

    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public int recurrenceInGameDays { get; set; }

    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public List<ContentContainer> rewards { get; set; }

    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public int charges { get; set; }

    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public int rewardAtSpecificGameDay { get; set; }

    //}


    [Serializable]
    [FirestoreData]
    public class DungeonDefinitionPublic
    {

        [field: SerializeField]
        [FirestoreProperty]
        public int partySize { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int entryPrice { get; set; }

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

        [field: SerializeField]
        [FirestoreProperty]
        public int floorsTotal { get; set; }


    }

    [Serializable]
    [FirestoreData]
    public class MonstersDefinitionPublic
    {

        [field: SerializeField]
        [FirestoreProperty]
        public int partySize { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int entryTimePrice { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public int tiersTotal { get; set; }


    }



    [Serializable]
    [FirestoreData]
    public class PointOfInterest
    {

        [field: SerializeField]
        [FirestoreProperty]
        public string id { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<IdWithChance> enemies { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string typeId { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public int tiersCount { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public int exploreTimePrice { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int pointOfInterestType { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<Questgiver> questgivers { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<Vendor> vendors { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public List<Trainer> trainers { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<string> specials { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public int maxCombatants { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int floorNumber { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public DungeonDefinitionPublic dungeon { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public MonstersDefinitionPublic monsters { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string roomType { get; set; }

        public List<Questgiver> GetValidQuestGivers(CharacterData _characterData)
        {
            //TADY SE FILTRUJOU QUEST GIVERI...DOBRE MISTO?!!

            //vyfiltruju questgivery ktere jsem uz splnil pryc.....
            var tempListToBeFiltered = new List<Questgiver>(questgivers);

            for (int i = tempListToBeFiltered.Count - 1; i >= 0; i--)
            {
                if (_characterData.questgiversClaimed.Contains(tempListToBeFiltered[i].id))
                    tempListToBeFiltered.RemoveAt(i);
            }


            //vyfiltruju questgivery podle prereQestu.....
            for (int i = tempListToBeFiltered.Count - 1; i >= 0; i--)
            {
                int prereqsFound = 0;

                foreach (var item in tempListToBeFiltered[i].prereqQuests)
                {
                    if (_characterData.questgiversClaimed.Contains(item))
                        prereqsFound++;
                }

                if (prereqsFound < tempListToBeFiltered[i].prereqQuests.Count)
                {
                    //    Debug.Log("Fillteriung out : " + tempListToBeFiltered[i].uid + " prereqsQuestsFound:  " + prereqsFound + " needed : " + tempListToBeFiltered[i].prereqQuests.Count);
                    tempListToBeFiltered.RemoveAt(i);
                }
            }


            //vyfiltruju questgivery podle prereqPoI.....
            //for (int i = tempListToBeFiltered.Count - 1; i >= 0; i--)
            //{
            //    int prereqsFound = 0;

            //    foreach (var item in tempListToBeFiltered[i].prereqExploredPointsOfInterest)
            //    {
            //        if (_characterData.exploredPositions.pointsOfInterest.Contains(item))
            //            prereqsFound++;
            //    }

            //    if (prereqsFound < tempListToBeFiltered[i].prereqExploredPointsOfInterest.Count)
            //    {
            //        //       Debug.Log("Fillteriung out : " + tempListToBeFiltered[i].uid + " prereqsPoIFound:  " + prereqsFound + " needed : " + tempListToBeFiltered[i].prereqExploredPointsOfInterest.Count);
            //        tempListToBeFiltered.RemoveAt(i);
            //    }
            //}

            //vyfiltruju questgivery podle levelu.....
            //for (int i = tempListToBeFiltered.Count - 1; i >= 0; i--)
            //{
            //    if (tempListToBeFiltered[i].minLevel > _characterData.stats.level)
            //    {
            //        tempListToBeFiltered.RemoveAt(i);
            //    }
            //}

            return tempListToBeFiltered;
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

        [field: SerializeField]
        [FirestoreProperty]
        public Coordinates2DCartesian screenPosition { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public Coordinates2DCartesian mapPosition { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int type { get; set; }


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
    public class Coordinates2DCartesian
    {
        [field: SerializeField]
        [FirestoreProperty]
        public int x { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int y { get; set; }


        public Vector2 ToVector2()
        {
            return new Vector2(x, y);
        }
    }


}

