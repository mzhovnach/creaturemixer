using UnityEngine;
using System;

public class CheatsBase : MonoBehaviour
{
    // Activate corner area size by screen width percentage
    public float ActivateAreaSize = 0.1f;

    // How many clicks the player should do before cheats list will be visible
    public int ClicksCount = 5;

    // How many seconds player have to click/touch the screen
    public float WaitTime = 2;

    private float[] _clickTimes;

    private int _clickTimesIndex;

    private bool _active = false;

    Rect windowRect = new Rect(5, 5, 500, 1000);

    void Start()
    {
        // create clicks array and reset it with float.MinValue
        _clickTimes = new float[ClicksCount];
        ResetClicks();

        //guiStyleBlackText.fontSize = 20;
        //guiStyleBlackText.fontStyle = FontStyle.Bold;
        //guiStyleBlackText.alignment = TextAnchor.MiddleLeft;
        //guiStyleBlackText.normal.textColor = Color.black;

        //guiStyleRedText.fontSize = 20;
        //guiStyleRedText.fontStyle = FontStyle.Bold;
        //guiStyleRedText.alignment = TextAnchor.MiddleLeft;
        //guiStyleRedText.normal.textColor = Color.red;

        windowRect.width = Screen.width - windowRect.x * 2;
        windowRect.height = Screen.height - windowRect.y * 2;
    }

    private void ResetClicks()
    {
        for (int i = 0; i < ClicksCount; i++)
        {
            _clickTimes[i] = float.MinValue;
        }
    }

    void Update()
    {
        // check for click or touch and register it
        if (CheckClickOrTouch())
        {
            // click will be registered at time since level load
            _clickTimes[_clickTimesIndex] = Time.timeSinceLevelLoad;
            // each next click will be written on next array index or 0 if overflow
            _clickTimesIndex = (_clickTimesIndex + 1) % ClicksCount;
        }

        // check if cheat list should be activated
        if (ShouldActivate())
        {
            _active = true;
            ResetClicks();
        }
    }

    // checks if cheat list should be activated
    private bool ShouldActivate()
    {
        // check if all click/touches were made within WaitTime
        foreach (float clickTime in _clickTimes)
        {
            if (clickTime < Time.timeSinceLevelLoad - WaitTime)
            {
                // return false if any of click/touch times has been done earlier
                return false;
            }
        }

        // if we are here, cheat should be activated
        return true;
    }

    // returns true if there's click or touch within the activate area
    private bool CheckClickOrTouch()
    {
        // convert activation area to pixels
        float sizeInPixels = ActivateAreaSize * Screen.width;

        // get the click/touch position
        Vector2? position = ClickOrTouchPoint();

        if (position.HasValue) // position.HasValue returns true if there is a click or touch
        {
            // check if withing the range
            if (position.Value.x >= Screen.width - sizeInPixels && Screen.height - position.Value.y <= sizeInPixels)
            {
                return true;
            }
        }

        return false;
    }

    // checks for click or touch and returns the screen position in pixels
    private Vector2? ClickOrTouchPoint()
    {
        if (Input.GetMouseButtonDown(0)) // left mouse click
        {
            return Input.mousePosition;
        }
        else if (Input.touchCount > 0) // one or more touch
        {
            // check only the first touch
            Touch touch = Input.touches[0];

            // it should react only when the touch has just began
            if (touch.phase == TouchPhase.Began)
            {
                return touch.position;
            }
        }

        // null if there's no click or touch
        return null;
    }

    void OnGUI()
    {
        if (_active)
        {
            windowRect = GUILayout.Window(0, windowRect, DoMyWindow, "Cheats");
        }
    }

    void DoMyWindow(int windowID)
    {
        //
        GUI.skin.button.fontSize = 30;
        GUI.skin.button.fontStyle = FontStyle.Bold;

        GUI.skin.label.fontSize = 30;
        GUI.skin.label.fontStyle = FontStyle.Bold;

        GUI.skin.horizontalSlider.fixedHeight = 30;
        GUI.skin.horizontalSliderThumb.fixedHeight = 40;
        GUI.skin.horizontalSliderThumb.fixedWidth = 40;

        //GUI.skin.toggle.fixedHeight = 40;
        //GUI.skin.toggle.fontSize = 30;
        //GUI.skin.toggle.fontStyle = FontStyle.Bold;
        //
        DisplayButtonCheat("Hide", () => _active = false);
        GUILayout.Label("TimeScale : " + Time.timeScale.ToString());
        Time.timeScale = GUILayout.HorizontalSlider(Time.timeScale, 0.0f, 5.0f);
        DisplayCheats();
    }

    protected void DisplayButtonCheat(string cheatName, Action clickedCallback)
    {
        if (GUILayout.Button(cheatName, GUI.skin.button))
        {
            clickedCallback();
        }
    }

    protected void DisplayRepeatButtonCheat(string cheatName, Action clickedCallback)
    {
        if (GUILayout.RepeatButton(cheatName))
        {
            clickedCallback();
        }
    }

    public virtual void DisplayCheats()
    {
        // display cheats list here...
    }

}