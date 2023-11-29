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

    [CreateAssetMenu(fileName = "Float", menuName = "Variables/Float")]
    public class FloatVariable : ScriptableObject
    {
#if UNITY_EDITOR
        [Multiline]
        public string DeveloperDescription = "";

#endif
       // public float InfoBonusValue;
         public float InfoBonusMultiplier;
        public bool ResetValueAfterStartup = false;
        public bool HasMaxValue = false;
        public bool HasMinValue = false;
        public bool LoopValues = false;
        public float MaxValue = 0;
        public float MinValue = 0;
        public float StartupValue = 0;
        public float Value;

        public bool IsPercentages = false;

        private List<Action> ChangeEventActions = new List<Action>();

        void ForceSerialization()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);

#endif
        }

      
        public void OnEnable()
        {
            SceneManager.sceneLoaded+= OnSceneLoaded;
            if (ResetValueAfterStartup)
            {
                Value = StartupValue;
                ForceSerialization();
            }

          
        }

        public void OnDisable()
        {
             SceneManager.sceneLoaded-= OnSceneLoaded;
            if (ResetValueAfterStartup)
            {
                Value = StartupValue;
                ForceSerialization();
            }
         
        }

          private void OnSceneLoaded(Scene _scene, LoadSceneMode _mode)
        {
                 
                //SetInfoBonusValue(0);
                ForceSerialization();
        }


        public void SetValue(float value)
        {
            if (value == Value)
                return;


            if (HasMaxValue && MaxValue < value)
            {
                if (LoopValues)
                    Value = MinValue;
                else
                    Value = MaxValue;

            }
            else if (HasMinValue && MinValue > value)
            {
                if (LoopValues)
                    Value = MaxValue;
                else
                    Value = MinValue;

            }
            else
                Value = value;


            if (ChangeEvent != null)
                ChangeEvent.Raise(Value);
        }

        public void SetValue(FloatVariable value)
        {
            if (value.Value == Value)
                return;

            if (HasMaxValue && MaxValue < value.Value)
            {
                if (LoopValues)
                    Value = MinValue;
                else
                    Value = MaxValue;
            }
            else if (HasMinValue && MinValue > value.Value)
            {
                if (LoopValues)
                    Value = MaxValue;
                else
                    Value = MinValue;

            }
            else
                Value = value.Value;


            if (ChangeEvent != null)
                ChangeEvent.Raise(Value);


        }

        public void Add(float amount)
        {
            SetValue(Value + amount);

        }

        public void Add(FloatVariable amount)
        {
            SetValue(Value + amount.Value);

        }

        public void Sub(float amount)
        {
            SetValue(Value - amount);

        }

        public void ApplyChangeSub(FloatVariable amount)
        {
            SetValue(Value - amount.Value);

        }


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


        public GameEvent_Float ChangeEvent;

    }
}