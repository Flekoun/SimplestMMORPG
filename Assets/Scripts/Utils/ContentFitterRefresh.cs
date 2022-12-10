using System.Collections;
using System.Collections.Generic;
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
        var rectTransform = (RectTransform)transform;
        RefreshContentFitter(rectTransform);
    }

    private void RefreshContentFitter(RectTransform transform)
    {
        if (transform == null || !transform.gameObject.activeSelf)
        {
            return;
        }

        foreach (RectTransform child in transform)
        {
            RefreshContentFitter(child);
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
                Debug.LogWarning("Chyba pri refreshovani content size fitteru.....");
            }
          
        }

        if (contentSizeFitter != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
        }
    }
}
