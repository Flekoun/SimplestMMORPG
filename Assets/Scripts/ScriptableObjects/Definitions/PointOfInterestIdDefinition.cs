using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "SimplestMMORPG/PointOfInterestIdDefinition")]
public class PointOfInterestIdDefinition : BaseIdDefinition
{

    //public string Id;
    //public string DisplayName;
    //public Sprite Image;
    public Vector2 position;

    //[Space]
  //  public PointOfInterestIdDefinitionSOSet PointOfInterestIdDefinitionSOSet;

//    [ContextMenu("Setup")]
//    private void Setup()
//    {
//        BaseDefinitionSOSet = Resources.FindObjectsOfTypeAll<PointOfInterestIdDefinitionSOSet>()[0];
//       // PointOfInterestIdDefinitionSOSet = Resources.FindObjectsOfTypeAll<PointOfInterestIdDefinitionSOSet>()[0];

//        Id = name.Substring(name.IndexOf('-') + 1);
//       /// PointOfInterestIdDefinitionSOSet.AddItem(this);
//        BaseDefinitionSOSet.AddItem(this);
//    }


//    [ContextMenu("DeletThis")]
//    private void DeleteThis()
//    {
//#if UNITY_EDITOR
//        BaseDefinitionSOSet.RemoveItem(this);
//     //   PointOfInterestIdDefinitionSOSet.RemoveItem(this);
//        List<string> path = new List<string>();
//        path.Add(AssetDatabase.GetAssetPath(this.GetInstanceID()));
//        AssetDatabase.DeleteAssets(path.ToArray(), new List<string>());
//#endif
//    }

  
    public Vector2 GetScreenPosition()
    {
        return position;
    }
}
