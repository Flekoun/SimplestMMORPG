using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ContentFitterRefresh : MonoBehaviour
{
    private void Awake()
    {
        RefreshContentFitters();
    }

    public void RefreshContentFitters()
    {
        if (transform is RectTransform)
        {
            var rectTransform = (RectTransform)transform;
            RefreshContentFitter(rectTransform);
        }
    }

    private void RefreshContentFitter(RectTransform transform)
    {
        if (transform == null || !transform.gameObject.activeSelf)
        {
            return;
        }

        foreach (var child in transform)
        {
            if (child is RectTransform)
                RefreshContentFitter(child as RectTransform);
        }

        var layoutGroup = transform.GetComponent<LayoutGroup>();
        var contentSizeFitter = transform.GetComponent<ContentSizeFitter>();
        if (layoutGroup != null)
        {
            try
            {
                layoutGroup.SetLayoutHorizontal();

                if (layoutGroup != null)
                    layoutGroup.SetLayoutVertical();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("Chyba pri refreshovani content size fitteru....." + ex.Message);
            }

        }

        if (contentSizeFitter != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
        }
    }





}
