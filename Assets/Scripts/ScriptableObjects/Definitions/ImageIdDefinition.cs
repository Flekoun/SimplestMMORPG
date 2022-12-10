using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;
using UnityEditor;

[CreateAssetMenu(menuName = "SimplestMMORPG/ImageIdDefinition")]
public class ImageIdDefinition : ScriptableObject
{
    public string IdPrefix = "";
    public string ImageId;
    public Sprite Image;
    [Space]
    public ImageIdDefinitionSOSet ImageIdDefinitionSOSet;

    [ContextMenu("Setup")]
    private void Setup()
    {
        ImageIdDefinitionSOSet = Resources.FindObjectsOfTypeAll<ImageIdDefinitionSOSet>()[0];
        ImageId = name.Substring(name.IndexOf('-') + 1);
        ImageIdDefinitionSOSet.AddItem(this);

    }
    [ContextMenu("DeletThis")]
    private void DeleteThis()
    {
#if UNITY_EDITOR
        ImageIdDefinitionSOSet.RemoveItem(this);
        List<string> path = new List<string>();
        path.Add(AssetDatabase.GetAssetPath(this.GetInstanceID()));
        AssetDatabase.DeleteAssets(path.ToArray(), new List<string>());
#endif
    }



}

