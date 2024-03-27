using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEngine.UI;

public enum STATUSUIORDER //스테이터스 배열에 담겨있는 순서
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

    public void SetUIName() //StatusUI창의 이름 정보 플레이어한테서 가져와서 표기
    {
        m_PlayerStatus[(int)STATUSUIORDER.NAME].text = Player.instance.GetPlayerStatus.m_strName;
    }

    public void SetUILEVEL() //StatusUI창의 레벨 정보 플레이어한테서 가져와서 표기
    {
        m_PlayerStatus[(int)STATUSUIORDER.LEVEL].text = Player.instance.GetPlayerStatus.m_iLevel.ToString();
    }

    public void SetUIEXP() //StatusUI창의 경험치 정보 플레이어한테서 가져와서 표기
    {
        StringBuilder NewText = new StringBuilder();
        NewText.Append(Player.instance.GetPlayerStatus.m_iCurrentEXP.ToString());
        NewText.Append(" / ");
        NewText.Append(Player.instance.GetPlayerStatus.m_iMaxEXP.ToString());

        m_PlayerStatus[(int)STATUSUIORDER.EXPANDMAXEXP].text = NewText.ToString();  
    }

    public void SetUIStatus() //StatusUI창의 이름,레벨,경험치 외의 표기변경
    {
        StringBuilder NewText = new StringBuilder();

        //체력/최대체력 정보 가져오기
        NewText.Append(Player.instance.GetPlayerStatus.m_iCurrentHP.ToString());
        NewText.Append(" / ");
        NewText.Append(Player.instance.GetPlayerStatus.GetTotalStatus.MaxHP.ToString());

        m_PlayerStatus[(int)STATUSUIORDER.HPANDMAXHP].text = NewText.ToString();

        NewText.Clear();

        //마나/최대마나 정보 가져오기
        NewText.Append(Player.instance.GetPlayerStatus.m_iCurrentMP.ToString());
        NewText.Append(" / ");
        NewText.Append(Player.instance.GetPlayerStatus.GetTotalStatus.MaxMP.ToString());

        m_PlayerStatus[(int)STATUSUIORDER.MPANDMAXMP].text = NewText.ToString();

        //공격력수치정보 가져오기
        m_PlayerStatus[(int)STATUSUIORDER.ATK].text = Player.instance.GetPlayerStatus.GetTotalStatus.Atk.ToString();

        //방어력수치정보 가져오기
        m_PlayerStatus[(int)STATUSUIORDER.DEF].text = Player.instance.GetPlayerStatus.GetTotalStatus.Def.ToString();

        //치명타확률수치정보 가져오기
        m_PlayerStatus[(int)STATUSUIORDER.CRITICAL].text = Player.instance.GetPlayerStatus.GetTotalStatus.Critical.ToString();

        //힘스탯정보 가져오기
        m_PlayerStatus[(int)STATUSUIORDER.STR].text = Player.instance.GetPlayerStatus.GetTotalStatus.Str.ToString();

        //민첩스탯정보 가져오기
        m_PlayerStatus[(int)STATUSUIORDER.DEX].text = Player.instance.GetPlayerStatus.GetTotalStatus.Dex.ToString();

        //지능스탯정보 가져오기
        m_PlayerStatus[(int)STATUSUIORDER.INT].text = Player.instance.GetPlayerStatus.GetTotalStatus.Int.ToString();

        //건강스탯정보 가져오기
        m_PlayerStatus[(int)STATUSUIORDER.HEALTH].text = Player.instance.GetPlayerStatus.GetTotalStatus.Health.ToString();

        //스탯포인트 정보 가져오기
        m_PlayerStatus[(int)STATUSUIORDER.STATPOINT].text = Player.instance.GetPlayerStatus.m_iStatPoint.ToString();

        //스피드수치 정보 가져오기
        m_PlayerStatus[(int)STATUSUIORDER.SPEED].text = Player.instance.GetPlayerStatus.m_fPlayerSpeed.ToString();
    }

    public void SetAllData() //위에는 따로따로 갱신하려만들었으나 일괄적으로 한번에 실행할떄 사용
    {
        SetUIName();
        SetUILEVEL();
        SetUIEXP();
        SetUIStatus();
    }

}
