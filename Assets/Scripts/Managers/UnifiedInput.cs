using Unity.VisualScripting;
using UnityEngine;

public static class UnifiedInput
{
    public static bool MoveRight =>
        Input.GetKeyDown(KeyCode.D) ||
        Input.GetKeyDown(KeyCode.RightArrow) ||
        MobileInputController.instance.SwipeRight;

    public static bool MoveLeft =>
        Input.GetKeyDown(KeyCode.A) ||
        Input.GetKeyDown(KeyCode.LeftArrow) ||
        MobileInputController.instance.SwipeLeft;

    public static bool Jump =>
        Input.GetKeyDown(KeyCode.Space) ||
        Input.GetKeyDown(KeyCode.UpArrow) ||
        MobileInputController.instance.SwipeUp;

    public static bool Slide =>
        Input.GetKeyDown(KeyCode.S) ||
        Input.GetKeyDown(KeyCode.DownArrow) ||
        MobileInputController.instance.SwipeDown;

    public static bool Fire =>
    Input.GetKeyDown(KeyCode.Mouse0) ||
    MobileInputController.instance.Tap ||
    Input.GetKeyDown(KeyCode.Space);

}
