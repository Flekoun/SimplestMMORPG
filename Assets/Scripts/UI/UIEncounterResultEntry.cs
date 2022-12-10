using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using simplestmmorpg.data;
using UnityEngine.Events;

public class UIEncounterResultEntry : MonoBehaviour
{
    public PrefabFactory Factory;
    public TextMeshProUGUI EnemiesText;
    public Transform CorpseParent;
    public GameObject CorpsePrefab;
    public UnityAction<UIEncounterResultEntry> OnClicked; 
    public EncounterResult Data;

    private UIEncountersResultSpawner Spawner;



    //public override string GetUid()
    //{
    //    return Data.uid;
    //}

    public void SelectButtonClicked()
    {

        OnClicked.Invoke(this);
       // Spawner.OnUIEntryClicked(Data);
    }

        //private void SetAsSelected(bool _selected)
        //{
        //    SelectedImage.gameObject.SetActive(_selected);
        //}

    public void SetEncounter(EncounterResult _encounterData ,UIEncountersResultSpawner _parentSpawner)
    {
       
        Data = _encounterData;
        Spawner = _parentSpawner;

        Utils.DestroyAllChildren(CorpseParent);

      
        foreach (var item in Data.enemies)
        {
            Factory.CreateGameObject(CorpsePrefab, CorpseParent);
        }

        //string enemies = "";
        //foreach (var item in Data.enemies)
        //{
        //    enemies = enemies + item.displayName +"\n";
        //}

      //  EnemiesText.SetText(enemies);
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}

