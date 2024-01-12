using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using UnityEngine.Events;

public class DijkstraMapMaker : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public UILineMaker AllPathsUILineMaker;
    public UILineMaker PlannedPathsUILineMaker;
    public Dijkstra Dijkstra;
    public DijskraResult PlannedPathNewest;
    public UnityAction<DijkstraMapVertex> OnVertexReachable;
    private List<DijkstraMapVertex> Data;
    // private List<WorldPosition> ExploredPositions;
    private string LastLocation = "";
    public struct PlannedPathCost
    {
        public int TravelPointsCost;
        public int TimeCost;
    }

    public PlannedPathCost GetPlannedPathCost()
    {
        if (PlannedPathNewest != null)
        {
            int timePerTravelPoint = AccountDataSO.OtherMetadataData.constants.timePerTravelPoint;
            int playerTravelPointAmount = Mathf.FloorToInt(AccountDataSO.CharacterData.currency.travelPoints);

            int stockOfTravelTimeInsideTravelPoints = playerTravelPointAmount * timePerTravelPoint;
            int lefotverPriceInTimeToPay = (PlannedPathNewest.totalWeight * timePerTravelPoint) - stockOfTravelTimeInsideTravelPoints;

            var result = new PlannedPathCost();

            if (lefotverPriceInTimeToPay > 0)
            {
                result.TravelPointsCost = playerTravelPointAmount;
                result.TimeCost = lefotverPriceInTimeToPay;

            }
            else
            {
                result.TravelPointsCost = PlannedPathNewest.totalWeight;
                result.TimeCost = 0;
            }

            return result;
        }
        else
        {
            var result = new PlannedPathCost();
            result.TimeCost = 0;
            result.TravelPointsCost = 0;
            return result;
        }
    }

    public void ClearPlannedTravelPath()
    {
        PlannedPathsUILineMaker.DestroyAllLines();
    }

    private DijkstraMapVertex GetVertexById(string _id)
    {
        foreach (var item in Data)
        {
            if (item.id == _id)
                return item;
        }

        Debug.LogWarning("SPATNE NASTAVENA DIJKSTRA ASI : " + _id);
        return null;
    }

    // Start is called before the first frame update
    public void SetupForCurrentLocation(bool _hardRefresh = false)//, List<ScreenPoisitionWihtId> _positionsList)
    {
        //pokud je uz nastavena, ignoruju;
        //if (LastLocation == AccountDataSO.LocationData.id)
        //    return;

        Data = new List<DijkstraMapVertex>();
        foreach (var vertex in AccountDataSO.LocationData.dijkstraMap.exportMap)
        {
            var vertexCopy = new DijkstraMapVertex();
            vertexCopy.id = vertex.id;
            vertexCopy.nodes = new List<DijkstraMapNode>();
            vertexCopy.screenPosition = new Coordinates2DCartesian();
            vertexCopy.screenPosition.x = vertex.screenPosition.x;
            vertexCopy.screenPosition.y = vertex.screenPosition.y;

            foreach (var node in vertex.nodes)
            {
                var nodeCopy = new DijkstraMapNode();
                nodeCopy.idOfVertex = node.idOfVertex;
                nodeCopy.weight = node.weight;
                vertexCopy.nodes.Add(nodeCopy);
            }

            Data.Add(vertexCopy);

        }

        if (LastLocation == AccountDataSO.LocationData.id && !_hardRefresh)
            return;

        AllPathsUILineMaker.DestroyAllLines();

        Dijkstra.FillGraph(Data);

        foreach (var vertex in Data)
        {
            foreach (var node in vertex.nodes)
            {
                var targetMapVertexDefinition = GetVertexById(node.idOfVertex);

                //pouze pokud je startovni pozice prozkoumana ukazem dijkstra cestu z nej
                var worldPosition = new WorldPosition();
                worldPosition.pointOfInterestId = vertex.id;
                worldPosition.locationId = AccountDataSO.CharacterData.position.locationId;
                worldPosition.zoneId = AccountDataSO.CharacterData.position.zoneId;

                //if (AccountDataSO.IsPositionExplored(worldPosition))
                //{

                AllPathsUILineMaker.MakeLineFromPrefab(vertex.screenPosition.x, vertex.screenPosition.y, targetMapVertexDefinition.screenPosition.x, targetMapVertexDefinition.screenPosition.y, node.weight);

                OnVertexReachable?.Invoke(vertex);
                OnVertexReachable?.Invoke(targetMapVertexDefinition);

                //}
            }

        }

        LastLocation = AccountDataSO.LocationData.id;

    }

    public void SetupForZone(List<DijkstraMapVertex> _data, string _zone)//, List<ScreenPoisitionWihtId> _positionsList)
    {

        Data = _data;
        //  List<DijkstraMapVertex> locationMap = Data;
        AllPathsUILineMaker.DestroyAllLines();

        Dijkstra.FillGraph(Data);

        foreach (var vertex in Data)
        {
            if (AccountDataSO.CharacterData.IsLocationExplored(vertex.id))
            {
                OnVertexReachable.Invoke(vertex);
            }
            //foreach (var node in vertex.nodes)
            //{



            //    //if (_positionsList.Count == 0)
            //    //{
            //    //    Debug.LogWarning("TEHRE ARE NO SCREEN POSITION DATA LOADED!");
            //    //    return;
            //    //}

            //    //var startDefinition = GetScreenPosition(_positionsList, vertex.id);
            //    var targetMapVertexDefinition = GetVertexById(node.idOfVertex);

            //    //pouze pokud je startovni pozice prozkoumana ukazem dijkstra cestu z nej

            //    if (AccountDataSO.CharacterData.IsLocationExplored(vertex.id))
            //    {
            //        //  Debug.Log("ME IDJE: " + startDefinition.id);
            //        //Vector2 start = startDefinition.screenPosition.ToVector2();
            //        //Vector2 target = targetDefinition.screenPosition.ToVector2();

            //        AllPathsUILineMaker.MakeLineFromPrefab(vertex.screenPosition.x, vertex.screenPosition.y, targetMapVertexDefinition.screenPosition.x, targetMapVertexDefinition.screenPosition.y, node.weight);


            //    OnVertexReachable.Invoke(vertex);
            //    OnVertexReachable.Invoke(targetMapVertexDefinition);

            //      }

            //}

        }

    }

    public List<string> GetReacheableVertices()
    {
        if (Data == null)
        {
            Debug.LogWarning("DIJKSTRA data nebyly nastaveny jeste ale uz chces reachavle vertexy? Volatm teda naplneni dat ");
            SetupForCurrentLocation();
        }

        var exploredMap = new List<DijkstraMapVertex>();

        foreach (var vertex in Data)
        {
            foreach (var exploredPosition in AccountDataSO.CharacterData.exploredPositions)
            {
                if (vertex.id == exploredPosition.pointOfInterestId) //pokud je vertex v prozkoumanych tak je jasny explored
                {
                    if (!exploredMap.Contains(vertex))
                    {
                        exploredMap.Add(vertex);
                        //   Debug.Log("Pridavam : " + vertex.id);
                    }

                    //pridame jeste jeho neibory
                    foreach (var node in vertex.nodes)
                    {
                        //  Debug.Log("Neighbora  : " + node.idOfVertex);
                        foreach (var vertex2 in Data)
                        {
                            if (vertex2.id == node.idOfVertex)
                            {
                                //    Debug.Log("Jsem nasel na mape  ....");
                                if (!exploredMap.Contains(vertex2))
                                {
                                    //     Debug.Log("Pridavam");
                                    exploredMap.Add(vertex2);
                                }
                                //else
                                //{
                                //    Debug.Log("Uz je tam nepridavam");
                                //}
                            }
                        }
                    }

                }
            }

        }
        //ulozim si vsechny id vsech vertexu abych mohl rychle kontrolovat jeke tam jsou
        List<string> allVertexIds = new List<string>();
        foreach (var vertex in exploredMap)
        {
            allVertexIds.Add(vertex.id);
        }

        //   Debug.Log("reachable vertex count : " + allVertexIds.Count);
        return allVertexIds;
    }

    public void ShowPlannedTravelPath(string _start, string _finish)
    {
        const int ABSURD_WEIGHT = 9999999;

        PlannedPathsUILineMaker.DestroyAllLines();

        //Vytvorim si custom Dijkstru jen z node ktere mam prozkoumne


        var exploredMap = new List<DijkstraMapVertex>();

        foreach (var vertex in Data)
        {
            foreach (var exploredPosition in AccountDataSO.CharacterData.exploredPositions)
            {
                if (vertex.id == exploredPosition.pointOfInterestId) //pokud je vertex v prozkoumanych tak je jasny explored
                {
                    if (!exploredMap.Contains(vertex))
                    {
                        exploredMap.Add(vertex);
                        //Debug.Log("Pridavam : " + vertex.id);
                    }

                    //pridame jeste jeho neibory
                    foreach (var node in vertex.nodes)
                    {
                        // Debug.Log("Neighbora  : " + node.idOfVertex);
                        foreach (var vertex2 in Data)
                        {
                            if (vertex2.id == node.idOfVertex)
                            {
                                //   Debug.Log("Jsem nasel na mape  ....");
                                if (!exploredMap.Contains(vertex2))
                                {
                                    //      Debug.Log("Pridavam");
                                    exploredMap.Add(vertex2);
                                }
                                //else
                                //{
                                //    Debug.Log("Uz je tam nepridavam");
                                //}
                            }
                        }
                    }

                }
            }

        }

        //ted jeste projdu vsechny vertexy ....pokud dany vertext neni prozkoumana, smazu mu veskere nody ( nevede smerem od nej cesta nikamn, jedina cesta je z prozkoumanych na ne)
        foreach (var vertex in exploredMap)
        {
            bool isExplored = false;
            foreach (var exploredPosition in AccountDataSO.CharacterData.exploredPositions)
            {
                if (vertex.id == exploredPosition.pointOfInterestId)
                    isExplored = true;
            }

            if (!isExplored)
            {
                //   Debug.Log(vertex.id + " neni prozkoumany, smazavam mu vsechny nody...");
                foreach (var node in vertex.nodes)
                {
                    node.weight = ABSURD_WEIGHT;
                }
                // vertex.nodes.Clear();
            }
        }


        //ulozim si vsechny id vsech vertexu abych mohl rychle kontrolovat jeke tam jsou
        List<string> allVertexIds = new List<string>();
        foreach (var vertex in exploredMap)
        {
            allVertexIds.Add(vertex.id);
        }

        //odstranim proste z mapy vertexy ktere nemam jako dostupne ziskane (kod shora)
        foreach (var vertex in exploredMap)
        {
            for (int i = vertex.nodes.Count - 1; i >= 0; i--)
            {
                if (!allVertexIds.Contains(vertex.nodes[i].idOfVertex))
                {
                    //    Debug.Log("ostranuji: " + vertex.nodes[i].idOfVertex + " z " + vertex.id);

                    vertex.nodes.RemoveAt(i);
                }
            }

        }

        Dijkstra.FillGraph(exploredMap);

        PlannedPathNewest = Dijkstra.GetShortestPath(_start, _finish);

        //ok takze ten sebilni client algo dijkstra vraci cestu od konce...jak debil...takze kdyz cestuju na unexplored je ta prvni vaha 99999 ....takze udelam tu hrozny hack....
        if (PlannedPathNewest.totalWeight >= ABSURD_WEIGHT)
        {
            PlannedPathNewest.totalWeight -= ABSURD_WEIGHT;
            PlannedPathNewest.totalWeight += 1;
        }

        for (int i = 0; i < PlannedPathNewest.nodes.Count; i++)
        {

            Vector2 start = GetVertexById(PlannedPathNewest.nodes[i]).screenPosition.ToVector2(); //(_definitionSetWithScreenPositions.GetDefinitionById(PlannedPathNewest.nodes[i]) as IHasScreenPosition).GetScreenPosition();
            if (PlannedPathNewest.nodes.Count >= i + 2)
            {
                Vector2 target = GetVertexById(PlannedPathNewest.nodes[i + 1]).screenPosition.ToVector2(); // (_definitionSetWithScreenPositions.GetDefinitionById(PlannedPathNewest.nodes[i + 1]) as IHasScreenPosition).GetScreenPosition();
                PlannedPathsUILineMaker.MakeLineFromPrefab(start.x, start.y, target.x, target.y, 0);
            }

        }

    }
}
