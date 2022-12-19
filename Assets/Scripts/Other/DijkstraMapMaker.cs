using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class DijkstraMapMaker : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public UILineMaker AllPathsUILineMaker;
    public UILineMaker PlannedPathsUILineMaker;
    public Dijkstra Dijkstra;
    public DijskraResult PlannedPathNewest;
    public UnityAction<ScreenPoisitionWihtId> OnVertexReachable;
    private List<DijkstraMapVertex> Data;

    public void ClearPlannedTravelPath()
    {
        PlannedPathsUILineMaker.DestroyAllLines();
    }

    private ScreenPoisitionWihtId GetScreenPosition(List<ScreenPoisitionWihtId> _positionsList, string _id)
    {
        foreach (var item in _positionsList)
        {
            if (item.id == _id)
                return item;
        }

        Debug.LogWarning("SPATNE NASTAVENA DIJKSTRA ASI : " + _id);
        return null;
    }

    // Start is called before the first frame update
    public void Setup(List<DijkstraMapVertex> _data, List<ScreenPoisitionWihtId> _positionsList)
    {

        Data = _data;
        //  List<DijkstraMapVertex> locationMap = Data;
        AllPathsUILineMaker.DestroyAllLines();

        Dijkstra.FillGraph(Data);

        foreach (var vertex in Data)
        {

            foreach (var node in vertex.nodes)
            {
                //foreach (var item in _data)
                //{
                //    Debug.Log("vertex.nodes: " +item.id);
                //}

                //foreach (var item in _positionsList)
                //{
                //    Debug.Log("_positionsList: " + item.id);
                //}


                if (_positionsList.Count == 0)
                {
                    Debug.LogWarning("TEHRE ARE NO SCREEN POSITION DATA LOADED!");
                    return;
                }

                var startDefinition = GetScreenPosition(_positionsList, vertex.id);
                var targetDefinition = GetScreenPosition(_positionsList, node.idOfVertex);

                //pouze pokud je startovni pozice prozkoumana ukazem dijkstra cestu z nej
                if (AccountDataSO.IsPositionExplored(startDefinition.id))
                {
                  //  Debug.Log("ME IDJE: " + startDefinition.id);
                    Vector2 start = startDefinition.screenPosition.ToVector2();
                    Vector2 target = targetDefinition.screenPosition.ToVector2();

                    AllPathsUILineMaker.MakeLineFromPrefab(start.x, start.y, target.x, target.y, node.weight);

                    OnVertexReachable.Invoke(startDefinition);
                    OnVertexReachable.Invoke(targetDefinition);

                }
                //else if (AccountDataSO.IsInDungeon())
                //{
                //    if (AccountDataSO.PartyData.dungeonProgress.IsPositionExplored(startDefinition.Id))
                //    {
                //        Vector2 start = (startDefinition as IHasScreenPosition).GetScreenPosition();
                //        Vector2 target = (targetDefinition as IHasScreenPosition).GetScreenPosition();

                //        AllPathsUILineMaker.MakeLineFromPrefab(start.x, start.y, target.x, target.y, node.weight);

                //        OnVertexReachable.Invoke(startDefinition);
                //        OnVertexReachable.Invoke(targetDefinition);
                //    }
                //}
            }

        }

    }

    public void ShowPlannedTravelPath(string _start, string _finish, List<ScreenPoisitionWihtId> _positionsList)
    {
        PlannedPathsUILineMaker.DestroyAllLines();

        PlannedPathNewest = Dijkstra.GetShortestPath(_start, _finish);

        for (int i = 0; i < PlannedPathNewest.nodes.Count; i++)
        {
            ////jen kontrola na pritomnost interface
            //if (!(_definitionSetWithScreenPositions.GetDefinitionById(PlannedPathNewest.nodes[i]) is IHasScreenPosition))
            //{
            //    Debug.LogError("Given set does not implements HasScreenPosition, therefore I cant draw its dijkstra!");
            //    return;
            //}

            Vector2 start = GetScreenPosition(_positionsList, PlannedPathNewest.nodes[i]).screenPosition.ToVector2(); //(_definitionSetWithScreenPositions.GetDefinitionById(PlannedPathNewest.nodes[i]) as IHasScreenPosition).GetScreenPosition();
            if (PlannedPathNewest.nodes.Count >= i + 2)
            {
                Vector2 target = GetScreenPosition(_positionsList, PlannedPathNewest.nodes[i + 1]).screenPosition.ToVector2(); // (_definitionSetWithScreenPositions.GetDefinitionById(PlannedPathNewest.nodes[i + 1]) as IHasScreenPosition).GetScreenPosition();
                PlannedPathsUILineMaker.MakeLineFromPrefab(start.x, start.y, target.x, target.y, 0);
            }

        }


    }
}
