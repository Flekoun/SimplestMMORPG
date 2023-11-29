using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public abstract class UISelectableEntry : MonoBehaviour
{
    [Header("Selectable")]
    public GameObject SelectedImage;
    public List<GameObject> SelectedImages;
    public bool IsSelected = false;

    public void ToggleSelected()
    {
        IsSelected = !IsSelected;

        SetSelected(IsSelected);
        //if (SelectedImage != null)
        //    SelectedImage.gameObject.SetActive(IsSelected);

        //foreach (var item in SelectedImages)
        //    item.gameObject.SetActive(IsSelected);

    }

    public void SetSelected(bool _selected)
    {
        IsSelected = _selected;

        if (SelectedImage != null)
            SelectedImage.gameObject.SetActive(IsSelected);

        foreach (var item in SelectedImages)
            item.gameObject.SetActive(IsSelected);
    }

    public abstract string GetUid();

}
