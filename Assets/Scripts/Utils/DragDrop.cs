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


//using UnityEngine;
//using UnityEngine.EventSystems;
//using System.Collections.Generic;
//using RoboRyanTron.Unite2017.Events;
//using UnityEngine.Events;

//public class DragDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler
//{
//    private Vector2 originalPosition;
//    private Canvas canvas;
//    private bool isDroppedInZone = false;
//    private CanvasGroup canvasGroup;

//    public UnityAction<DropZone> OnDropedOnZone;
//    public GameEvent_GameObject OnStartDrag;
//    public GameEvent_GameObject OnDragOver;

//    private static List<DragDrop> draggedObjects = new List<DragDrop>();
//    private static bool isDragging = false; // To check if any object is currently being dragged

//    private void Awake()
//    {
//        canvas = UIManager.instance.MainCanvas;
//        canvasGroup = GetComponent<CanvasGroup>();
//    }

//    public void OnBeginDrag(PointerEventData eventData)
//    {
//        originalPosition = transform.position;
//        OnStartDrag?.Raise(this.gameObject);
//        canvasGroup.blocksRaycasts = false;

//        draggedObjects.Add(this);
//        isDragging = true;
//    }

//    public void OnDrag(PointerEventData eventData)
//    {
//        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, eventData.position, canvas.worldCamera, out Vector2 movePos);

//        foreach (var dragObject in draggedObjects)
//        {
//            dragObject.transform.position = canvas.transform.TransformPoint(movePos);
//        }
//    }

//    //public void OnDrag(PointerEventData eventData)
//    //{
//    //    RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, eventData.position, canvas.worldCamera, out Vector2 movePos);

//    //    Vector2 basePosition = canvas.transform.TransformPoint(movePos);


//    //    for (int i = 0; i < draggedObjects.Count; i++)
//    //    {
//    //        // Calculate an offset for each object based on its position in the list
//    //        float offsetX = i * 10; // Change 10 to whatever offset value you prefer
//    //        float offsetY = i * 10; // Same here, adjust for vertical offset if needed

//    //        // Apply the calculated position
//    //        // draggedObjects[i].transform.position = new Vector3(basePosition.x + offsetX, basePosition.y + offsetY, 0f);
//    //        draggedObjects[i].gameObject.transform.position = basePosition;
//    //    }
//    //}

//    public void OnEndDrag(PointerEventData eventData)
//    {
//        OnDragOver?.Raise(this.gameObject);
//        canvasGroup.blocksRaycasts = true;

//        if (!isDroppedInZone)
//        {
//            ResetPosition();
//        }

//        draggedObjects.Clear();
//        isDragging = false;


//    }

//    public void OnPointerEnter(PointerEventData eventData)
//    {
//        //  if (isDragging && !draggedObjects.Contains(this))
//        //  {
//        // draggedObjects.Add(this);
//        // }

//        UIManager.instance.AddDraggedSkillToList(this);
//    }

//    public void ResetPosition()
//    {
//        //transform.position = new Vector3(originalPosition.x, originalPosition.y, 0f);
//        //transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0f);

//        UIManager.instance.ClearList();
//    }

//    public void DroppedInZone(DropZone _dropZone)
//    {
//        isDroppedInZone = true;
//        OnDropedOnZone?.Invoke(_dropZone);
//    }

//    private void AddObjectToDragged()
//    {

//    }
//}

