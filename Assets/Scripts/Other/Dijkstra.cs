using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using Unity.VisualScripting;
using UnityEngine;

public class Dijkstra : MonoBehaviour
{
    private Graph graph;

    public DijskraResult GetShortestPath(string _start, string _end)
    {
        Debug.Log("getting shortest path from : " + _start + " to: " + _end);
        var result = graph.shortest_path(_start, _end);

        result.nodes.Add(_start); //ten dijskra algo vracti cestu od konce a jeste k tomu startovaci pozici tam nema, takze ji rucne pridam
        for (int i = 0; i < result.nodes.Count; i++)
        {


            Debug.Log(result.nodes[i] + " > ");

            foreach (var vertice in graph.vertices)
            {
                if (vertice.Key == result.nodes[i])
                {
                    Debug.Log("vertice: " + vertice.Key);

                    foreach (var node in vertice.Value)
                    {
                        if (result.nodes.Count >= i + 2)
                        {
                            if (node.Key == result.nodes[i + 1])
                            {
                                Debug.Log("node: " + node.Key + " : " + node.Value);
                                result.totalWeight += node.Value;
                            }
                        }
                    }
                }


            }
        }





        Debug.Log("timeTaken: " + result.totalWeight);

        return result;
    }

    public void FillGraph(List<DijkstraMapVertex> _dijkstraMap)
    {

        graph = new Graph();

        foreach (DijkstraMapVertex vertex in _dijkstraMap)
        {
            Dictionary<string, int> Nodes = new Dictionary<string, int>();

            foreach (var node in vertex.nodes)
            {
                Nodes.Add(node.idOfVertex, node.weight);
            }
            graph.add_vertex(vertex.id, Nodes);

        }


        //var result = graph.shortest_path("PLAINS", "VILE_DEN");


        //foreach (var item in result.nodes)
        //{
        //    Debug.Log(item + " > ");
        //}
        //Debug.Log("timeTaken: " + result.totalWeight);





        //Graph g = new Graph();
        //g.add_vertex("Seattle", new Dictionary<string, int>() { { "San Francisco", 1306 }, { "Denver", 2161 }, { "Minneapolis", 2661 } });
        //g.add_vertex("San Francisco", new Dictionary<string, int>() { { "Seattle", 1306 }, { "Las Vegas", 919 }, { "Los Angeles", 629 } });
        //g.add_vertex("Las Vegas", new Dictionary<string, int>() { { "San Francisco", 919 }, { "Los Angeles", 435 }, { "Denver", 1225 }, { "Dallas", 1983 } });
        //g.add_vertex("Los Angeles", new Dictionary<string, int>() { { "San Francisco", 629 }, { "Las Vegas", 435 } });
        //g.add_vertex("Denver", new Dictionary<string, int>() { { "Seattle", 2161 }, { "Las Vegas", 1225 }, { "Minneapolis", 1483 }, { "Dallas", 1258 } });
        //g.add_vertex("Minneapolis", new Dictionary<string, int>() { { "Seattle", 2661 }, { "Denver", 1483 }, { "Dallas", 1532 }, { "Chicago", 661 } });
        //g.add_vertex("Dallas", new Dictionary<string, int>() { { "Las Vegas", 1983 }, { "Denver", 1258 }, { "Minneapolis", 1532 }, { "Washington DC", 2113 } });
        //g.add_vertex("Chicago", new Dictionary<string, int>() { { "Minneapolis", 661 }, { "Washington DC", 1145 }, { "Boston", 1613 } });
        //g.add_vertex("Washington DC", new Dictionary<string, int>() { { "Dallas", 2113 }, { "Chicago", 1145 }, { "Boston", 725 }, { "New York", 383 }, { "Miami", 1709 } });
        //g.add_vertex("Boston", new Dictionary<string, int>() { { "Chicago", 1613 }, { "Washington DC", 725 }, { "New York", 338 } });
        //g.add_vertex("New York", new Dictionary<string, int>() { { "Washington DC", 383 }, { "Boston", 338 }, { "Miami", 2145 } });
        //g.add_vertex("Miami", new Dictionary<string, int>() { { "Dallas", 2161 }, { "Washington DC", 1709 }, { "New York", 2145 } });

        //g.shortest_path("Miami", "Seattle").ForEach(x => Debug.Log(x + " > "));
    }
}

public class DijskraResult
{
    public List<string> nodes = new List<string>();
    public int totalWeight;

}


public class Graph
{
    public Dictionary<string, Dictionary<string, int>> vertices = new Dictionary<string, Dictionary<string, int>>();

    public void add_vertex(string name, Dictionary<string, int> edges)
    {
        vertices[name] = edges;
    }

    public DijskraResult shortest_path(string start, string finish)
    {
        var previous = new Dictionary<string, string>();
        var distances = new Dictionary<string, int>();
        var nodes = new List<string>();

        DijskraResult path = new DijskraResult(); ;

        foreach (var vertex in vertices)
        {
            if (vertex.Key == start)
            {
                distances[vertex.Key] = 1;
            }
            else
            {
                distances[vertex.Key] = int.MaxValue;
            }

            nodes.Add(vertex.Key);
        }

        while (nodes.Count != 0)
        {
            nodes.Sort((x, y) => distances[x] - distances[y]);

            var smallest = nodes[0];
            nodes.Remove(smallest);

            if (smallest == finish)
            {
                // path = new DijskraResult();
                while (previous.ContainsKey(smallest))
                {

                    path.nodes.Add(smallest);

                    smallest = previous[smallest];

                }

                break;
            }

            if (distances[smallest] == int.MaxValue)
            {
                break;
            }

            foreach (var neighbor in vertices[smallest])
            {
                var alt = distances[smallest] + neighbor.Value;
                if (alt < distances[neighbor.Key])
                {
                    distances[neighbor.Key] = alt;
                    previous[neighbor.Key] = smallest;


                }
            }
        }

        return path;
    }

    //public class Graph
    //{
    //    Dictionary<string, Dictionary<string, int>> vertices = new Dictionary<string, Dictionary<string, int>>();

    //    public void add_vertex(string name, Dictionary<string, int> edges)
    //    {
    //        vertices[name] = edges;
    //    }

    //    public List<string> shortest_path(string start, string finish)
    //    {
    //        var previous = new Dictionary<string, string>();
    //        var distances = new Dictionary<string, int>();
    //        var nodes = new List<string>();

    //        List<string> path = null;

    //        foreach (var vertex in vertices)
    //        {
    //            if (vertex.Key == start)
    //            {
    //                distances[vertex.Key] = 1;
    //            }
    //            else
    //            {
    //                distances[vertex.Key] = int.MaxValue;
    //            }

    //            nodes.Add(vertex.Key);
    //        }

    //        while (nodes.Count != 0)
    //        {
    //            nodes.Sort((x, y) => distances[x] - distances[y]);

    //            var smallest = nodes[0];
    //            nodes.Remove(smallest);

    //            if (smallest == finish)
    //            {
    //                path = new List<string>();
    //                while (previous.ContainsKey(smallest))
    //                {
    //                    path.Add(smallest);
    //                    smallest = previous[smallest];
    //                }

    //                break;
    //            }

    //            if (distances[smallest] == int.MaxValue)
    //            {
    //                break;
    //            }

    //            foreach (var neighbor in vertices[smallest])
    //            {
    //                var alt = distances[smallest] + neighbor.Value;
    //                if (alt < distances[neighbor.Key])
    //                {
    //                    distances[neighbor.Key] = alt;
    //                    previous[neighbor.Key] = smallest;
    //                }
    //            }
    //        }

    //        return path;
    //    }
}
