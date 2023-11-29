
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using Firebase.Functions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RoboRyanTron.Unite2017.Variables;
using simplestmmorpg.adminToolsData;
using simplestmmorpg.data;
using simplestmmorpg.playerData;
using TMPro;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

[CreateAssetMenu]
public class FirebaseCloudFunctionSO_Admin : ScriptableObject
{
    public AccountDataSO AccountDataSO;
    public StringVariable ServerSecret;
    FirebaseFunctions functions;

    public UnityAction OnCloudFunctionInProgress;
    public UnityAction OnCloudFunctionFinished;


    //public bool UseLocalHost = false;


    public void OnEnable()
    {
        functions = FirebaseFunctions.DefaultInstance;

        //#if UNITY_EDITOR
        //        if (UseLocalHost)
        //            functions.UseFunctionsEmulator("localhost:5001");
        //#endif

    }


    private void CloudFunctionCalled()
    {
        //  Debug.Log("showing");
        OnCloudFunctionInProgress?.Invoke();
    }

    private void CloudFunctionFinished(string _result)
    {
        //    Debug.Log("hiding");

        if (_result.StartsWith("ERROR"))
        {
            Debug.Log(_result);
            UIManager.instance.SpawnErrorText(_result);
        }

        OnCloudFunctionFinished?.Invoke();
    }

    private void CloudFunctionFinished()
    {
        OnCloudFunctionFinished?.Invoke();
    }

    private async Task CallCloudFunction(string _cloudFunctionName, Dictionary<string, object> _data)
    {
        string result = await FirebaseFunctions.DefaultInstance.GetHttpsCallable(_cloudFunctionName).CallAsync(_data).ContinueWith(OnCloudFuntionResult);
        CloudFunctionFinished(result);
    }


    public async void SavePointOfInterestScreenPositionsForLocation(List<UIPointOfInterestButton> _pointsOfInterestButtons)
    {

        CloudFunctionCalled();
        List<Coordinates2DCartesian> screenPoisitions = new List<Coordinates2DCartesian>();
        List<string> PoIIds = new List<string>();


        foreach (var poiButton in _pointsOfInterestButtons)
        {
            var pos = new Coordinates2DCartesian();

            pos.x = (int)poiButton.transform.localPosition.x;
            pos.y = (int)poiButton.transform.localPosition.y;

            screenPoisitions.Add(pos);
            PoIIds.Add(poiButton.WorldPosition.pointOfInterestId);
        }


        var positions = JsonConvert.SerializeObject(screenPoisitions);

        var ids = JsonConvert.SerializeObject(PoIIds);
        Debug.Log("positions : " + positions);
        var data = new Dictionary<string, object>();
        data["positions"] = positions;
        data["poiIds"] = ids;
        data["locationId"] = AccountDataSO.CharacterData.position.locationId;

        await CallCloudFunction("adminTools-savePointOfInterestScreenPositionsForLocation", data);
    }



    public async void SaveDropTablesEnemy(EnemyDropTablesData _enemyDropTable, string _zoneId, string _locationId)
    {

        CloudFunctionCalled();

        var dropTables = JsonConvert.SerializeObject(_enemyDropTable);
        Debug.Log("dropTables : " + dropTables);

        var data = new Dictionary<string, object>();
        data["dropTables"] = dropTables;
        data["locationId"] = _locationId;
        data["zoneId"] = _zoneId;

        await CallCloudFunction("adminTools-saveDropTablesEnemy", data);
    }

    public async void SaveTiers(PointOfInterestServerDataDefinitions _tiers, string _zoneId, string _locationId, string _pointOfInterest)
    {
        ////odstranim vsechny properties ktere jsou null.....protoze rewardsGenerated mi vraci hafo null prootze implementuje ten IContentDisplayable a ten kokot z nejakeho duvovu serializuje vsechny porperty z toho interfacu...
        //var json = JsonConvert.SerializeObject(_tiers);
        //var obj = JsonConvert.DeserializeObject<JObject>(json);
        //var properties = obj.Properties().Where(prop => prop.Value.Type == JTokenType.Null).ToList();
        //foreach (var prop in properties)
        //{
        //    prop.Remove();
        //}
        //string resultJson = obj.ToString();


        var tiers = JsonConvert.SerializeObject(_tiers);

        CloudFunctionCalled();

        Debug.Log("tiers : " + tiers);

        var data = new Dictionary<string, object>();
        data["tiers"] = tiers;
        data["locationId"] = _locationId;
        data["zoneId"] = _zoneId;
        data["pointOfInterestId"] = _pointOfInterest;

        await CallCloudFunction("adminTools-saveTiers", data);
    }

    public async void SaveMapGeneratorPointsOfInterest(InternalDefinition _definition)
    {
        ////odstranim vsechny properties ktere jsou null.....protoze rewardsGenerated mi vraci hafo null prootze implementuje ten IContentDisplayable a ten kokot z nejakeho duvovu serializuje vsechny porperty z toho interfacu...
        //var json = JsonConvert.SerializeObject(_tiers);
        //var obj = JsonConvert.DeserializeObject<JObject>(json);
        //var properties = obj.Properties().Where(prop => prop.Value.Type == JTokenType.Null).ToList();
        //foreach (var prop in properties)
        //{
        //    prop.Remove();
        //}
        //string resultJson = obj.ToString();


        var definition = JsonConvert.SerializeObject(_definition);

        CloudFunctionCalled();

        Debug.Log("definition : " + definition);

        var data = new Dictionary<string, object>();
        data["definition"] = definition;


        await CallCloudFunction("adminTools-saveMapGeneratorPointsOfInterest", data);
    }

    public async void GenerateLocationMap(DijkstraMap _map, string _zoneId, string _locationId)
    {
        ////odstranim vsechny properties ktere jsou null.....protoze rewardsGenerated mi vraci hafo null prootze implementuje ten IContentDisplayable a ten kokot z nejakeho duvovu serializuje vsechny porperty z toho interfacu...
        //var json = JsonConvert.SerializeObject(_tiers);
        //var obj = JsonConvert.DeserializeObject<JObject>(json);
        //var properties = obj.Properties().Where(prop => prop.Value.Type == JTokenType.Null).ToList();
        //foreach (var prop in properties)
        //{
        //    prop.Remove();
        //}
        //string resultJson = obj.ToString();


        var map = JsonConvert.SerializeObject(_map);

        CloudFunctionCalled();

        Debug.Log("map : " + map);

        var data = new Dictionary<string, object>();
        data["dijkstra"] = map;
        data["locationId"] = _locationId;
        data["zoneId"] = _zoneId;

        await CallCloudFunction("adminTools-generateLocationMap", data);
    }



    private string OnCloudFuntionResult(Task<HttpsCallableResult> _task)
    {
        //        Debug.Log("sem tu");
        if (_task.IsFaulted)
        {
            string resultError = "ERROR " + _task.Exception.InnerException.Message;
            return resultError;

        }
        string json = JsonConvert.SerializeObject(_task.Result.Data, Formatting.Indented);
        return json;




    }



}
