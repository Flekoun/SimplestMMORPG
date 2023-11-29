using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIQualityProgress : MonoBehaviour
{

    public RectTransform ProgressUI;
    public RectTransform BackProgress;
    public RectTransform FrontProgress;
    public Image FrontImage;
    public Image BackImage;
    public int ProgressElements = 5;
    private float OriginalWidth;
    private float OriginalHeight;
    private float SingleStarWidth;


    private bool Initialized = false;
    public void OnEnable()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (!Initialized)
        {

            OriginalWidth = ProgressUI.sizeDelta.x;
            OriginalHeight = ProgressUI.sizeDelta.y;
            SingleStarWidth = OriginalWidth / ProgressElements;
            //  Debug.Log("SingleStarWidth:" + SingleStarWidth);
            // Debug.Log("OriginalWidth:" + OriginalWidth);
            // Debug.Log("OriginalHeight:" + OriginalHeight);
            Initialized = true;
        }
    }

    public void Setup(int _quality, int _qualityMax, Sprite _frontImage = null, Sprite _backImage = null)
    {
        Initialize();
        //    ProgressUI.sizeDelta = new Vector2(OriginalWidth, OriginalHeight);

        ProgressUI.sizeDelta = new Vector2(SingleStarWidth * _qualityMax, ProgressUI.sizeDelta.y);
        BackProgress.sizeDelta = new Vector2(ProgressUI.sizeDelta.x, ProgressUI.sizeDelta.y);

        FrontProgress.sizeDelta = new Vector2(SingleStarWidth * _quality, FrontProgress.sizeDelta.y);

        if (_frontImage != null)
            FrontImage.sprite = _frontImage;
        if (_backImage != null)
            BackImage.sprite = _backImage;
    }
}
