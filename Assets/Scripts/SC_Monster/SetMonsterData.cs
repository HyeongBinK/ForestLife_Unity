using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MONSTER_TYPE
{
    NONE = 0,
    BABYWOOD,
    ROCKSNAIL,
    PUNCHTREE,
    MINIGOLLEM,
    WOODGOLLEM
}

public class SetMonsterData 
{
    public int level { get; private set; }
    public MONSTER_TYPE MobNumber { get; private set; }
    protected MonsterData state;
    public MonsterData State { get { return state; } }

    public virtual void SetState()
    {
        switch (MobNumber)
        {
            case MONSTER_TYPE.BABYWOOD :
                {
                    state.name = "º£ÀÌºñ¿ìµå";
                    state.MaxHP = 50 + (10 * level); 
                    state.Atk = 5 + level;
                    state.Def = 0;
                    state.Gold = 10 + (5 * level);
                    state.Exp = 3 + level;
                    state.Speed = 2.0f;
                    state.AttackTerm = 3f;
                    state.DropItemName = 0;
                    state.DropRareItemName = 5;
                    state.NormalDropRate = 50;
                    state.RareDropRate = 10;
                    state.DieAnimTime = 2.16f;
                    state.AttackAnimTime = 1.67f;
                }
                break;
            case MONSTER_TYPE.ROCKSNAIL:
                {
                    state.name = "µ¹´ÞÆØÀÌ";
                    state.MaxHP = 100 + (20 * level);
                    state.Atk = 10 + level;
                    state.Def = 3 + (int)(0.2 * level);
                    state.Gold = 20 + (7 * level);
                    state.Exp = 5 + (int)(1.5*level);
                    state.Speed = 1.5f;
                    state.AttackTerm = 3f;
                    state.DropItemName = 2;
                    state.DropRareItemName = 5;
                    state.NormalDropRate = 50;
                    state.RareDropRate = 10;
                    state.DieAnimTime = 3.2f;
                    state.AttackAnimTime = 2.67f;
                }
                break;
            case MONSTER_TYPE.PUNCHTREE:
                {
                    state.name = "ÆÝÄª³ª¹«";
                    state.MaxHP = 200 + (30 * level);
                    state.Atk = 30 + (int)(1.5 * level);
                    state.Def = 3 + (int)(0.2 * level);
                    state.Gold = 50 + (10 * level);
                    state.Exp = 15 + (2 * level);
                    state.Speed = 2.5f;
                    state.AttackTerm = 2f;
                    state.DropItemName = 1;
                    state.DropRareItemName = 6;
                    state.NormalDropRate = 50;
                    state.RareDropRate = 10;
                    state.DieAnimTime = 2.2f;
                    state.AttackAnimTime = 2.17f;
                }
                break;
            case MONSTER_TYPE.MINIGOLLEM:
                {
                    state.name = "¹Ì´Ï°ñ·½";
                    state.MaxHP = 300 + (30 * level);
                    state.Atk = 30 + 2 * level;
                    state.Def = 3 + (int)(0.5 * level);
                    state.Gold = 100 + (10 * level);
                    state.Exp = 30 + (2 * level);
                    state.Speed = 3f;
                    state.AttackTerm = 2f;
                    state.DropItemName = 3;
                    state.DropRareItemName = 6;
                    state.NormalDropRate = 50;
                    state.RareDropRate = 10;
                    state.DieAnimTime = 3.9f;
                    state.AttackAnimTime = 2.67f;
                }
                break;
            case MONSTER_TYPE.WOODGOLLEM:
                {
                    state.name = "¿ìµå°ñ·½";
                    state.MaxHP = 1000 + (50 * level);
                    state.Atk = 200 + 5 * level;
                    state.Def = 50 + level;
                    state.Gold = 3000 + (50 * level);
                    state.Exp = 300 + (2 * level);
                    state.Speed = 3f;
                    state.AttackTerm = 7f;
                    state.DropItemName = 4;
                    state.DropRareItemName = 22;
                    state.NormalDropRate = 50;
                    state.RareDropRate = 10;
                    state.DieAnimTime = 6.0f;
                    state.AttackAnimTime = 0;
                }
                break;
        }
    }

    public SetMonsterData(int level, MONSTER_TYPE MobNumber)
    {
        this.level = level;
        this.MobNumber = MobNumber;

        SetState();
    }

    public void Berserk()
    {
        state.Atk = (int)(state.Atk * 1.5);
        state.Def = (int)(state.Def*1.2);
        state.Speed *= 1.5f;
    }



}