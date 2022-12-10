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
    public UnityAction<BaseIdDefinition> OnVertexReachable;
    private List<DijkstraMapVertex> Data;

    public void ClearPlannedTravelPath()
    {
        PlannedPathsUILineMaker.DestroyAllLines();
    }

    // Start is called before the first frame update
    public void Setup(List<DijkstraMapVertex> _data, BaseDefinitionSOSet _definitionSetWithScreenPositions)
    {

        Data = _data;
        //  List<DijkstraMapVertex> locationMap = Data;
        AllPathsUILineMaker.DestroyAllLines();

        Dijkstra.FillGraph(Data);

        foreach (var vertex in Data)
        {

            foreach (var node in vertex.nodes)
            {
                //jen kontrola na pritomnost interface
                if (!(_definitionSetWithScreenPositions.GetDefinitionById(vertex.id) is IHasScreenPosition))
                {
                    Debug.LogError("Given set does not implements HasScreenPosition, therefore I cant draw its dijkstra!");
                    return;
                }

                var startDefinition = _definitionSetWithScreenPositions.GetDefinitionById(vertex.id);
                var targetDefinition = _definitionSetWithScreenPositions.GetDefinitionById(node.idOfVertex);

                //pouze pokud je startovni pozice prozkoumana ukazem dijkstra cestu z nej
                if (AccountDataSO.CharacterData.IsPositionExplored(startDefinition.Id))
                {
                    Vector2 start = (startDefinition as IHasScreenPosition).GetScreenPosition();
                    Vector2 target = (targetDefinition as IHasScreenPosition).GetScreenPosition();

                    AllPathsUILineMaker.MakeLineFromPrefab(start.x, start.y, target.x, target.y, node.weight);

                    OnVertexReachable.Invoke(startDefinition);
                    OnVertexReachable.Invoke(targetDefinition);

                }
            }

        }

    }

    public void ShowPlannedTravelPath(string _start, string _finish, BaseDefinitionSOSet _definitionSetWithScreenPositions)
    {
        PlannedPathsUILineMaker.DestroyAllLines();

        PlannedPathNewest = Dijkstra.GetShortestPath(_start, _finish);

        for (int i = 0; i < PlannedPathNewest.nodes.Count; i++)
        {
            //jen kontrola na pritomnost interface
            if (!(_definitionSetWithScreenPositions.GetDefinitionById(PlannedPathNewest.nodes[i]) is IHasScreenPosition))
            {
                Debug.LogError("Given set does not implements HasScreenPosition, therefore I cant draw its dijkstra!");
                return;
            }

            Vector2 start = (_definitionSetWithScreenPositions.GetDefinitionById(PlannedPathNewest.nodes[i]) as IHasScreenPosition).GetScreenPosition();
            if (PlannedPathNewest.nodes.Count >= i + 2)
            {
                Vector2 target = (_definitionSetWithScreenPositions.GetDefinitionById(PlannedPathNewest.nodes[i + 1]) as IHasScreenPosition).GetScreenPosition();
                PlannedPathsUILineMaker.MakeLineFromPrefab(start.x, start.y, target.x, target.y, 0);
            }

        }


    }
}
