using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UIProgressBar : MonoBehaviour
{
    public Image FrontImage;
    public TextMeshProUGUI Text;

    private int maxValue = -1;

    public void SetMaxValue(int _maxAmount)
    {
        maxValue = _maxAmount;
    }

    public void SetValues(int _maxAmount, int _amount)
    {
        maxValue = _maxAmount;
        SetValue(_amount);
    }
    // Start is called before the first frame updatepu
    public void SetValue(int _amount)
    {
        if (maxValue == -1)
        {
            Debug.LogError("Max value not set! Set if before you set value!");
            return;
        }
        //    Mathf.Lerp()
        FrontImage.fillAmount = (float)((float)_amount / (float)maxValue);

        Text.SetText(_amount.ToString() + "/" + maxValue.ToString());
    }



    // Update is called once per frame
    void Update()
    {

    }
}
