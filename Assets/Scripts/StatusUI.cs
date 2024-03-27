using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEngine.UI;

public enum STATUSUIORDER //�������ͽ� �迭�� ����ִ� ����
{
    START,
    NAME =0,
    LEVEL,
    EXPANDMAXEXP,
    HPANDMAXHP,
    MPANDMAXMP,
    ATK,
    DEF,
    CRITICAL,
    STR,
    DEX,
    INT,
    HEALTH,
    STATPOINT,
    SPEED,
    END
}

public class StatusUI : MonoBehaviour
{
    [SerializeField] Text[] m_PlayerStatus;

    private void Awake()
    {
        if (gameObject.activeSelf)
            gameObject.SetActive(false);
    }

    public void SetUIName() //StatusUIâ�� �̸� ���� �÷��̾����׼� �����ͼ� ǥ��
    {
        m_PlayerStatus[(int)STATUSUIORDER.NAME].text = Player.instance.GetPlayerStatus.m_strName;
    }

    public void SetUILEVEL() //StatusUIâ�� ���� ���� �÷��̾����׼� �����ͼ� ǥ��
    {
        m_PlayerStatus[(int)STATUSUIORDER.LEVEL].text = Player.instance.GetPlayerStatus.m_iLevel.ToString();
    }

    public void SetUIEXP() //StatusUIâ�� ����ġ ���� �÷��̾����׼� �����ͼ� ǥ��
    {
        StringBuilder NewText = new StringBuilder();
        NewText.Append(Player.instance.GetPlayerStatus.m_iCurrentEXP.ToString());
        NewText.Append(" / ");
        NewText.Append(Player.instance.GetPlayerStatus.m_iMaxEXP.ToString());

        m_PlayerStatus[(int)STATUSUIORDER.EXPANDMAXEXP].text = NewText.ToString();  
    }

    public void SetUIStatus() //StatusUIâ�� �̸�,����,����ġ ���� ǥ�⺯��
    {
        StringBuilder NewText = new StringBuilder();

        //ü��/�ִ�ü�� ���� ��������
        NewText.Append(Player.instance.GetPlayerStatus.m_iCurrentHP.ToString());
        NewText.Append(" / ");
        NewText.Append(Player.instance.GetPlayerStatus.GetTotalStatus.MaxHP.ToString());

        m_PlayerStatus[(int)STATUSUIORDER.HPANDMAXHP].text = NewText.ToString();

        NewText.Clear();

        //����/�ִ븶�� ���� ��������
        NewText.Append(Player.instance.GetPlayerStatus.m_iCurrentMP.ToString());
        NewText.Append(" / ");
        NewText.Append(Player.instance.GetPlayerStatus.GetTotalStatus.MaxMP.ToString());

        m_PlayerStatus[(int)STATUSUIORDER.MPANDMAXMP].text = NewText.ToString();

        //���ݷ¼�ġ���� ��������
        m_PlayerStatus[(int)STATUSUIORDER.ATK].text = Player.instance.GetPlayerStatus.GetTotalStatus.Atk.ToString();

        //���¼�ġ���� ��������
        m_PlayerStatus[(int)STATUSUIORDER.DEF].text = Player.instance.GetPlayerStatus.GetTotalStatus.Def.ToString();

        //ġ��ŸȮ����ġ���� ��������
        m_PlayerStatus[(int)STATUSUIORDER.CRITICAL].text = Player.instance.GetPlayerStatus.GetTotalStatus.Critical.ToString();

        //���������� ��������
        m_PlayerStatus[(int)STATUSUIORDER.STR].text = Player.instance.GetPlayerStatus.GetTotalStatus.Str.ToString();

        //��ø�������� ��������
        m_PlayerStatus[(int)STATUSUIORDER.DEX].text = Player.instance.GetPlayerStatus.GetTotalStatus.Dex.ToString();

        //���ɽ������� ��������
        m_PlayerStatus[(int)STATUSUIORDER.INT].text = Player.instance.GetPlayerStatus.GetTotalStatus.Int.ToString();

        //�ǰ��������� ��������
        m_PlayerStatus[(int)STATUSUIORDER.HEALTH].text = Player.instance.GetPlayerStatus.GetTotalStatus.Health.ToString();

        //��������Ʈ ���� ��������
        m_PlayerStatus[(int)STATUSUIORDER.STATPOINT].text = Player.instance.GetPlayerStatus.m_iStatPoint.ToString();

        //���ǵ��ġ ���� ��������
        m_PlayerStatus[(int)STATUSUIORDER.SPEED].text = Player.instance.GetPlayerStatus.m_fPlayerSpeed.ToString();
    }

    public void SetAllData() //������ ���ε��� �����Ϸ���������� �ϰ������� �ѹ��� �����ҋ� ���
    {
        SetUIName();
        SetUILEVEL();
        SetUIEXP();
        SetUIStatus();
    }

}
