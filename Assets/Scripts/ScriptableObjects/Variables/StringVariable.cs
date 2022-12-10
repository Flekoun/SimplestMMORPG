// ----------------------------------------------------------------------------
// Unite 2017 - Game Architecture with Scriptable Objects
// 
// Author: Ryan Hipple
// Date:   10/04/17
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using RoboRyanTron.Unite2017.Events;
using UnityEngine;

namespace RoboRyanTron.Unite2017.Variables
{
    [CreateAssetMenu]
    public class StringVariable : ScriptableObject
    {

        private List<Action> ChangeEventActions = new List<Action>();
        public bool ResetValueAfterStartup = false;
        public string StartupValue = "";
        //  [SerializeField]
        //    private string value = "";

        public void ListenOnChangeEvent(Action _action)
        {
            if (!ChangeEventActions.Contains(_action))
                ChangeEventActions.Add(_action);
        }

        public void UnlistenOnChangeEvent(Action _action)
        {
            if (ChangeEventActions.Contains(_action))
                ChangeEventActions.Remove(_action);
        }
        public void OnEnable()
        {
            if (ResetValueAfterStartup)
                Value = StartupValue;
        }

        public void OnDisable()
        {
            if (ResetValueAfterStartup)
                Value = StartupValue;
        }

        public string Value;


        //{
        //    get { return value; }
        //    set { this.value = value; }
        //}


        public void SetValue(string value)
        {
            // Debug.Log("Applying change: " + Value + " to: " + value);
            if (value == Value)
                return;

            Value = value;


            if (ChangeEvent != null)
                ChangeEvent.Raise(Value);

            foreach (var item in ChangeEventActions)
            {
                if (item != null)
                    item.Invoke();
            }
        }

        public void SetValue(StringVariable value)
        {
            //   Debug.Log("Applying change: " + Value + " to: "+ value.Value);
            if (value.Value == Value)
                return;


            Value = value.Value;


            if (ChangeEvent != null)
                ChangeEvent.Raise(Value);
        }

        public GameEvent_String ChangeEvent;

    }
}