using System.Collections;
using System.Collections.Generic;
using PrisonGlobals;
using RoboRyanTron.Unite2017.Variables;
using UnityEngine;
using UnityEngine.Events;

public class MathOperations_Int : MonoBehaviour
{
    public IntReference Value;

    public UnityEvent_Int OnOperationFinished;

    public void AddValue(int _value)
    {
       // Value.Value += _value;

        OnOperationFinished.Invoke(Value.Value + _value);

    }

    public void SubValue(int _value)
    {
       // Value.Value -= _value;

        OnOperationFinished.Invoke(Value.Value- _value);

    }

    public void SubValue(IntVariable _value)
    {
        SubValue(_value.Value);
    }

    public void AddValue(IntVariable _value)
    {
        AddValue(_value.Value);
    }

}
