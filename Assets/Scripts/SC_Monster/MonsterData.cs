using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MonsterData 
{
    public string name; //몬스터이름
    public int MaxHP; //최대체력
    public int Atk; //공격력
    public int Def; //방어력
    public int Gold; //처치시 드랍하는 최대골드량 (50~100프로 랜덤드랍)
    public int Exp; //처치시 오르는 경험치량(고정)
    public float Speed; //이동속도
    public float AttackTerm; //공격주기(공격애니메이션 재생후 그다음 공격모션까지의 대기 시간)
    public float DieAnimTime; //사망애니메이션 총시간
    public float AttackAnimTime; //공격애니메이션 총시간
    public int DropItemName; //드랍할 아이템의 고유번호
    public int DropRareItemName; //드랍할 레어아이템의 고유번호
    public int RareDropRate; //희귀아이템 드랍확률 
    public int NormalDropRate; //일반아이템 드랍확률
}
