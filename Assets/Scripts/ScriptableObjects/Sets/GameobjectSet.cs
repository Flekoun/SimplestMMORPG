using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoboRyanTron.Unite2017.Sets;
using RoboRyanTron.Unite2017.Variables;
using RoboRyanTron.Unite2017.Events;

[CreateAssetMenu(menuName = "Sets/GameObjectSet")]


public class GameobjectSet : RuntimeSet<GameObject>
{

 

    public List<GameObject> GetAllExceptOne(GameObject _exception)
    {
        List<GameObject> result = new List<GameObject>();

        foreach (GameObject item in this.Items)
        {
            if(item!= _exception)
                result.Add(item);
        }

        return result;
    }
  
    //public bool DoesPlayerBulleyeExist(int _playerId)
    //{

    //    foreach (ChooseAble bEye in Items)
    //    {
    //        if (bEye.PlayerId == _playerId)
    //            return true;
    //    }

    //    return false;
    //}
  

  

}

