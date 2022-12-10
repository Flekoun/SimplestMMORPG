using System.Collections;
using System.Collections.Generic;
using RoboRyanTron.Unite2017.Variables;
using UnityEngine;
using UnityEngine.Events;

public class DiceRoller : MonoBehaviour
{

    public bool RollAtStart = false;
    // [Range(0,100)]
    public FloatVariable ChanceToSucceed;
    public UnityEvent OnSuccess;
    public UnityEvent OnFailure;

    private int successCount = 0;
    private int failCount = 0;

    public void RollDice(int _count)
    {
        for (int i = 0; i < _count; i++)
        {
            RollDice();
        }
    }

    public void Start()
    {
        if(RollAtStart)
            RollDice();
    }

    public void RollDice()
    {

        float result = Random.Range(0f, 1f);


        if (result <= ChanceToSucceed.Value)
            Success();
        else
            Fail();


        //NECHAPU CO SEM TIMTO MYSLEL...
        // float chanceIndex = (100f / ChanceToSucceed.Value); //Tady byla hodnota 0-100
        // float minValue = 0f;
        // float maxValue = 1f;

        // if(chanceIndex > 1f)
        // {
        //     maxValue = chanceIndex;
        // }

        // float result = Random.Range(minValue, maxValue);

        // if (chanceIndex >= 1f)
        // {
        //     if (result < 1f)
        //         Success();
        //     else
        //         Fail();
        // }
        // else
        // {
        //     if (result > chanceIndex)
        //         Success();
        //     else
        //         Fail();
        // }
    }

    private void Success()
    {
        //   Debug.Log("Success!");
        OnSuccess.Invoke();
        successCount++;
        //  DebugChance();
    }

    private void Fail()
    {
        // Debug.Log("Fail!");
        OnFailure.Invoke();
        failCount++;
        //  DebugChance();
    }

    private void DebugChance()
    {
        Debug.Log("Calculated Chance = " + ((float)successCount / (float)(successCount + failCount)) * 100f + "%");
    }


}
