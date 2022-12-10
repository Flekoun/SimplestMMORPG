using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MonobehaviorEventHandlers : MonoBehaviour
{
    public void Awake()
    {
        OnAwake.Invoke();
    }

    public void Start()
    {
        OnStart.Invoke();
    }
    // Start is called before the first frame update
    void OnEnable()
    {
        OnEnabled.Invoke();
    }

    // Update is called once per frame
    void OnDisable()
    {
        OnDisabled.Invoke();
    }
    public void OnDestroy()
    {
        OnDestroyed.Invoke();
    }
    public UnityEvent OnAwake;
    public UnityEvent OnEnabled;
    public UnityEvent OnStart;

    public UnityEvent OnDisabled;
    public UnityEvent OnDestroyed;
}
