using UnityEngine;
using UnityEngine.UI;

public class ObjectZoom : MonoBehaviour
{
    public float zoomSpeedTouch = 0.005f;
    public float zoomSpeedMouse = 0.5f;
    public float minZoom = 0.5f;
    public float maxZoom = 2.0f;
    public ScrollRect ScrollRect;

    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (Application.isMobilePlatform)
        {
            HandleTouchZoom();
        }
        else
        {
            HandleMouseZoom();
        }
    }

    void HandleTouchZoom()
    {
        if (Input.touchCount == 2)
        {
            if (ScrollRect != null)
                ScrollRect.enabled = false;

            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            // Calculate the midpoint of the two touches
            Vector2 touchMidpoint = (touchZero.position + touchOne.position) / 2;

            ZoomAtPoint(deltaMagnitudeDiff * (-1) * zoomSpeedTouch, touchMidpoint);
        }
        else if (ScrollRect != null)
        {
            ScrollRect.enabled = true;
        }
    }

    //void HandleTouchZoom()
    //{
    //    if (Input.touchCount == 2)
    //    {
    //        if (ScrollRect != null)
    //            ScrollRect.enabled = false;

    //        Touch touchZero = Input.GetTouch(0);
    //        Touch touchOne = Input.GetTouch(1);

    //        Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
    //        Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

    //        float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
    //        float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

    //        float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

    //        Zoom(deltaMagnitudeDiff * zoomSpeedTouch);


    //    }
    //    else if (ScrollRect != null)
    //        ScrollRect.enabled = true;
    //}

    //void HandleMouseZoom()
    //{
    //    float scrollData = Input.GetAxis("Mouse ScrollWheel");
    //    Zoom(scrollData * zoomSpeedMouse);
    //}

    void HandleMouseZoom()
    {
        float scrollData = Input.GetAxis("Mouse ScrollWheel");
        if (scrollData != 0)
        {
            if (ScrollRect != null)
                ScrollRect.enabled = false;

            Vector2 mousePosition = Input.mousePosition;
            ZoomAtPoint(scrollData * zoomSpeedMouse, mousePosition);
        }
        else if (ScrollRect != null)
            ScrollRect.enabled = true;
    }


    void Zoom(float increment)
    {
        float newScale = Mathf.Clamp(rectTransform.localScale.x + (increment * (-1)), minZoom, maxZoom);
        rectTransform.localScale = new Vector3(newScale, newScale, newScale);
    }

    void ZoomAtPoint(float increment, Vector2 screenPoint)
    {
        // Calculate the new scale
        float newScale = Mathf.Clamp(rectTransform.localScale.x + increment, minZoom, maxZoom);

        // Convert screen point to local point in RectTransform space
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, Camera.main, out Vector2 localPoint);

        // Calculate the position change
        Vector3 localPointInOldScale = new Vector3(localPoint.x * rectTransform.localScale.x, localPoint.y * rectTransform.localScale.y, 0f);
        Vector3 localPointInNewScale = new Vector3(localPoint.x * newScale, localPoint.y * newScale, 0f);
        Vector3 positionChange = localPointInNewScale - localPointInOldScale;

        // Apply the scale and position changes
        rectTransform.localScale = new Vector3(newScale, newScale, newScale);
        rectTransform.localPosition -= positionChange;
    }

}
