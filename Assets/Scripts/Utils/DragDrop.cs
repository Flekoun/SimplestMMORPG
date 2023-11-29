using RoboRyanTron.Unite2017.Events;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DragDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector2 originalPosition;
    private Canvas canvas; // Dragging in UI space needs reference to the canvas to work properly.
    private bool isDroppedInZone = false;
    private CanvasGroup canvasGroup; // Reference to the CanvasGroup component

    public UnityAction<DropZone> OnDropedOnZone;
    public GameEvent_GameObject OnStartDrag;
    public GameEvent_GameObject OnDragOver;
    //public UnityAction OnHoverOverDropZone;
    //public UnityAction OnExitHoverOverDropZone;

    private void Awake()
    {
        // Get the CanvasGroup component
        canvas = UIManager.instance.MainCanvas;
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Store the original position of the object
        originalPosition = transform.position;

        OnStartDrag?.Raise(this.gameObject);


        // Don't block raycasts (so the raycast can "see through" this object to the drop zone)
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Convert the drag from screen space to canvas space
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, eventData.position, canvas.worldCamera, out Vector2 movePos);

        // Move the object with the mouse/touch
        transform.position = canvas.transform.TransformPoint(movePos);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        OnDragOver?.Raise(this.gameObject);
        // Block raycasts again
        canvasGroup.blocksRaycasts = true;

        // The object returns to its original position when it's not dropped on a valid target
        if (!isDroppedInZone)
        {
            ResetPosition();
        }
        // Reset the flag
        isDroppedInZone = false;
    }

    public void ResetPosition()
    {
        //   transform.position = originalPosition;
        transform.position = new Vector3(originalPosition.x, originalPosition.y, 0f); //nevim proc ale z to nastavuje na nejakych silecych -22000 proto takhle
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0f);
    }

    public void DroppedInZone(DropZone _dropZone)
    {
        // Called from DropZone to signal that a successful drop happened
        isDroppedInZone = true;
        OnDropedOnZone?.Invoke(_dropZone);
    }

}