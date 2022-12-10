using System.Collections;
using System.Collections.Generic;
using RoboRyanTron.Unite2017.Variables;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class TimeTrigger : MonoBehaviour {

	public bool StartUponInit = false;
	//public bool IsRepeating;


    [Header("Basic Settings")]
    [Tooltip("The OnTriggerStarted will be called after this delay and trigger will start repeating/working")]
    public FloatReference InitDelay;
    [Tooltip("The Trigger will over after given time passed")]
    public FloatReference AutoTurnOffAfterTime;
    [Tooltip("The Trigger will over after given number of ticks")]
    public IntReference AutoTurnOffAfterTicks;

    [Header("TimeBased Settings")]
    public FloatReference RepeatTimeMin;
    public FloatReference RepeatTimeMax;

    [Header("Repeat Count Settings")]
   

    private bool IsOn=false;
    [HideInInspector]
    public int numberOfTicksPassed = 0;

    public UnityEvent OnTriggerTick;
    public UnityEvent OnTriggerStarted;
    public UnityEvent OnTriggerEnded;

  
    public void Start()
	{
     

        if (StartUponInit)
			SetTrigger(true);
	}

    public void DisableTimer()
    {
        StopAllCoroutines();
        IsOn = false;
    }

public void SetAutoTurnOffAfterTime(float _time)
{
    AutoTurnOffAfterTime.Value = _time;
}

	public void SetTrigger(bool _setOn)
	{
        if (this.gameObject.activeSelf)
        {
            if (_setOn)
            {
                numberOfTicksPassed = 0;

                if (!IsOn)
                {
                    IsOn = true;
                   
                    StartCoroutine("InitDelayTimer");
                }

            }
            else
            {
                if (IsOn)
                {

                    StopAllCoroutines();
                    IsOn = false;
                    CancelInvoke();
                    OnTriggerEnded.Invoke();
                }
            }
        }
	}

    public void ManualTick()
    {
         OnTriggerTick.Invoke();
    }
    
	private IEnumerator Timer()
	{

        yield return new WaitForSeconds(Random.Range(RepeatTimeMin.Value, RepeatTimeMax.Value));

        numberOfTicksPassed++;
        OnTriggerTick.Invoke();
     

        if (AutoTurnOffAfterTicks.Value > 0 && numberOfTicksPassed >= AutoTurnOffAfterTicks.Value)
        {
            //OnTriggerEnded.Invoke();
            SetTrigger(false);
        }
        else if (RepeatTimeMax.Value > 0 && IsOn)
            StartCoroutine("Timer");
	

	}
		
	private IEnumerator InitDelayTimer()
	{

        yield return new WaitForSeconds(InitDelay.Value);
        //	OnTrigger.Invoke();
        OnTriggerStarted.Invoke();


        if (RepeatTimeMax.Value > 0 && IsOn)
            StartCoroutine("Timer");
      //  else
      //  {
      //  
       //     SetTrigger(false);
       // }


        if (AutoTurnOffAfterTime.Value > 0)
			StartCoroutine("AutoTurnOff");

	}


	private IEnumerator AutoTurnOff()
	{

		yield return new WaitForSeconds(AutoTurnOffAfterTime.Value+0.25f); //TODO: Just trying hack to let ticks that ends at the same second as a duration to be performed
       // OnTriggerEnded.Invoke();
        SetTrigger(false);


	}

    public float GetTotalTimeUntilTimerIsOver()
    {

        if (AutoTurnOffAfterTime.Value > 0)
            return AutoTurnOffAfterTime.Value;
        else if (AutoTurnOffAfterTicks.Value > 0)
        {
         
            if (RepeatTimeMin.Value == RepeatTimeMax.Value)
            {
            
                return AutoTurnOffAfterTicks.Value * RepeatTimeMin.Value;
            }
            else
                return AutoTurnOffAfterTicks.Value * (RepeatTimeMin.Value + RepeatTimeMax.Value) / 2;
        }
        else
            return -1;
    }

    public int GetTotalTicksCountUntilTimerIsOver()
    {
        if (AutoTurnOffAfterTicks.Value > 0)
            return AutoTurnOffAfterTicks.Value;
        else return -1;

    }
}


//[CustomEditor(typeof(TimeTrigger))]
//public class TimeTriggerEditor : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        TimeTrigger timeTriger = target as TimeTrigger;

//        timeTriger.StartUponInit = GUILayout.Toggle(timeTriger.StartUponInit, "StartUponInit");

//        if (timeTriger.StartUponInit)
//            timeTriger.RepeatTimeMin.Value = EditorGUILayout.FloatField(timeTriger.RepeatTimeMin.Value);

//    }
//}
