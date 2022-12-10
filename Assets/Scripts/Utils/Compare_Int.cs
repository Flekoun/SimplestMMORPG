using System.Collections;
using System.Collections.Generic;
using RoboRyanTron.Unite2017.Variables;
using UnityEngine;
using UnityEngine.Events;

public class Compare_Int : MonoBehaviour {

    public IntReference Value;


    public IntReference ValueToCompareTempModifier;
    public UnityEvent OnIsEqual;
    public UnityEvent OnIsGreater;
    public UnityEvent OnIsLess;
    public UnityEvent OnIsGreaterOrEqual;
    public UnityEvent OnIsLessOrEqual;
    public UnityEvent OnIsNotEqual;

    //public void SetValue(int _value)
    //{

    //    Value = _value;
    //}

    public void CompareWith(int _int)
    {
    //    Debug.Log("comparing incoming: " + _int + " with : "+ Value.Value);
        int tmpValue = Value +ValueToCompareTempModifier.Value;
        if (_int == tmpValue  )
            OnIsEqual.Invoke();

        if (_int> tmpValue )
            OnIsGreater.Invoke();

        if (_int<tmpValue )
            OnIsLess.Invoke();

        if (_int <= tmpValue)
            OnIsLessOrEqual.Invoke();

        if (_int >= tmpValue)
            OnIsGreaterOrEqual.Invoke();

        if (_int != tmpValue)
            OnIsNotEqual.Invoke();
    }

    public void CompareWith(IntVariable _int)
    {
        CompareWith(_int.Value);
    }
}
