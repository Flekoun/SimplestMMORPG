using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using simplestmmorpg.data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class UIGatherablesSpawner : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public PrefabFactory PrefabFactory;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public Transform Parent;
    public GameObject GatherableUIPrefab;
    public ListenOnGatherables ListenOnGatherables;
    public UIPointsOfInterestSpawner UIPointsOfInterestSpawner;
    public List<Gatherable> Data;
    public List<UIGatherable> UIEntriesList = new List<UIGatherable>();
    public UnityAction OnRefreshed;
    public void Awake()
    {
        ListenOnGatherables.OnNewData += Refresh;
        //AccountDataSO.OnCharacterLoadedFirstTime += Refresh;
        //   AccountDataSO.OnWorldPositionChanged += Refresh;
    }

    public void OnDisable()
    {
        //   AccountDataSO.OnVendorsDataChanged -= Refresh;
        AccountDataSO.OnWorldPointOfInterestChanged -= StartListeningOnNewPoI;

    }

    public void OnEnable()
    {
        //  AccountDataSO.OnVendorsDataChanged += Refresh;
        //  AccountDataSO.OnWorldPositionChanged += Refresh;
        AccountDataSO.OnWorldPointOfInterestChanged += StartListeningOnNewPoI;
        StartListeningOnNewPoI();
    }

    void StartListeningOnNewPoI()
    {
        ListenOnGatherables.StartListeningOnPosition(AccountDataSO.CharacterData.position);
    }

    void Refresh(List<Gatherable> _data)
    {
        Data = _data;
        Utils.DestroyAllChildren(Parent);//, 1);
        UIEntriesList.Clear();


        SpawnAtRandomPositions();
        //foreach (Gatherable item in _data)
        //{
        //    var gatherable = PrefabFactory.CreateGameObject<UIGatherable>(GatherableUIPrefab, Parent, GetRandomPosition());
        //    gatherable.SetData(item);
        //    gatherable.OnClicked += OnEntryClicked;
        //    UIEntriesList.Add(gatherable);

        //}

     

            if (OnRefreshed != null)
            OnRefreshed.Invoke();

        //ContentFitterRefresh.RefreshContentFitters();

    }

    public async void OnEntryClicked(UIGatherable _item)
    {
        if (_item.Data.HasEnoughtSkillToGatherThis(AccountDataSO.CharacterData.professions))
        {
            await FirebaseCloudFunctionSO.ClaimGatherableAsync(_item.Data.uid);
            UIManager.instance.ImportantMessage.ShowMesssage(Utils.GetMetadataForGatherable(_item.Data.gatherableType).title.GetText() + " gathered!");

        }
        else
        {
            UIManager.instance.ImportantMessage.ShowMesssage(Utils.ReplacePlaceholdersInTextWithDescriptionFromMetadata("Need " + _item.Data.SkillsNeededToGatherThis() + "to be gathered"));
        }

    }

    private void SpawnAtRandomPositions()
    {

        Debug.Log("EGGG??" + Data.Count);
        //// Generate a random distance from the reference object
        //float distance = Random.Range(minDistance, maxDistance);

        //// Generate a random angle around the reference object
        //float angle = Random.Range(0.0f, 360.0f);

        //Vector2 position = UIPointsOfInterestSpawner.GetPoIPlayerIsCurrentlyOn().transform.position + Quaternion.Euler(0, 0, angle) * Vector2.right * distance;

        float minDistance = 0.8f;
        float maxDistance = 1.8f;


        // Calculate the angle between each prefab
        float angleStep = 360.0f / Data.Count + 1;

        //float randomStartAngle = (Random.Range(0f, 360f));
        // Spawn the prefabs around the reference object
        for (int i = 0; i < Data.Count; i++)
        {

            Debug.Log("EGGG??YYY" + i);
            float stepVariance = Random.Range(-(angleStep / 2f), (angleStep / 2f));  //50% variance in angle to make it look mor random

            // Generate a random distance from the reference object
            float distance = Random.Range(minDistance, maxDistance);

            // Calculate the distance based on the angle
            // float distance = Mathf.Lerp(maxDistance, minDistance, (angleStep * i) / 180.0f);

            // Calculate the position of the prefab based on the distance and angle
            Vector2 position = UIPointsOfInterestSpawner.GetPoIPlayerIsCurrentlyOn().transform.position + Quaternion.Euler(0, 0, stepVariance + angleStep * i) * Vector2.right * distance;

            var gatherable = PrefabFactory.CreateGameObject<UIGatherable>(GatherableUIPrefab, Parent, position);
            gatherable.SetData(Data[i]);
            gatherable.OnClicked += OnEntryClicked;
            UIEntriesList.Add(gatherable);
        }
    }





}
