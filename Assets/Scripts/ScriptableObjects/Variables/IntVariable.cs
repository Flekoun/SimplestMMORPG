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
using UnityEngine.SceneManagement;

namespace RoboRyanTron.Unite2017.Variables
{
    [CreateAssetMenu(fileName = "Int", menuName = "Variables/Int")]
    public class IntVariable : ScriptableObject
    {
#if UNITY_EDITOR
        [Multiline]
        public string DeveloperDescription = "";

#endif
       
        public bool ResetValueAfterStartup = false;
        public bool HasMaxValue = false;
        public bool HasMinValue = false;
        public bool LoopValues = false;
        public IntReference MaxValue;
        public IntReference MinValue;
        public int StartupValue = 0;
        public int Value;


        private List<Action> ChangeEventActions = new List<Action>();

        void ForceSerialization()
        {
#if UNITY_EDITOR
            // Debug.Log("WTH:" + this.name);
            UnityEditor.EditorUtility.SetDirty(this);

#endif
        }

        // public int GetInfoBonus()
        // {
        //     return InfoBonusValue;//+ Mathf.RoundToInt(((float)InfoBonusValue)*InfoBonusMultiplier);
        // }
        // public int GetValueWithBonus()
        // {
        //     return Mathf.RoundToInt((float)(Value + InfoBonusValue) * (InfoBonusMultiplier + 1));
        // }

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
            SceneManager.sceneLoaded += OnSceneLoaded;

            if (ResetValueAfterStartup)
            {
                Value = StartupValue;
                ForceSerialization();
            }

            


        }

        public void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            if (ResetValueAfterStartup)
            {
                Value = StartupValue;
                ForceSerialization();
            }

          
        }

        private void OnSceneLoaded(Scene _scene, LoadSceneMode _mode)
        {
//TODO : does this do anything?
            ForceSerialization();
        }
        public void SetToMaxValue()
        {
            SetValue(MaxValue.Value);
        }

        public void SetToMinValue()
        {
            SetValue(MinValue.Value);
        }


        public void SetValue(int value)
        {
            // Debug.Log("Applying change: " + Value + " to: " + value);
            if (value == Value)
                return;

            if (HasMaxValue && MaxValue.Value < value)
            {
                if (LoopValues)
                    Value = MinValue.Value;  //SetValue?!Zacykleni? Break?
                else
                    Value = MaxValue.Value;

            }
            else if (HasMinValue && MinValue.Value > value)
            {
                if (LoopValues)
                    Value = MaxValue.Value;
                else
                    Value = MinValue.Value;

            }
            else
                Value = value;


            foreach (var item in ChangeEventActions)
            {
                if (item != null)
                    item.Invoke();
            }

            if (ChangeEvent != null)
                ChangeEvent.Raise(Value);

            ForceSerialization();
        }

        public void SetValue(IntVariable value)
        {
            //   Debug.Log("Applying change: " + Value + " to: "+ value.Value);
            if (value.Value == Value)
                return;

            if (HasMaxValue && MaxValue.Value < value.Value)
            {
                if (LoopValues)
                    Value = MinValue.Value;
                else
                    Value = MaxValue.Value;
            }
            else if (HasMinValue && MinValue.Value > value.Value)
            {
                if (LoopValues)
                    Value = MaxValue.Value;
                else
                    Value = MinValue.Value;

            }
            else
                Value = value.Value;


            foreach (var item in ChangeEventActions)
            {
                if (item != null)
                    item.Invoke();
            }

            if (ChangeEvent != null)
                ChangeEvent.Raise(Value);

            ForceSerialization();
        }

        public void Add(int amount)
        {
            SetValue(Value + amount);
        }

        public void Add(IntVariable amount)
        {
            SetValue(Value + amount.Value);
        }

        public void Sub(int amount)
        {
            SetValue(Value - amount);
        }

        public void Sub(IntVariable amount)
        {
            SetValue(Value - amount.Value);
        }

      

        public GameEvent_Int ChangeEvent;
    }

}