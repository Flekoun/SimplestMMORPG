using UnityEngine;
using UnityEngine.EventSystems;

public class DragDropLine : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private LineRenderer lineRenderer; // Reference to the LineRenderer component
    public Canvas canvas; // Reference to the canvas

    private void Awake()
    {
        canvas = UIManager.instance.MainCanvas;
        // Get the LineRenderer component
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Activate the line renderer and set its first position to the card
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, transform.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Convert the pointer position from screen space to canvas space
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, eventData.position, canvas.worldCamera, out Vector2 movePos);

        // Update the second position of the line renderer to the pointer position
        lineRenderer.SetPosition(1, canvas.transform.TransformPoint(movePos));
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Deactivate the line renderer
        lineRenderer.enabled = false;
    }
}
