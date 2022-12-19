using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using UnityEngine.Events;

public class UISpecialsSpawner : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public PrefabFactory PrefabFactory;
    public Transform Parent;
    public GameObject UIAuctionHouseButtonPrefab;
    public GameObject UIInboxButtonPrefab;

    public List<GameObject> UIEntriesList = new List<GameObject>();
    public UnityAction OnRefreshed;
    public void Awake()
    {
        //AccountDataSO.OnCharacterLoadedFirstTime += Refresh;
        //   AccountDataSO.OnWorldPositionChanged += Refresh;
    }

    public void OnDisable()
    {
        //   AccountDataSO.OnVendorsDataChanged -= Refresh;
        AccountDataSO.OnWorldPointOfInterestChanged -= Refresh;

    }

    public void OnEnable()
    {
        //  AccountDataSO.OnVendorsDataChanged += Refresh;
        //  AccountDataSO.OnWorldPositionChanged += Refresh;
        AccountDataSO.OnWorldPointOfInterestChanged += Refresh;
        Refresh();
    }

    public bool HasSpawnedAnySpecials()
    {
        Debug.Log("DDD :" + UIEntriesList.Count);
        return UIEntriesList.Count > 0;
    }

    void Refresh()
    {
        Utils.DestroyAllChildren(Parent, 1);
        UIEntriesList.Clear();


        foreach (var special in AccountDataSO.GetCurrentPointOfInterest().specials)
        {
            switch (special)
            {
                case Utils.POI_SPECIALS.AUCTION_HOUSE:
                    var ah = PrefabFactory.CreateGameObject<GameObject>(UIAuctionHouseButtonPrefab, Parent);
                    UIEntriesList.Add(ah);
                    break;

                case Utils.POI_SPECIALS.MAILBOX:
                    var inbox = PrefabFactory.CreateGameObject<GameObject>(UIInboxButtonPrefab, Parent);
                    UIEntriesList.Add(inbox);
                    break;

                default:
                    break;
            }
        }

        if (OnRefreshed != null)
            OnRefreshed.Invoke();

    }



}
