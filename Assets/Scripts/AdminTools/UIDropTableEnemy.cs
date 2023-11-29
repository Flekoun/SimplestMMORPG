using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using simplestmmorpg.adminToolsData;


public class UIDropTableEnemy : MonoBehaviour
{
    public ImageIdDefinitionSOSet AllImageIdDefinitionSOSet;
    public PrefabFactory PrefabFactory;
    public GameObject UIDropTablePrefab;

    public TextMeshProUGUI EnemyIdText;
    public Image EnemyPortrait;
    public Transform Parent;

    private List<UIDropTable> List = new List<UIDropTable>();
    private EnemyDropTable Data;


    // Start is called before the first frame update
    public void Setup(EnemyDropTable _item)
    {
        Data = _item;
        EnemyIdText.SetText(Utils.DescriptionsMetadata.GetEnemyMetadata(_item.enemyId).title.EN);
        EnemyPortrait.sprite = AllImageIdDefinitionSOSet.GetDefinitionById(Utils.DescriptionsMetadata.GetEnemyMetadata(_item.enemyId).imageId).Image;

        Utils.DestroyAllChildren(Parent);
        List.Clear();
        foreach (var dropTable in _item.dropTables)
        {
            var UIItem = PrefabFactory.CreateGameObject<UIDropTable>(UIDropTablePrefab, Parent);

            UIItem.Setup(dropTable);
            List.Add(UIItem);
        }
    }

    public void Save()
    {
        foreach (var item in List)
        {
            item.Save();
        }
    }


}
