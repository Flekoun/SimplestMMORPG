using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;


public class SwipeDetector : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Vector2 startTouchPosition;
    private bool swipeInProgress = false;
    public UnityAction OnSwipeUpPerformed;

    // Set a minimum distance for the swipe (you can adjust this value in the Inspector)
    public float minSwipeDistance = 100f;

    public void OnPointerDown(PointerEventData eventData)
    {
        // Store the starting point of the swipe
        startTouchPosition = eventData.position;
        swipeInProgress = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!swipeInProgress)
        {
            return;
        }
        else
        {
            swipeInProgress = false;
        }

        // Calculate the swipe direction and distance
        Vector2 swipeDirection = eventData.position - startTouchPosition;
        float swipeDistance = swipeDirection.magnitude;
        swipeDirection.Normalize();

        // Check if the swipe was upward and long enough
        if (Vector2.Dot(swipeDirection, Vector2.up) > 0.5 && swipeDistance > minSwipeDistance)
        {
            OnSwipeUpPerformed.Invoke();
            // Here you can implement whatever you want to happen when a swipe up is detected
        }
    }
}
