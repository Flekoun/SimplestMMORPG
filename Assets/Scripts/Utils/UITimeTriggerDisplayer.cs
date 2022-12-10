using System.Collections;
using System.Collections.Generic;
using RoboRyanTron.Unite2017.Variables;
using UnityEngine;
using UnityEngine.UI;

public class UITimeTriggerDisplayer : MonoBehaviour
{
    public Text Value_Text;
    public StringReference Prefix;
    public StringReference Suffix;
    public TimeTrigger TimeTriggerToDisplay;

   
    private float timeLeft = 0;

    private void CalculateTimeLeft()
    {
       //if( TimeTriggerToDisplay.AutoTurnOffAfterTicks>0)
       // {
       //     if (System.Math.Abs(TimeTriggerToDisplay.RepeatTimeMax.Value - TimeTriggerToDisplay.RepeatTimeMin.Value) < 0.1f)
       //     {
       //         timeLeft = TimeTriggerToDisplay.AutoTurnOffAfterTicks * TimeTriggerToDisplay.RepeatTimeMin;
       //         timeLeft -= TimeTriggerToDisplay.numberOfTicksPassed * TimeTriggerToDisplay.RepeatTimeMin;
       //     }
       //     else
       //         timeLeft = 1234;
       // }
       //else if (TimeTriggerToDisplay.AutoTurnOffAfterTime > 0)
       // {

       //     if (TimeTriggerToDisplay.RepeatTimeMax.Value == TimeTriggerToDisplay.RepeatTimeMin.Value)
       //     {
       //         timeLeft = TimeTriggerToDisplay.AutoTurnOffAfterTime;
       //         timeLeft -= TimeTriggerToDisplay.numberOfTicksPassed * TimeTriggerToDisplay.RepeatTimeMin;
       //     }
       //     else
       //         timeLeft = 1234;
       // }


    }
    public void Start()
    {
        Restart();
    }

    public void OnEnable()
    {
        Refresh();
    }
    public void Refresh()
    {
       
        CalculateTimeLeft();
      
        //   if (ValueToDisplay.Value!= null)
        Value_Text.text = Prefix + timeLeft.ToString() + Suffix;
        timeLeft--;
        //    else if (ValueToDisplay_Float.Variable != null)
        //         Value_Text.text = Prefix + ValueToDisplay_Float.Value.ToString() + Suffix;
        //    else
        //        Value_Text.text = "ERROR";
    }

    public void Restart()
    {
        CancelInvoke();
        timeLeft = TimeTriggerToDisplay.GetTotalTimeUntilTimerIsOver();
        InvokeRepeating("Refresh", 0f, 1f);
    }


    //private IEnumerator Timer()
    //{
    //    yield return new WaitForSeconds(1);
    //    Refresh();
    //}

}
