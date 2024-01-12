using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using simplestmmorpg.data;
using static Utils;
using System.Linq;

public class MapGenerator : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public FirebaseCloudFunctionSO_Admin FirebaseCloudFunctionSO_Admin;

    public int Width = 7;
    public int Height = 15;
    public int NumPaths = 6;
    //public int EndGameRoomLimit = 4;

    // Define probabilities for each room type
    public Dictionary<ROOM_TYPE, float> roomTypeProbabilities = new Dictionary<ROOM_TYPE, float>
    {
        { ROOM_TYPE.MONSTER_SOLO, 0.2f },
        { ROOM_TYPE.MONSTER_COOP, 0.2f },
                 { ROOM_TYPE.MONSTER_ELITE, 0.2f },

         { ROOM_TYPE.CHAPEL, 0.2f },

               { ROOM_TYPE.MERCHANT, 0.6f },
                 { ROOM_TYPE.QUEST, 0.2f },
                    { ROOM_TYPE.REST, 0.2f },
                     { ROOM_TYPE.TOWN, 0.8f },
                        { ROOM_TYPE.TREASURE, 0.2f },
                           { ROOM_TYPE.DUNGEON, 1f },
    };

    // Define maximum limit for each room type
    public Dictionary<ROOM_TYPE, int> roomTypeMaxCount = new Dictionary<ROOM_TYPE, int>
    {
        { ROOM_TYPE.MONSTER_SOLO, 100 },
        { ROOM_TYPE.MONSTER_COOP, 0 },
        { ROOM_TYPE.MONSTER_ELITE, 0 },
        { ROOM_TYPE.CHAPEL, 0 },
        { ROOM_TYPE.MERCHANT, 16 },
        { ROOM_TYPE.QUEST, 0 },
        { ROOM_TYPE.REST, 0 },
        { ROOM_TYPE.TOWN,6},
        { ROOM_TYPE.TREASURE,2 },
        { ROOM_TYPE.DUNGEON,2 },
    };

    private Dictionary<ROOM_TYPE, bool> roomTypeCanRepeat = new Dictionary<ROOM_TYPE, bool>
{
           { ROOM_TYPE.ENDGAME,true },
    { ROOM_TYPE.MONSTER_SOLO, true },
        { ROOM_TYPE.MONSTER_COOP, true },
                 { ROOM_TYPE.MONSTER_ELITE, false },

         { ROOM_TYPE.CHAPEL, false },

               { ROOM_TYPE.MERCHANT, false },
                 { ROOM_TYPE.QUEST, false },
                    { ROOM_TYPE.REST, false },
                     { ROOM_TYPE.TOWN,false},
                        { ROOM_TYPE.TREASURE,false },
                                                { ROOM_TYPE.DUNGEON,false },
};

    // Track current count for each room type
    private Dictionary<ROOM_TYPE, int> currentRoomTypeCount = new Dictionary<ROOM_TYPE, int>();

    // public enum RoomType { None, Monster, Treasure, Rest, Elite, Merchant, Boss }

    private ROOM_TYPE[,] map;//= new RoomType[Width, Height];


    private DijkstraMap mapData;


    ROOM_TYPE RandomRoomType(int floor, int x, int y)
    {
        var types = new List<ROOM_TYPE> { ROOM_TYPE.MONSTER_SOLO, ROOM_TYPE.MONSTER_COOP, ROOM_TYPE.REST, ROOM_TYPE.MERCHANT, ROOM_TYPE.TOWN, ROOM_TYPE.MONSTER_ELITE, ROOM_TYPE.TREASURE, ROOM_TYPE.QUEST, ROOM_TYPE.CHAPEL, ROOM_TYPE.DUNGEON };

        types = types.Where(rt => roomTypeMaxCount[rt] > 0).ToList();

        if (floor == 0)
            types.RemoveAll(rt => rt != ROOM_TYPE.MONSTER_SOLO && rt != ROOM_TYPE.MERCHANT);

        if (floor < 2)
            types.RemoveAll(rt => rt == ROOM_TYPE.DUNGEON);
        //return types[RandomPosition(types.Count)]; // Fallback in case something goes wrong
        if (floor < 3)
            types.RemoveAll(rt => rt == ROOM_TYPE.MONSTER_ELITE || rt == ROOM_TYPE.REST || rt == ROOM_TYPE.TREASURE);

        // Filter out types that exceeded the limit
        types = types.Where(rt => (!currentRoomTypeCount.ContainsKey(rt) || currentRoomTypeCount[rt] < roomTypeMaxCount[rt])).ToList();

        // Remove types that cannot repeat and are neighbors
        types.RemoveAll(rt => !roomTypeCanRepeat[rt] && IsNeighbourSameType(rt, x, y));

        float totalProbability = types.Sum(rt => roomTypeProbabilities[rt]);
        float randomValue = UnityEngine.Random.Range(0, totalProbability);

        foreach (var type in types)
        {
            if (randomValue < roomTypeProbabilities[type])
            {
                if (!currentRoomTypeCount.ContainsKey(type))
                    currentRoomTypeCount[type] = 1;
                else
                    currentRoomTypeCount[type]++;

                return type;
            }
            randomValue -= roomTypeProbabilities[type];
        }

        return types[RandomPosition(types.Count)]; // Fallback in case something goes wrong
    }


    bool IsNeighbourSameType(ROOM_TYPE type, int x, int y)
    {
        // Check all direct and diagonal neighbors
        int[] dx = { 0, 0, 1, -1, 1, 1, -1, -1 };
        int[] dy = { 1, -1, 0, 0, 1, -1, 1, -1 };

        for (int i = 0; i < 8; i++)
        {
            int nx = x + dx[i];
            int ny = y + dy[i];

            if (nx >= 0 && ny >= 0 && nx < Width && ny < Height && map[nx, ny] == type)
                return true;
        }

        return false;
    }



    void SaveToFile(string content)
    {
        string path = GetSavePath();

        try
        {
            File.WriteAllText(path, content);
            Debug.Log($"Saved to {path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save the file: {e.Message}");
        }
    }

    string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, "mapData.json");
    }



    private void GenerateMap()
    {
        mapData = new DijkstraMap();
        mapData.exportMap = new List<DijkstraMapVertex>();

        map = new ROOM_TYPE[Width, Height];

        for (int i = 0; i < NumPaths; i++)
        {
            int prevX = i == 0 ? RandomPosition(Width) : RandomPosition(Width - 1);
            int prevY = 0;

            for (int j = 1; j < Height; j++)
            {
                int nextX = RandomPositionAround(prevX);
                int nextY = j;

                // Assign temporary room type for visualization
                map[prevX, prevY] = ROOM_TYPE.MONSTER_SOLO;
                map[nextX, nextY] = ROOM_TYPE.MONSTER_SOLO;

                prevX = nextX;
                prevY = nextY;
            }
        }

        AssignRoomTypes();
        PopulateMapData();
    }

    void AddStartNode(Dictionary<string, DijkstraMapVertex> vertices)
    {
        DijkstraMapVertex startVertex = new DijkstraMapVertex
        {
            id = "POI_START",
            type = (int)ROOM_TYPE.START,  // The type can be set to whatever you think is appropriate
                                          // screenPosition = new Coordinates2DCartesian { x = Width / 2 * 500, y = -1 * 500 },
            screenPosition = new Coordinates2DCartesian { x = 0, y = (-Height / 2 - 1) * 500 },
            mapPosition = new Coordinates2DCartesian { x = -1, y = -1 }
        };
        startVertex.nodes = new List<DijkstraMapNode>();

        for (int x = 0; x < Width; x++)
        {
            if (map[x, 0] != ROOM_TYPE.NONE)
            {
                DijkstraMapNode connectedNode = new DijkstraMapNode { idOfVertex = $"POI_{x}_0", weight = 1 };
                startVertex.nodes.Add(connectedNode);
            }
        }

        mapData.exportMap.Add(startVertex);
        vertices[startVertex.id] = startVertex;
    }


    void AddFinalNode(Dictionary<string, DijkstraMapVertex> vertices)
    {
        DijkstraMapVertex endVertex = new DijkstraMapVertex
        {
            id = "POI_ENDGAME",
            type = (int)ROOM_TYPE.ENDGAME,  // The type can be set to whatever you think is appropriate
                                            // screenPosition = new Coordinates2DCartesian { x = Width / 2 * 500, y = Height * 500 },
            screenPosition = new Coordinates2DCartesian { x = 0, y = (Height / 2) * 500 }, // Centered y-axis and set one unit below the map
            mapPosition = new Coordinates2DCartesian { x = -1, y = Height }
        };
        endVertex.nodes = new List<DijkstraMapNode>();




        for (int x = 0; x < Width; x++)
        {
            if (map[x, Height - 1] != ROOM_TYPE.NONE)
            {
                DijkstraMapNode connectedNode = new DijkstraMapNode { idOfVertex = $"POI_{x}_{Height - 1}", weight = 1 };
                endVertex.nodes.Add(connectedNode);
            }
        }

        mapData.exportMap.Add(endVertex);
        vertices[endVertex.id] = endVertex;


        // vazny na final node z predposlenich
        for (int x = 0; x < Width; x++)
        {
            if (map[x, Height - 1] != ROOM_TYPE.NONE)
            {
                if (vertices.TryGetValue($"POI_{x}_{Height - 1}", out DijkstraMapVertex lastRowVertex))
                {
                    DijkstraMapNode connectionToFinalNode = new DijkstraMapNode { idOfVertex = "POI_ENDGAME", weight = 1 };
                    lastRowVertex.nodes.Add(connectionToFinalNode);
                }
            }
        }
    }


    void PopulateMapData()
    {
        Dictionary<string, DijkstraMapVertex> vertices = new Dictionary<string, DijkstraMapVertex>();
        AddStartNode(vertices);

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                if (map[x, y] != ROOM_TYPE.NONE)
                {
                    DijkstraMapVertex vertex = new DijkstraMapVertex
                    {
                        id = $"POI_{x}_{y}",
                        type = (int)map[x, y],  // Set the type here
                        screenPosition = new Coordinates2DCartesian { x = (x - Width / 2) * 500, y = (y - Height / 2) * 500 }, // Centered coordinates
                        //screenPosition = new Coordinates2DCartesian { x = x * 500, y = y * 500 },
                        mapPosition = new Coordinates2DCartesian { x = x, y = y },
                        nodes = new List<DijkstraMapNode>()
                    };

                    HashSet<string> connectedNodes = new HashSet<string>();

                    // Connect vertically if possible
                    if (y > 0 && map[x, y - 1] != ROOM_TYPE.NONE) connectedNodes.Add($"POI_{x}_{y - 1}");
                    if (y < Height - 1 && map[x, y + 1] != ROOM_TYPE.NONE) connectedNodes.Add($"POI_{x}_{y + 1}");

                    // If missing a vertical connection, attempt a lateral connection
                    if (connectedNodes.Count < 2)
                    {
                        if (x > 0 && map[x - 1, y] != ROOM_TYPE.NONE) connectedNodes.Add($"POI_{x - 1}_{y}");
                        if (x < Width - 1 && map[x + 1, y] != ROOM_TYPE.NONE) connectedNodes.Add($"POI_{x + 1}_{y}");
                    }

                    // If still missing connections, try diagonally
                    if (connectedNodes.Count < 2)
                    {
                        if (y > 0)
                        {
                            if (x > 0 && map[x - 1, y - 1] != ROOM_TYPE.NONE) connectedNodes.Add($"POI_{x - 1}_{y - 1}");
                            if (x < Width - 1 && map[x + 1, y - 1] != ROOM_TYPE.NONE) connectedNodes.Add($"POI_{x + 1}_{y - 1}");
                        }
                        if (y < Height - 1)
                        {
                            if (x > 0 && map[x - 1, y + 1] != ROOM_TYPE.NONE) connectedNodes.Add($"POI_{x - 1}_{y + 1}");
                            if (x < Width - 1 && map[x + 1, y + 1] != ROOM_TYPE.NONE) connectedNodes.Add($"POI_{x + 1}_{y + 1}");
                        }
                    }

                    foreach (var nodeId in connectedNodes)
                    {
                        DijkstraMapNode node = new DijkstraMapNode { idOfVertex = nodeId, weight = 1 };
                        vertex.nodes.Add(node);
                    }

                    mapData.exportMap.Add(vertex);
                    vertices[vertex.id] = vertex;
                }
            }
        }


        // Ensure bidirectional connections:
        foreach (var vertex in vertices.Values)
        {
            foreach (var node in vertex.nodes)
            {
                if (vertices.TryGetValue(node.idOfVertex, out DijkstraMapVertex targetVertex))
                {
                    if (!targetVertex.nodes.Exists(n => n.idOfVertex == vertex.id))
                    {
                        DijkstraMapNode backNode = new DijkstraMapNode { idOfVertex = vertex.id, weight = 1 };
                        targetVertex.nodes.Add(backNode);
                    }
                }
            }
        }

        AddFinalNode(vertices);

    }



    void AssignRoomTypes()
    {
        // Create a list of all (x, y) coordinates
        List<(int x, int y)> coordinates = new List<(int x, int y)>();

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                coordinates.Add((x, y));
            }
        }

        // Shuffle the coordinates
        Shuffle(coordinates);

        // Go through the shuffled list to set the room types
        foreach (var coord in coordinates)
        {
            int x = coord.x;
            int y = coord.y;

            if (map[x, y] != ROOM_TYPE.NONE)
            {
                map[x, y] = RandomRoomType(y, x, y);
            }

            //if (y == 0 && map[x, y] != POI_TYPE.NONE)
            //{
            //    map[x, y] = POI_TYPE.MONSTER_SOLO;
            //}
            //else if (y > 0 && y < Height - 1 && map[x, y] != POI_TYPE.NONE)
            //{
            //    map[x, y] = RandomRoomType(y, x, y);
            //}


        }
    }

    void Shuffle<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;

        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }



    int RandomPosition(int max)
    {
        return UnityEngine.Random.Range(0, max);
    }

    int RandomPositionAround(int x)
    {
        return Mathf.Clamp(x + RandomPosition(3) - 1, 0, Width - 1);
    }

    void PrintMap()
    {
        for (int y = 0; y < Height; y++)
        {
            string row = "";
            for (int x = 0; x < Width; x++)
            {
                row += map[x, y] == ROOM_TYPE.NONE ? ' ' : map[x, y].ToString()[0];
            }
            Debug.Log(row);
        }
    }

    int ManhattanDistance(int x1, int y1, int x2, int y2)
    {
        return Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2);
    }

    public void GenerateNewMap()
    {

        currentRoomTypeCount.Clear(); // Reset the room type count before generating a new map
        GenerateMap();
        string generatedMapjson = JsonUtility.ToJson(mapData, true);

        AccountDataSO.LocationData.dijkstraMap = JsonUtility.FromJson<simplestmmorpg.data.DijkstraMap>(generatedMapjson);


    }


    public void SaveMapToFile()
    {
        //  string json = JsonUtility.ToJson(mapData, true);
        //SaveToFile(json);

        FirebaseCloudFunctionSO_Admin.GenerateLocationMap(mapData, "DUNOTAR", "SEASON_TEST");

    }
}
