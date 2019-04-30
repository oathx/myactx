using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SLua;

[CustomLuaClass]
public class XTextStyleComponent : MonoBehaviour
{
    public string style;

    public bool applySize = true;
    public bool applyColor = true;
    public bool applyGradient = true;
    public bool applyShadow = true;
    public bool applyOutLine = true;

    // Use this for initialization
    void Start()
    {
        XTextStyleSheetObject.StyleData data = Get();
        if (data != null)
            Apply(data);
    }

    [DoNotToLua]
    public XTextStyleSheetObject.StyleData Get()
    {
        if (string.IsNullOrEmpty(style))
        {
            XTextStyleComponent textStyle = transform.parent.GetComponentInParent<XTextStyleComponent>();
            if (textStyle == null)
                return null;
            return textStyle.Get();
        }

        return XTextStyleManager.Instance.Get(style);
    }

    [DoNotToLua]
    public void Apply(XTextStyleSheetObject.StyleData data)
    {
        Text text = GetComponent<Text>();
        if (text == null) return;

        if (applySize)
            text.fontSize = data.fontSize;

        text.fontStyle = data.fontStyle;

        if (applyColor)
            text.color = data.color;


        if (applyGradient)
        {
            XGradientComponent gradient = gameObject.GetComponent<XGradientComponent>();
            if (data.gradient)
            {
                if (gradient == null)
                    gradient = AddGradientComponent();

                gradient.SetColor(data.gradientTopColor, data.gradientBottomColor);
                gradient.enabled = true;
            }
            else
            {
                if (gradient != null)
                    gradient.enabled = false;
            }
        }

        if (applyOutLine)
        {
            Outline outline = gameObject.GetComponent<Outline>();
            if (data.outline)
            {
                if (outline == null)
                    outline = gameObject.AddComponent<Outline>();

                outline.effectColor = data.outlineColor; ;
                outline.effectDistance = data.outlineDistance;
                outline.useGraphicAlpha = true;
                outline.enabled = true;
            }
            else
            {
                if (outline != null)
                    outline.enabled = false;
            }
        }

        if (applyShadow)
        {
            Shadow shadow = gameObject.GetComponent<Shadow>();
            if (data.shadow)
            {
                if (shadow == null)
                    shadow = gameObject.AddComponent<Shadow>();

                shadow.effectColor = data.shadowColor; ;
                shadow.effectDistance = data.shadowDistance;
                shadow.useGraphicAlpha = data.shadowUseGraphicAlpha;
                shadow.enabled = true;
            }
            else
            {
                if (shadow != null && typeof(Outline) != shadow.GetType())
                    shadow.enabled = false;
            }
        }
    }


    public void Apply()
    {
        XTextStyleSheetObject.StyleData data = XTextStyleManager.Instance.Get(style);
        if (data != null)
            Apply(data);
    }

    public void ChangeStyle(int sort)
    {
        XTextStyleSheetObject.StyleData data = XTextStyleManager.Instance.GetBySort(sort);
        if (data != null)
            Apply(data);
    }

    public void ApplyBySwitch(bool isOn)
    {
        if (isOn)
        {
            style = style.Replace("off", "on");
        }
        else
        {
            style = style.Replace("on", "off");
        }

        XTextStyleSheetObject.StyleData data = Get();
        if (data != null)
            Apply(data);
    }

    protected virtual XGradientComponent AddGradientComponent()
    {
        return gameObject.AddComponent<XGradientComponent>();
    }
}
