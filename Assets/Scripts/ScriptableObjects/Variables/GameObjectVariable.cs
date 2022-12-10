// ----------------------------------------------------------------------------
// Unite 2017 - Game Architecture with Scriptable Objects
// 
// Author: Ryan Hipple
// Date:   10/04/17
// ----------------------------------------------------------------------------

using RoboRyanTron.Unite2017.Events;
using UnityEngine;

namespace RoboRyanTron.Unite2017.Variables
{
    [CreateAssetMenu]
    public class GameObjectVariable : ScriptableObject
    {
   

        public GameObject Value;
        //{
        //    get { return value; }
        //    set { this.value = value; }
        //}

        public void ClearValue()
        {
            Value = null;
            if (ChangeEvent != null)
                ChangeEvent.Raise(Value);
        }

        public void SetValue(GameObject value)
        {
            // Debug.Log("Applying change: " + Value + " to: " + value);
            if (value == Value)
                return;


         
            Value = value;
         

            if (ChangeEvent != null)
                ChangeEvent.Raise(Value);
        }

        public void SetValue(GameObjectVariable value)
        {
            //   Debug.Log("Applying change: " + Value + " to: "+ value.Value);
            if (value.Value == Value)
                return;

       
                Value = value.Value;


            if (ChangeEvent != null)
                ChangeEvent.Raise(Value);
        }

        public GameEvent_GameObject ChangeEvent;
    }
}