using UnityEngine;

public class MobileInputController : MonoBehaviour
{
    public static MobileInputController instance;

    private Vector2 startTouch;
    private float startTime;
    private bool isTracking;
    private bool swipeFired;

    public bool SwipeLeft;
    public bool SwipeRight;
    public bool SwipeUp;
    public bool SwipeDown;
    public bool Tap;

    private const float maxTapDuration = 0.3f;
    private float deadZone = 80f; // minimum swipe distance in pixels, rescaled to the screen every frame

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        SwipeLeft = SwipeRight = SwipeUp = SwipeDown = Tap = false;
        if (Platform.IsPaused) { isTracking = false; return; }

        // 6% of the short screen side, so a swipe feels the same on every resolution
        deadZone = Mathf.Max(30f, Mathf.Min(Screen.width, Screen.height) * 0.06f);

        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);

            switch (t.phase)
            {
                case TouchPhase.Began:
                    Begin(t.position);
                    break;
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    Track(t.position);
                    break;
                case TouchPhase.Ended:
                    End(t.position, true);
                    break;
                case TouchPhase.Canceled:
                    End(t.position, false);
                    break;
            }
        }
#if UNITY_EDITOR
        // Mouse stands in for a finger inside the editor
        else if (Input.GetMouseButtonDown(0))
            Begin(Input.mousePosition);
        else if (Input.GetMouseButton(0))
            Track(Input.mousePosition);
        else if (Input.GetMouseButtonUp(0))
            End(Input.mousePosition, true);
#endif
    }

    private void Begin(Vector2 pos)
    {
        startTouch = pos;
        startTime = Time.unscaledTime;
        isTracking = true;
        swipeFired = false;
    }

    // A swipe fires as soon as the finger has travelled far enough (no need to lift it), once per touch.
    private void Track(Vector2 pos)
    {
        if (!isTracking || swipeFired)
            return;

        Vector2 delta = pos - startTouch;
        if (delta.magnitude <= deadZone)
            return;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            SwipeLeft = delta.x < 0;
            SwipeRight = delta.x > 0;
        }
        else
        {
            SwipeUp = delta.y > 0;
            SwipeDown = delta.y < 0;
        }

        swipeFired = true;
    }

    // A tap is a short touch that never became a swipe, so swiping no longer fires the tank / plane weapon.
    private void End(Vector2 pos, bool allowTap)
    {
        if (!isTracking)
            return;

        Track(pos);

        if (allowTap && !swipeFired && Time.unscaledTime - startTime <= maxTapDuration)
            Tap = true;

        isTracking = false;
    }
}
