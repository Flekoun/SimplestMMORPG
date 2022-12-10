using System.Collections;
using System.Collections.Generic;
using PrisonGlobals;
using RoboRyanTron.Unite2017.Variables;
using UnityEngine;
using UnityEngine.UI;

public class UIImageProgressValueDisplayer : MonoBehaviour
{
    public Image ProgressImage;
    public IntReference ProgressValue;
     public IntReference ProgressMax;
   
   public IntReference BonusValue;

    public void SetBonusValue(int _bonusValue)
    {
            BonusValue.Value = _bonusValue;
            ShowProgress();
    
    }

    public void SetProgressValue(int _progressValue)
    {
       
        ProgressValue.Value = _progressValue;
        ShowProgress();
        OnProgressValueSet.Invoke(_progressValue);
    }

     public void SetProgressMax(int _progressMax)
    {
        ProgressMax.Value = _progressMax;
        ShowProgress();
        OnProgressMaxValueSet.Invoke(_progressMax);
    }

    public void ShowProgress()
    {  
        ProgressImage.fillAmount=  ((float)ProgressValue.Value+(float)BonusValue)/(float)ProgressMax.Value;
    }


    public UnityEvent_Int OnProgressValueSet;
    public UnityEvent_Int OnProgressMaxValueSet;

}
