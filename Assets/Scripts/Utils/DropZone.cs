using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    private bool IsActive = false;

    public UnityEvent OnDragDropStartHoveringOverThisDropZone;
    public UnityEvent OnDragDropEndHoveringOverThisDropZone;
    public void OnDrop(PointerEventData eventData)
    {
        // Check if the dropped object is the one you want
        DragDrop dragDrop = eventData.pointerDrag?.GetComponent<DragDrop>();
        if (dragDrop != null)
        {
        //    Debug.Log("Dropped " + eventData.pointerDrag.name + " on " + gameObject.name);
            // Snap the draggable object to the center of the drop zone
            eventData.pointerDrag.transform.position = transform.position;

            // Signal the DragDrop script that a successful drop happened
            dragDrop.DroppedInZone(this);

            // You can add more logic here to handle the successful drop (e.g., change UI, trigger a game event, etc.)
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
       // Debug.Log("Pointer entered over dropzone X " + gameObject.name);
        DragDrop dragDrop = eventData.pointerDrag?.GetComponent<DragDrop>();
        if (dragDrop != null)
        {
            IsActive = true;
            //Debug.Log("Pointer entered over dropzone " + gameObject.name);

            OnDragDropStartHoveringOverThisDropZone.Invoke();
            //dragDrop.OnHoverOverDropZone?.Invoke();
        }

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DragDrop dragDrop = eventData.pointerDrag?.GetComponent<DragDrop>();
        if (IsActive)
        {
           // Debug.Log("Pointer exited from active dropzone " + gameObject.name);
            IsActive = false;

            OnDragDropEndHoveringOverThisDropZone.Invoke();
            //   dragDrop.OnExitHoverOverDropZone?.Invoke();
        }
    }
}
