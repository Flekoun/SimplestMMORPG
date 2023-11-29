using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoboRyanTron.Unite2017.Variables;

public class UIProgressBar : MonoBehaviour
{
    public SlicedFilledImage FrontImage;
    public SlicedFilledImage FrontLazyTargetImage;
    public SlicedFilledImage PenaltyImage;
    public SlicedFilledImage LeastValueImage;
    public Image FrontImage_NormalImage;
    public TextMeshProUGUI Text;
    public TextMeshProUGUI PenaltyText;
    public bool IsLazy = false;
    public FloatReference LazyDurationSeconds;
    public FloatReference LazyDelaySeconds;
    public bool HidePenalty = false;
    public bool HidePenaltyText = false;
    private float OldAmount = 0;

    private int penaltyAmount = 0;

    private int maxValueWithoutPenalty = -1;
    private int maxValue = -1;

    public void SetMaxValue(int _maxAmount)
    {
        maxValue = _maxAmount;
        maxValueWithoutPenalty = _maxAmount;
    }

    public void SetValues(int _maxAmount, int _amount, int _penaltyAmount = 0, bool _ignoreLazdy = false)
    {
        penaltyAmount = _penaltyAmount;
        if (HidePenalty)
            penaltyAmount = 0;

        maxValueWithoutPenalty = _maxAmount;
        maxValue = maxValueWithoutPenalty + penaltyAmount;

        if (_penaltyAmount > 0)
        {
            PenaltyImage.fillAmount = (float)((float)_penaltyAmount / (float)maxValue);
            PenaltyText.SetText(_penaltyAmount.ToString());

            PenaltyImage.gameObject.SetActive(!HidePenalty);
            PenaltyText.gameObject.SetActive(!HidePenalty && !HidePenaltyText && _amount > 0);
        }
        else
        {
            if (PenaltyImage != null)
            {
                PenaltyImage.fillAmount = 0;
                PenaltyImage.gameObject.SetActive(false);
            }
            if (PenaltyText != null)
            {
                PenaltyText.SetText(_penaltyAmount.ToString());
                PenaltyText.gameObject.SetActive(false);
            }
        }


        SetValue(_amount, _ignoreLazdy);

    }





    public void SetLeastValueImage(int _amount)
    {
        LeastValueImage.fillAmount = (float)((float)_amount / (float)maxValue);
    }


    public void SetValue(int _amount, bool _ignoreLazdy = false)
    {

        if (maxValue == -1)
        {
            Debug.LogError("Max value not set! Set if before you set value!");
            return;
        }

        if (FrontLazyTargetImage != null)
            FrontLazyTargetImage.gameObject.SetActive(IsLazy);

        if (!IsLazy || _ignoreLazdy)
        {
            UpdateFrontProgressBar(_amount);
            UpdateLazyProgressBar(_amount);
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(UpdateProgress(_amount, OldAmount));
        }

        OldAmount = _amount;
    }

    private IEnumerator UpdateProgress(float _targetAmount, float _oldAmount)
    {
        float timeStep = 0.05f;
        float lazyAmount = _oldAmount;
        float stepAmount = Mathf.Abs(lazyAmount - _targetAmount) / (LazyDurationSeconds.Value / timeStep);

        if (LazyDelaySeconds.Value > 0)
            yield return new WaitForSecondsRealtime(LazyDelaySeconds.Value);

        if (_targetAmount < _oldAmount)
        {
            UpdateLazyProgressBar(_targetAmount);

            while (Mathf.Abs(lazyAmount - _targetAmount) >= 0.05)
            {
                lazyAmount -= stepAmount;
                UpdateFrontProgressBar(lazyAmount);

                yield return new WaitForSecondsRealtime(timeStep);
            }

        }
        else
        {
            UpdateFrontProgressBar(_targetAmount);

            while (Mathf.Abs(lazyAmount - _targetAmount) >= 0.05)
            {
                lazyAmount += stepAmount;
                UpdateLazyProgressBar(lazyAmount);

                yield return new WaitForSecondsRealtime(timeStep);
            }

        }

        UpdateLazyProgressBar(_targetAmount);
        UpdateFrontProgressBar(_targetAmount);


    }

    private void UpdateLazyProgressBar(float _amount)
    {

        if (FrontLazyTargetImage != null)
            FrontLazyTargetImage.fillAmount = (float)((float)_amount / (float)maxValue);
        else
            Debug.LogWarning("You forgot to assign lazy Image! : " + this.gameObject.name);
    }

    private void UpdateFrontProgressBar(float _amount)
    {

        if (Text != null)
            Text.SetText(Mathf.FloorToInt(_amount).ToString() + "/" + maxValueWithoutPenalty.ToString());

        if (FrontImage != null)
            FrontImage.fillAmount = (float)((float)_amount / (float)maxValue);
        else if (FrontImage_NormalImage != null)
            FrontImage_NormalImage.fillAmount = (float)((float)_amount / (float)maxValue);
        else
            Debug.LogWarning("You forgot to assign progress Image! : " + this.gameObject.name);
    }
}
