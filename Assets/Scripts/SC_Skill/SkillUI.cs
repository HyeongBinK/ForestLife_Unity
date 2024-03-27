using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SkillUI : MonoBehaviour
{
    [SerializeField] private List<SkillSlot> SkillSlots; //½ºÅ³½½·Ôµé
    [SerializeField] private Text m_SkillPointText;

    private void Awake()
    {
        SetSkillPointText();
    }

    public void SetSkillPointText()
    {
        if (Player.instance)
                m_SkillPointText.text = Player.instance.GetPlayerStatus.m_iSkillPoint.ToString();
    }

}
