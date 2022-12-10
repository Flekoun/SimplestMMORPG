using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class BaseIdDefinition : ScriptableObject
{

    public string Id;
   // public string DisplayName;
  //  public ImageIdDefinition Image;
    //public Vector2 position;

    [Space]
    public BaseDefinitionSOSet BaseDefinitionSOSet;

  
    [ContextMenu("Setup")]
    private void Setup()
    {
      //  BaseDefinitionSOSet = Resources.FindObjectsOfTypeAll<BaseDefinitionSOSet>()[0];

        Id = name.Substring(name.IndexOf('-') + 1);
        BaseDefinitionSOSet.AddItem(this);
    }


    [ContextMenu("DeletThis")]
    private void DeleteThis()
    {
#if UNITY_EDITOR
        BaseDefinitionSOSet.RemoveItem(this);
        List<string> path = new List<string>();
        path.Add(AssetDatabase.GetAssetPath(this.GetInstanceID()));
        AssetDatabase.DeleteAssets(path.ToArray(), new List<string>());
#endif
    }

 //   public GetLocalize

    //public string GetId()
    //{
    //    return Id;
    //}

    //public Vector2 GetScreenPosition()
    //{
    //    return position;
    //}
}
