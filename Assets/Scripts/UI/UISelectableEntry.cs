using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public abstract class UISelectableEntry : MonoBehaviour
{
    [Header("Selectable")]
    public GameObject SelectedImage;
    public bool IsSelected = false;
   
    public void ToggleSelected()
    {
        IsSelected = !IsSelected;
        SelectedImage.gameObject.SetActive(IsSelected);

    }

    public void SetSelected(bool _selected)
    {
        IsSelected = _selected;
        SelectedImage.gameObject.SetActive(IsSelected);
    }

    public abstract string GetUid();
   
}
