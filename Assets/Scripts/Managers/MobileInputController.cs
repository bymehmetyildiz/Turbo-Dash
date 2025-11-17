using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileInputController : MonoBehaviour
{
    public static MobileInputController instance;

    private Vector2 startTouch;
    private Vector2 swipeDelta;
    private bool isSwiping;

    public bool SwipeLeft;
    public bool SwipeRight;
    public bool SwipeUp;
    public bool SwipeDown;
    public bool Tap;

    private float deadZone = 80f; // minimum swipe distance in pixels

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        SwipeLeft = SwipeRight = SwipeUp = SwipeDown = Tap = false;

        // ---------------------
        // Detect mouse tap (editor only)
        // ---------------------
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            Tap = true;
            isSwiping = true;
            startTouch = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            ResetSwipe();
        }

        if (isSwiping)
            swipeDelta = (Vector2)Input.mousePosition - startTouch;
#endif

        // ---------------------
        // Detect touch (mobile)
        // ---------------------
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);

            if (t.phase == TouchPhase.Began)
            {
                Tap = true;
                isSwiping = true;
                startTouch = t.position;
            }
            else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
            {
                ResetSwipe();
            }

            if (isSwiping)
                swipeDelta = t.position - startTouch;
        }

        // ---------------------
        // Process swipe
        // ---------------------
        if (swipeDelta.magnitude > deadZone)
        {
            float x = swipeDelta.x;
            float y = swipeDelta.y;

            if (Mathf.Abs(x) > Mathf.Abs(y))
            {
                SwipeLeft = x < 0;
                SwipeRight = x > 0;
            }
            else
            {
                SwipeUp = y > 0;
                SwipeDown = y < 0;
            }

            ResetSwipe();
        }
    }

    private void ResetSwipe()
    {
        startTouch = Vector2.zero;
        swipeDelta = Vector2.zero;
        isSwiping = false;
    }
}
