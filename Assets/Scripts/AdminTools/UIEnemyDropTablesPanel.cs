using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEnemyDropTablesPanel : MonoBehaviour
{
    public FirebaseCloudFunctionSO_Admin FirebaseCloudFunctionSO_Admin;
    public PrefabFactory PrefabFactory;
    public GameObject UIDropTableEnemyPrefab;
    public ListenOnEnemyDropTables ListenOnEnemyDropTables;
    public Transform Parent;
    public GameObject Model;

    public string ZoneId = "DUNOTAR";
    public string LocationId = "VALLEY_OF_TRIALS";

    private List<UIDropTableEnemy> List = new List<UIDropTableEnemy>();

    public void Awake()
    {
        AdminToolsManager.instance.OnEnemyDropTablesDataChanged += Refresh;
    }

    public void Show()
    {
        ListenOnEnemyDropTables.StartListening(ZoneId, LocationId);
        Model.gameObject.SetActive(true);


    }

    private void Refresh()
    {
        Utils.DestroyAllChildren(Parent);
        List.Clear();

        foreach (var item in AdminToolsManager.instance.EnemyDropTablesData.enemyDropTables)
        {
            var UIItem = PrefabFactory.CreateGameObject<UIDropTableEnemy>(UIDropTableEnemyPrefab, Parent);

            UIItem.Setup(item);
            List.Add(UIItem);
        }

        foreach (var item in AdminToolsManager.instance.EnemyDropTablesData.chestDropTables)
        {
            var UIItem = PrefabFactory.CreateGameObject<UIDropTableEnemy>(UIDropTableEnemyPrefab, Parent);

            UIItem.Setup(item);
            List.Add(UIItem);
        }
    }

    public void Hide()
    {
        Model.gameObject.SetActive(false);
        ListenOnEnemyDropTables.StopListening();
    }

    public void SaveClicked()
    {
        foreach (var item in List)
            item.Save();

        FirebaseCloudFunctionSO_Admin.SaveDropTablesEnemy(AdminToolsManager.instance.EnemyDropTablesData, ZoneId, LocationId);

    }
}
