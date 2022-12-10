using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SimplestMMORPG/Sets/EquipSlotDefinitionSOSet")]
public class EquipSlotDefinitionSOSet : ScriptableObject
{
    // Start is called before the first frame update
  

    public List<EquipSlotDefinition> Items;
   // public EffectDefinition Default;

    public EquipSlotDefinition GetRandomItem()
    {
        return Items[UnityEngine.Random.Range(0, Items.Count)];
    }

    public EquipSlotDefinition GetDefinitionById(string _id)
    {

        EquipSlotDefinition effectDef = Items.Find(item => item.EquipSlotId == _id /* (item.Id.CompareTo(_id)==0)*/ );

        if (effectDef != null)
        {
            return effectDef;
        }
        else Debug.LogWarning("ID  nenalezen! Dopln ho do " + this.name + " : " + _id); return null;

    }
}
