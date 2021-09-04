using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class DebugConsole : DebugCom
{
    public RectTransform UI_Panel_Trans;

    public ScrollRect UI_TextScrollRect;
    public RectTransform UI_TextContainer;
    public TMP_InputField UI_InputField;

    public void Start()
    {
        _Close(false);
        UI_line_objects = new GameObject[Max_Lines];
    }

    // Opening / closing:
    public bool is_open = false;
    [NonSerialized] public float Openness_Speed = 5f;
    public bool is_compact = true;

    float UI_Canvas_Height { get { return DebugSystem.UI_Canvas.rect.height; } } // TODO: Performance!
    public float Compact_Height = 290f;
    public float GetTargetHeight(bool compact_target)
    {
        if (compact_target) return Compact_Height;
        else return UI_Canvas_Height;
    }

    public void Open(bool compact, bool anim = true) => Instance?._Open(compact, anim);
    public void Close(bool anim = true) => Instance?._Close(anim);

    public void _Open(bool? compact = null, bool anim = true)
    {
        if (compact.HasValue) is_compact = compact.Value;
        is_open = true;
        FocusInputField();

        Animate_Openness(is_open, anim);
    }
    public void _Close(bool anim = true)
    {
        is_open = false;
        UnfocusInputField();

        Animate_Openness(is_open, anim);
    }

    bool openness_animating;
    float openness_t;

    float openness_start_y;
    float openness_target_y;

    float openness_start_height;
    float openness_target_height;

    public void Animate_Openness(bool to_open, bool anim = true)
    {
        openness_t = (anim) ? 0f : 1f;

        openness_start_y = UI_Panel_Trans.anchoredPosition.y;
        openness_target_y = (to_open) ? 0f : GetTargetHeight(is_compact);

        openness_target_height = GetTargetHeight(is_compact);
        openness_start_height = UI_Panel_Trans.sizeDelta.y;

        ScrollConsole(false);
        openness_animating = true;
    }
    void UPDATE_Openness()
    {
        if (!openness_animating) return;

        float y = Mathf.Lerp(openness_start_y, openness_target_y, openness_t);
        float height = Mathf.Lerp(openness_start_height, openness_target_height, openness_t);

        UI_Panel_Trans.anchoredPosition = new Vector2(UI_Panel_Trans.anchoredPosition.x, y);
        UI_Panel_Trans.sizeDelta = new Vector2(UI_Panel_Trans.sizeDelta.x, height);

        ScrollConsole();

        if (openness_t <= 1.0f) openness_t += Openness_Speed * Time.unscaledDeltaTime;
        else openness_animating = false;
    }

    public void ChangeSize(bool compact, float? compact_height = null)
    {
        is_compact = compact;
        if (compact_height.HasValue) Compact_Height = compact_height.Value;
        Animate_Openness(is_open);
    }

    // Scrolling:
    public const float SCROLL_BOTTOM = 0f;
    public const float SCROLL_TOP = 1f;

    [NonSerialized] public float Scroll_Speed = 1.0f;

    bool is_scrolling;
    float scroll_t;
    float scroll_target;
    float scroll_start;
    public void ScrollConsole(bool anim = true) => ScrollConsole(SCROLL_BOTTOM, anim);
    public void ScrollConsole(float target, bool anim = true) => StartCoroutine(_ScrollConsole(target, anim));
    IEnumerator _ScrollConsole(float target, bool anim)
    {
        yield return new WaitForEndOfFrame();

        scroll_t = (anim) ? 0f : 1f;

        scroll_start = UI_TextScrollRect.verticalNormalizedPosition;
        scroll_target = target;

        is_scrolling = true;
    }
    void UPDATE_Scroll()
    {
        if (!is_scrolling) return;

        float scroll = Mathf.Lerp(scroll_start, scroll_target, scroll_t);
        UI_TextScrollRect.verticalNormalizedPosition = scroll;

        if (scroll_t <= 1.0f) scroll_t += Scroll_Speed * Time.unscaledDeltaTime;
        else is_scrolling = false;
    }

    // Text UI: 
    public GameObject UI_Line_Prefab;
    public int Max_Lines = 1000;
    int UI_line_obj_count = -1;
    GameObject[] UI_line_objects;
    void UI_AddLine(string text, Color? color = null)
    {
        GameObject obj = Instantiate(UI_Line_Prefab, UI_TextContainer);
        TMP_Text line = obj.GetComponent<TMP_Text>();

        line.SetText(text);
        if (color.HasValue) line.color = color.Value;

        if (UI_line_obj_count >= Max_Lines) UI_line_obj_count = -1;
        UI_line_objects[++UI_line_obj_count] = obj;
    }
    void UI_ClearLines()
    {
        foreach (GameObject obj in UI_line_objects)
            Destroy(obj.gameObject);
    }

    void FocusInputField()
    {
        UI_InputField.ActivateInputField();
    }

    void UnfocusInputField()
    {
        UI_InputField.DeactivateInputField();
    }

    // Text: 
    public bool Log(string text, params object[] args)
    {
        if (Instance) return Instance._Log(text, args);
        else throw new Exception("No debug console!");
    }
    public bool _Log(string text, params object[] args)
    {
        UI_AddLine(text.Parse(args));
        return true;
    }
    public void Clear() => UI_ClearLines();
}
