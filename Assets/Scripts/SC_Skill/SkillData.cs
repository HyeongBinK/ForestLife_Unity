using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SKILLTYPE
{
    SKILLVALUE_ACTIVE = 0,
    SKILLVALUE_PASSIVE
}

public enum EFFECTTYPE
{
    EFFECTTYPE_DAMAGE =0, //0외엔 전부 패시브
    EFFECTTYPE_ATTACKUP,
    EFFECTTYPE_SHIELDUP,
    EFFECTTYPE_CRITICALUP,
    EFFECTTYPE_SPEEDUP
}

public enum MATCHNUMBERANDNAME //스킬고유번호와 스킬이름을 연결
{
    PowerSwing = 0,
    Earthquake,
    Thunder,
    AttackIncrease,
    ShieldIncrease,
    CriticalIncrease,
    SpeedIncrease
}


public struct SkillData 
{
    public string SkillName; //스킬이름
    public SKILLTYPE SkillType; //스킬종류
    public int EquiredLevel; //스킬습득가능레밸
    public int SkillMaxLevel; //스킬의 최대 레밸치
    public int MPCost; //스킬MP소모량
    public float DammagePercent; //스킬의 데미지위력
    public float IncreasePerLevel; //레밸당 위력증가량
    public int SkillNumber; //스킬고유넘버
    public EFFECTTYPE EffectType; //스킬의 종류
    public string SkillDiscription; //스킬의설명
    public string SkillImageFileName; //스킬의 이미지이름(데이터테이블에서 가져와서 이용하기위해)

    public float GetValue(int level)
    {
        return DammagePercent + (IncreasePerLevel * level);
    }

}
