using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SimplestMMORPG/Sets/ImageIdSOSet")]
public class ImageIdDefinitionSOSet : ScriptableObject
{
    // Start is called before the first frame update


    public List<ImageIdDefinition> Items;
    public ImageIdDefinition Default;

    public ImageIdDefinition GetRandomItem()
    {
        return Items[UnityEngine.Random.Range(0, Items.Count)];
    }

    public ImageIdDefinition GetDefinitionById(string _id, string _prefix = "")
    {
        ImageIdDefinition effectDef = null;
        if (_prefix == "")
            effectDef = Items.Find(item => item.IdPrefix + item.ImageId == item.IdPrefix + _id /* (item.Id.CompareTo(_id)==0)*/ );
        else
            effectDef = Items.Find(item => item.IdPrefix + item.ImageId == _prefix + _id /* (item.Id.CompareTo(_id)==0)*/ );

        if (effectDef != null)
        {
            return effectDef;
        }
        else Debug.LogWarning("ID  nenalezen! Dopln ho do " + this.name + " : " + _id + " prefix : " + _prefix); return Default;

    }



    public void AddItem(ImageIdDefinition _item)
    {
        if (!Items.Contains(_item))
            Items.Add(_item);
    }

    public void RemoveItem(ImageIdDefinition _item)
    {
        if (Items.Contains(_item))
            Items.Remove(_item);
    }
}
