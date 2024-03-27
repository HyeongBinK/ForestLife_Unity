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
    EFFECTTYPE_DAMAGE =0, //0�ܿ� ���� �нú�
    EFFECTTYPE_ATTACKUP,
    EFFECTTYPE_SHIELDUP,
    EFFECTTYPE_CRITICALUP,
    EFFECTTYPE_SPEEDUP
}

public enum MATCHNUMBERANDNAME //��ų������ȣ�� ��ų�̸��� ����
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
    public string SkillName; //��ų�̸�
    public SKILLTYPE SkillType; //��ų����
    public int EquiredLevel; //��ų���氡�ɷ���
    public int SkillMaxLevel; //��ų�� �ִ� ����ġ
    public int MPCost; //��ųMP�Ҹ�
    public float DammagePercent; //��ų�� ����������
    public float IncreasePerLevel; //����� ����������
    public int SkillNumber; //��ų�����ѹ�
    public EFFECTTYPE EffectType; //��ų�� ����
    public string SkillDiscription; //��ų�Ǽ���
    public string SkillImageFileName; //��ų�� �̹����̸�(���������̺��� �����ͼ� �̿��ϱ�����)

    public float GetValue(int level)
    {
        return DammagePercent + (IncreasePerLevel * level);
    }

}
