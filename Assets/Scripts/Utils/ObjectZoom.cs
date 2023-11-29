using UnityEngine;

public class ObjectZoom : MonoBehaviour
{
    public float zoomSpeedTouch = 0.005f;
    public float zoomSpeedMouse = 0.5f;
    public float minZoom = 0.5f;
    public float maxZoom = 2.0f;

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
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            Zoom(deltaMagnitudeDiff * zoomSpeedTouch);
        }
    }

    void HandleMouseZoom()
    {
        float scrollData = Input.GetAxis("Mouse ScrollWheel");
        Zoom(scrollData * zoomSpeedMouse);
    }

    void Zoom(float increment)
    {
        float newScale = Mathf.Clamp(rectTransform.localScale.x + increment, minZoom, maxZoom);
        rectTransform.localScale = new Vector3(newScale, newScale, newScale);
    }
}
