using System.Collections;
using System.Collections.Generic;
using RoboRyanTron.Unite2017.Variables;
using UnityEngine;
using UnityEngine.UI;

public class UIImageProgressTimeDisplayer : MonoBehaviour
{
    public Image ProgressImage;
    public float ProgressTotalTime;
 
    private float fillStep;

    public void SetProgressTime(float _progressTime)
    {
        ProgressTotalTime = _progressTime;
        StartProgress();
    }

    private void StartProgress()
    {
        CancelInvoke();
        ProgressImage.fillAmount = 0f;
        fillStep =1f/ (ProgressTotalTime*10f);
//        Debug.Log("fillStep: " + fillStep);
        InvokeRepeating("Progress",0,0.1f);

    }

    private void Progress()
    {
        ProgressImage.fillAmount+= fillStep;

        if (System.Math.Abs(ProgressImage.fillAmount - 1f) < 0.01f)
        {
            ProgressImage.fillAmount = 0;
            CancelInvoke();
        }

    }


}
