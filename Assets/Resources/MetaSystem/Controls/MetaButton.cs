using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using Coffee.UIExtensions;

public class MetaButton : Button
{
    public static bool Maskability = true;

    public RectTransform UI_RectTrans;
    public TMP_Text UI_Text;
    public RectTransform UI_Ripple;
    public Image UI_RippleImage;

    public ParticleSystem UI_ParticleSystem;
    public UIParticle UI_Particle;

    // Text:
    public string Text
    {
        get { return UI_Text.text; }
        set { UI_Text.SetText(value); }
    }
    public void SetText(string text) => UI_Text.SetText(text);

    // Size:
    public float Width
    {
        get { return UI_RectTrans.sizeDelta.x; }
        set { UI_RectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value); }
    }
    public float Height
    {
        get { return UI_RectTrans.sizeDelta.y; }
        set { UI_RectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value); }
    }

    // Ripple:
    public void RIPPLE_OnPressed()
    {
        if (is_ripple_active) return;

        is_ripple_active = true;
        StartCoroutine(RIPPLE_PressCoroutine());
        ripple_speed = Ripple_Speed_Slow;
    }
    public void RIPPLE_OnReleased()
    {
        if (!is_ripple_active) return;

        is_ripple_active = false;
        ripple_speed = Ripple_Speed_Click;
        StartCoroutine(RIPPLE_FadeCoroutine(0f));
    }
    
    protected override void Start()
    {
        base.Start();
        og_colors = colors;
        
        // TODO: Temporary?
        UI_Text.SetText(gameObject.name);
    }
    
    ColorBlock og_colors;
    bool is_ripple_active;
    bool is_ripple_down_anim;
    float ripple_speed;
    
    public float Ripple_Size_Add = 0.1f;
    public float Ripple_Fade_In_Sec = 0.1f;
    public float Ripple_Fade_Out_Sec = 0.3f;
    public float Ripple_Speed_Slow = 1f;
    public float Ripple_Speed_Click = 3f;
    public float Ripple_Down_Finish_Delay_Sec = 0.26f;
    
    // TODO: Should probably do something if the mouse leaves the button while being pressed.
    // TODO!: Handle a mouse potentially not being present!
    IEnumerator RIPPLE_PressCoroutine()
    {
        UI_Particle.maskable = Maskability;

        float t = 0f;
        float target = Width * (1f + Ripple_Size_Add); // + Offset for rounded/spherical corners
        Vector2 center = new Vector2(-Width / 2f, 0); // TODO!: This works only for buttons that are left-aligned / in a container! Does not work on centered (pos) button!
        is_ripple_down_anim = true;
        
        // TODO: When you release the button, it becomes normalColor. However, this looks weird with the ripple effect animation.
        // So here, we make the normalColor and highlightedColor the same as pressedColor until the ripple animation finishes.
        var new_colors = colors;
        new_colors.normalColor = new_colors.pressedColor;
        new_colors.highlightedColor = new_colors.pressedColor;
        colors = new_colors;
        
        // Fade in the ripple effect:
        UI_RippleImage.CrossFadeAlpha(1f, Ripple_Fade_In_Sec, false);
        
        UI_ParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        UI_ParticleSystem.Play();
        
        while (is_ripple_down_anim)
        {
            float value = Mathf.Lerp(0f, target, t);

            // Expand size:
            UI_Ripple.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value);
            UI_Ripple.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value);
            
            // Move ripple to mouse position: 
            var mouse_pos = Mouse.current.position.ReadValue();
            Vector2 mouse_target;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(UI_RectTrans, mouse_pos, null, out mouse_target);
            
            // Return to center towards end of animation: 
            Vector3 result = Vector3.Lerp(mouse_target, center, t);
            
            UI_Ripple.localPosition = result;
            
            //Logger.Log("mouse_pos: %  |  mouse_target: %  |  result: %", mouse_pos, mouse_target, result);
            
            if (t > 1f)
            {
                yield return new WaitForSeconds(Ripple_Down_Finish_Delay_Sec); // Delay the down animation just a bit after it finishes.
                is_ripple_down_anim = false;
                colors = og_colors;
                yield break;
            }
            t += ripple_speed * Time.deltaTime;
            yield return null;
        }
    }

    public float Ripple_Speed_Fade = 3.5f;
    IEnumerator RIPPLE_FadeCoroutine(float target)
    {
        float og_value = Width * (1f + Ripple_Size_Add);

        // Wait for down animation to finish:
        while (is_ripple_down_anim) yield return null;

        UI_RippleImage.CrossFadeAlpha(0f, Ripple_Fade_Out_Sec, false);
    }

    void Update()
    {
        if (IsPressed())
        {
            RIPPLE_OnPressed();
            return;
        }

        RIPPLE_OnReleased();
    }
}