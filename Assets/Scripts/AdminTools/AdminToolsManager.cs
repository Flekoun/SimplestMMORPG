using System.Collections;
using System.Collections.Generic;
using Firebase.Firestore;
using simplestmmorpg.adminToolsData;
using simplestmmorpg.data;
using UnityEngine;
using UnityEngine.Events;

public class AdminToolsManager : MonoBehaviour
{

    public DropTablesData EnemyDropTablesData;
    public PointOfInterestServerDataDefinitions ServerData;
    public InternalDefinition InternalDefinition;
    public UnityAction OnEnemyDropTablesDataChanged;
    public UnityAction OnTiersChanged;
    public UnityAction OnInternalDefinitionChanged;
    public UIItemIdChooser UIItemIdChooser;
    //Here is a private reference only this class can access
    private static AdminToolsManager _instance;
    //This is the public reference that other classes will use
    public static AdminToolsManager instance
    {
        get
        {
            //If _instance hasn't been set yet, we grab it from the scene!
            //This will only happen the first time this reference is used.
            if (_instance == null)
                _instance = GameObject.FindObjectOfType<AdminToolsManager>();
            return _instance;
        }
    }

    public void SetEnemyDropTablesData(DocumentSnapshot _snapshot)
    {
        EnemyDropTablesData = _snapshot.ConvertTo<DropTablesData>();
        OnEnemyDropTablesDataChanged?.Invoke();
    }

    public void SetServerData(DocumentSnapshot _snapshot)
    {
        ServerData = _snapshot.ConvertTo<PointOfInterestServerDataDefinitions>();
        OnTiersChanged?.Invoke();
    }

    public void SetInternalDefinitionData(DocumentSnapshot _snapshot)
    {
        InternalDefinition = _snapshot.ConvertTo<InternalDefinition>();
        OnInternalDefinitionChanged?.Invoke();
    }

    public void ShowItemChooserMonsters(UnityAction<List<UISelectableEntry>> _callback)
    {

        UIItemIdChooser.Enemies = true;
        UIItemIdChooser.Items = false;
        UIItemIdChooser.CraftingRecipes = false;
        UIItemIdChooser.PointsOfInterest = false;

        UIItemIdChooser.ClearAllListeners();
        UIItemIdChooser.OnItemsToAddSelected += _callback;
        UIItemIdChooser.Show();

    }

    public void ShowItemChooserItems(UnityAction<List<UISelectableEntry>> _callback)
    {
        UIItemIdChooser.Enemies = false;
        UIItemIdChooser.Items = true;
        UIItemIdChooser.CraftingRecipes = true;
        UIItemIdChooser.PointsOfInterest = false;

        UIItemIdChooser.ClearAllListeners();
        UIItemIdChooser.OnItemsToAddSelected += _callback;

        UIItemIdChooser.Show();

    }

}
