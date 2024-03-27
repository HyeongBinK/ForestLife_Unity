using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Text;

public class ScreenButtonTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] string Keycode;
    private string MakeToolTipText()
    {
        StringBuilder ToolTipText = new StringBuilder();
        ToolTipText.Append(gameObject.name);
        ToolTipText.Append("(Key:");
        ToolTipText.Append(Keycode);
        ToolTipText.Append(")");

        return ToolTipText.ToString();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UIManager.Instance.SetAndActiveMiniToolTipBox(MakeToolTipText(), gameObject.transform.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.Instance.DisActiveMiniToolTip();
    }
}
