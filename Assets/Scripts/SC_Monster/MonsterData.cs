using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MonsterData 
{
    public string name; //�����̸�
    public int MaxHP; //�ִ�ü��
    public int Atk; //���ݷ�
    public int Def; //����
    public int Gold; //óġ�� ����ϴ� �ִ��差 (50~100���� �������)
    public int Exp; //óġ�� ������ ����ġ��(����)
    public float Speed; //�̵��ӵ�
    public float AttackTerm; //�����ֱ�(���ݾִϸ��̼� ����� �״��� ���ݸ�Ǳ����� ��� �ð�)
    public float DieAnimTime; //����ִϸ��̼� �ѽð�
    public float AttackAnimTime; //���ݾִϸ��̼� �ѽð�
    public int DropItemName; //����� �������� ������ȣ
    public int DropRareItemName; //����� ����������� ������ȣ
    public int RareDropRate; //��;����� ���Ȯ�� 
    public int NormalDropRate; //�Ϲݾ����� ���Ȯ��
}
