using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SimplestMMORPG/Sets/BaseIdDefinitionSOSet")]
public class BaseDefinitionSOSet : ScriptableObject
{
    // Start is called before the first frame update


    public List<BaseIdDefinition> Items;
    public BaseIdDefinition Default;

    public BaseIdDefinition GetRandomItem()
    {
        return Items[UnityEngine.Random.Range(0, Items.Count)];
    }

    public BaseIdDefinition GetDefinitionById(string _id)
    {

        BaseIdDefinition effectDef = Items.Find(item => item.Id == _id /* (item.Id.CompareTo(_id)==0)*/ );

        if (effectDef != null)
        {
            return effectDef;
        }
        else Debug.LogWarning("ID  nenalezen! Dopln ho do " + this.name + " : " + _id); return Default;

    }

    public void AddItem(BaseIdDefinition _item)
    {
        if (!Items.Contains(_item))
            Items.Add(_item);
    }

    public void RemoveItem(BaseIdDefinition _item)
    {
        if (Items.Contains(_item))
            Items.Remove(_item);
    }
}


public interface IHasScreenPosition
{
    public Vector2 GetScreenPosition();
}
