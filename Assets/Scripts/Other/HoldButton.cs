using System.Collections;
using System.Collections.Generic;
using RoboRyanTron.Unite2017.Variables;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{

    //public UnityEvent onPointerDown;
    //public UnityEvent onPointerUp;
    //public UnityEvent whilePointerPressed;


    public FloatReference HoldDuration;
    public Image HoldProgressImage;

    private Button _button;
    private float currentHoldTime = 0f;

    private bool functional = true;

    float currentValue = 0;
    float currentValueSlow = 0;
    float t = 0;

    public void SetFunctional(bool _functional)
    {
        functional = _functional;
    }

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    public bool IsFunctional()
    {
        return functional;
    }

    private IEnumerator WhilePressed()
    {


        // this looks strange but is okey in a Coroutine
        // as long as you yield somewhere
        while (currentHoldTime < HoldDuration)
        {
            //  whilePointerPressed?.Invoke();
            currentHoldTime += 0.05f;
            RefreshProgress();


            yield return new WaitForSecondsRealtime(0.05f);
        }

        onHoldFinished.Invoke();
    }

    //private void Pressed()
    //{
    //    whilePointerPressed?.Invoke();
    //}

    public void OnPointerDown(PointerEventData eventData)
    {
        // ignore if button not interactable
        if (!_button.interactable || !functional) return;

        // just to be sure kill all current routines
        // (although there should be none)
        StopAllCoroutines();
        //   InvokeRepeating("Pressed", 1, 1);
        StartCoroutine(WhilePressed());

        //  onPointerDown?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        currentHoldTime = 0;
        RefreshProgress();

        StopAllCoroutines();
        //  onPointerUp?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        currentHoldTime = 0;
        RefreshProgress();

        StopAllCoroutines();
        //   onPointerUp?.Invoke();
    }

    private void RefreshProgress()
    {
        //  HoldProgressImage.fillAmount = currentHoldTime / HoldDuration;
      //  currentValueSlow = currentValue;
        currentValue =currentHoldTime / HoldDuration;
      
    }

    void Update()
    {
        //interpolating slowHP and currentHP inf unequal
        if (currentValueSlow != currentValue)
        {
            currentValueSlow = Mathf.Lerp(currentValueSlow, currentValue, t);
            t += 0.5f * Time.deltaTime;

            HoldProgressImage.fillAmount = currentValueSlow;
        }
        else
        {
            t = 0;
            //resetting interpolator
        }

       
        //Setting fill amount
     //   barFast.fillAmount = currHP / maxHP;
       // barSlow.fillAmount = currHPSlow / maxHP;
    }

    // Afaik needed so Pointer exit works .. doing nothing further
    public void OnPointerEnter(PointerEventData eventData) { }


    public UnityEvent onHoldFinished;

}
