using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class Player : MonoBehaviour
{
    private static Player m_instance; // �̱����� �Ҵ�� ����

    // �̱��� ���ٿ� ������Ƽ
    public static Player instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<Player>();
            }

            return m_instance;
        }
    }

   [SerializeField] private PlayerStatus PlayerStatusData; //�÷��̾��� �������ͽ� ������
    public PlayerStatus GetPlayerStatus { get { return PlayerStatusData; } }
    private bool m_bInvincibility = false; //true : ��������, false : �ǰݰ���
   // public event Action OnDamage;
   // public event Action OnDeath;
    private bool m_bIsPortalKey = false; // ��ŻŸ��Ű�� ������ true, ���� false
    private bool m_bIsNpcKey = false; // NPC��ȣ�ۿ�Ű�� ������ true, ���� false

    private Inventory m_inventory = new Inventory(); //�÷��̾��� �κ��丮
    [SerializeField] private PlayerEquipment m_PlayerEquipment; //�÷��̾��� ���â
    public bool m_IsStoreUIActive { get; private set; } //����â�� Ȱ��ȭ�Ǿ�������� true ���ÿ� false
    private void Awake()
    {
        if (instance != this)
        {
            Destroy(gameObject);
        }

        m_inventory.DefaultSetting();
        m_IsStoreUIActive = false;
    }

    public void GetDamage(int DamageValue) //�ǰݽ� �������Դ��Լ�
    {
        if (m_bInvincibility == false) //�������°� �ƴϸ�
        {
            DamageValue += (int)(UnityEngine.Random.Range(-DamageValue * 0.1f, DamageValue * 0.1f)); //+-10% ������
            DamageValue -= (int)PlayerStatusData.GetTotalStatus.Def; //������ - ����
            DamageValue = Mathf.Clamp(DamageValue, 0, GameManager.instance.MaxDammage); //�ְ������� ������ �ְ������� ���� 
            PlayerStatusData.HPDecrease(DamageValue); //ü�°����̺�Ʈ
            SoundManager.Instance.PlayPlayerHitSound(); //�ǰ������
            StartCoroutine(ActiveInvincibility()); //�����������
            UIManager.Instance.SetDamageTextToPlayerAndPlay(DamageValue); //�÷��̾� �Ӹ����� �������ؽ�Ʈ Ȱ��ȭ
        }
    }

    public bool IsSkillMPCost(int MPCost) //��ų�� ����� ���������� �ִ°�
    {
        if(GetPlayerStatus.m_iCurrentMP >= MPCost)
        {
            GetPlayerStatus.MPDecrease(MPCost);
            return true;
        }

        GameManager.instance.AddNewLog("�����������մϴ�!");
        return false;
    }

    public bool UseSkill(string SkillName) //��ų���
    {
        if (GetPlayerStatus.GetSkillLevelByName(SkillName) > 0)
        {
            if (PlayerAct.instance.IsPlayerCanAct())
            {
                if (IsSkillMPCost(DataTableManager.instance.GetSkillMPCost(SkillName)))
                {
                    switch (SkillName)
                    {
                        case "PowerSwing":
                            PlayerAct.instance.SetActState(PLAYERACTSTATE.POWERSWING);
                            break;
                        case "Earthquake":
                            PlayerAct.instance.SetActState(PLAYERACTSTATE.EARTHQUAKE);
                            break;
                        case "Thunder":
                            PlayerAct.instance.SetActState(PLAYERACTSTATE.THUNDER);
                            break;
                        default:
                            return false;
                    }

                    return true;
                }
            }
        }
        return false;
    }

    public void PushPortalKey() //��ŻŸ���ư�� ��������
    {
        if (!m_bIsPortalKey)
        {
            m_bIsPortalKey = true;
            StartCoroutine(ResetPortalKey());
        }
    }

    public void PushNpcKey() //Npc��ȣ�ۿ��ư ��������
    {
        if (!m_bIsNpcKey)
        {
            m_bIsNpcKey = true;
            StartCoroutine(ResetNpcKey());
        }
    }

    IEnumerator ActiveInvincibility() //�ǰݽ� �������� ��� Ȱ��ȭ
    {
        m_bInvincibility = true;

        yield return new WaitForSeconds(0.1f);

        m_bInvincibility = false;
    }

    public bool OpenStore(NPCTYPE type)
    {
        if (m_bIsNpcKey)
        {
            switch (type)
            {
                case NPCTYPE.CONSUMPTIONSTORENPC:
                    {
                        UIManager.Instance.OpenConsumptionStoreUI();
                    }
                    break;
                case NPCTYPE.WEAPONSTORENPC:
                    {
                        UIManager.Instance.OpenEquipmentStoreUI();
                    }
                    break;
                default:
                    return false;
            }
            return true;
        }
        return false;
    }

    public void OnPortal(string TargetScene) //��ŻŸ��
    {
        if (m_bIsPortalKey == true)
        {
            m_bIsPortalKey = false;
            GameManager.instance.MoveScene(TargetScene);
        }
    }

    public void PlayerStartMoveScene()
    {
        m_bInvincibility = true;
        PlayerAct.instance.SetPlayerIsAct(false);
    }

    public void PlayerEndMoveScene()
    {
        m_bInvincibility = false;
        PlayerAct.instance.SetPlayerIsAct(true);
    }

    public void PickupGold(int Gold) //����ݱ� 
    {
        m_inventory.GetGold(Gold);
    }
    
    public void PickupItem(int ItemUniqueNumber, int Quantity) //�������� ȹ��(�ݱ�, ���� ���� � ���)
    {
        if (m_inventory.AddItem(ItemUniqueNumber, Quantity))
            return;

        GameManager.instance.AddNewLog("�����۽��濡 �����߽��ϴ�");
    }

    public bool BuyItem(int ItemUniqueNumber, int Quantity) //�����۱���(������ �������� ������ȣ, ����)
    {
        int TotalPrice = (DataTableManager.instance.GetItemData(ItemUniqueNumber).Price) * Quantity;
        if (m_inventory.UseGold(TotalPrice))
        {
            if (m_inventory.AddItem(ItemUniqueNumber, Quantity)) //�������� ȹ�濡 �������� ���
            return true;

            //�κ��丮�� �������ų� �ٸ����ο� ���� ������ ȹ�濡 ����������� 
            m_inventory.GetGold(TotalPrice); //�ݾ� ȯ��
            return false;
        }
        return false;
    }

    public void SellItem(int SlotNumber, int Quantity)
    {
        m_inventory.SellItem(SlotNumber, Quantity);
    }

    public bool ReturnEquipment(SlotEquipmentData Equipment) //���â���� �κ��丮�� �������� �ǵ����� �Լ�(���)
    {
        if (m_inventory.ReturnEquipmentItem(Equipment))
        {
            m_PlayerEquipment.UpdataPlayerWeaponObject();
            return true;
        }
        GameManager.instance.AddNewLog("�κ��丮�� ������ �������� �ǵ����� ���߽��ϴ�.");
        return false;
    }

    public void EquipItem(int SlotNumber, string Tooltip) //����������
    {
      if(m_PlayerEquipment.EquipItem(GetSlotEquipmentData(SlotNumber), Tooltip))
        {
            SetPlayerEquipmentStatus();
            if(m_inventory.UseItem(SlotNumber))
            return;
        }
        GameManager.instance.AddNewLog("�κ��丮���� �������� ���/����ó���� �����߽��ϴ� Error");
    }

    public void SetPlayerEquipmentStatus() //�÷��̾�����ͽ� ���� ����ɷ�ġ�հ� ����
    {
        m_PlayerEquipment.UpdateTotalEquipmentStatus();
        PlayerStatusData.ChangeEquipmentData(m_PlayerEquipment.GetTotalEquipmentStatus);
        UIManager.Instance.RefreshStatusUIInformation();
    }

    IEnumerator ResetPortalKey() //��Ż�� Ÿ��Ű�� �������� �����ð� ����� �Ҹ����� ����
    {
        yield return new WaitForSeconds(0.1f);
        m_bIsPortalKey = false;
    }

    IEnumerator ResetNpcKey() //Npc��ȣ�ۿ�Ű�� �������� �����ð� ����� �Ҹ����� ����
    {
        yield return new WaitForSeconds(0.1f);
        m_bIsNpcKey = false;
    }

    public void UseConsumptionItem(int SlotNumber, QuickSlot quickSlot) //���Ǿ����ۻ��
    {
        if (m_inventory.ExportIsSlotData(SlotNumber))
        {
            var ItemUniqueNumber = m_inventory.ExportSlotData(SlotNumber).ItemNumber;
            switch (DataTableManager.instance.GetItemData(ItemUniqueNumber).ItemType)
            {
                case ITEM_TYPE.CONSUMPTION :
                    {
                        var ConsumptionItemData = DataTableManager.instance.GetConsumptionData(ItemUniqueNumber);
                        switch (ConsumptionItemData.Type)
                        {
                            case HEALTYPE.HP:
                                GetPlayerStatus.UsePortion(ConsumptionItemData.Value * 0.01f, 0, 0);
                                break;
                            case HEALTYPE.MP:
                                GetPlayerStatus.UsePortion(0, ConsumptionItemData.Value * 0.01f, 0);
                                break;
                            case HEALTYPE.EXP:
                                GetPlayerStatus.UsePortion(0, 0, ConsumptionItemData.Value);
                                break;
                        }
                    }
                    break;
                case ITEM_TYPE.SCROLL:
                    {
                        GameManager.instance.ReturnToHome();
                    }
                    break;
            }

            m_inventory.UseItem(SlotNumber);
            if (quickSlot != null)
                quickSlot.SetSlotQuantityText(m_inventory.ExportSlotData(SlotNumber).Quantity.ToString());
        }
    }


    //�÷��̾�� �κ��丮 ��ųƮ�� �����Ե��� ������ �����ϱ� ���� �Լ�
    public bool GetSlotIsData(int SlotNumber) //���Կ������Ͱ� �ִ����� ���� �����͸� �κ��丮���� ��������
    {
        return m_inventory.ExportIsSlotData(SlotNumber);
    }

    public SlotItemData GetSlotItemData(int SlotNumber) //���Գ� �����۵����͸� �κ��丮���� ��������
    {
        return m_inventory.ExportSlotData(SlotNumber);
    }

    public SlotEquipmentData GetSlotEquipmentData(int SlotNumber) //�������۵����͸� �κ��丮���� ��������
    {
        return m_inventory.ExportEquipmentData(SlotNumber);
    }

    public int CurActivatedSlotNumber() //����Ȱ��ȭ�� �����Ǽ���
    {
        return m_inventory.GetInventorySlotNumber;
    }

    public int GetCurInventoryGold() //���� �κ��丮�� ��差
    {
        return m_inventory.Gold;
    }

    public void ChangeInventoryData(int slot1, int slot2) //�κ��丮�� �����ͺ���
    {
        m_inventory.SwitchSlotdata(slot1, slot2);
    }

    public void ThrowAwayInventoryItem(int SlotNumber, int Number) //�κ��丮�� �����۹�����
    {
        if (DataTableManager.instance.GetItemData(m_inventory.ExportSlotData(SlotNumber).ItemNumber).ItemType == ITEM_TYPE.EQUIPMENT)
            Number = 1;
        else
        {
            //�������� �����ϴ� ��ɸ��� �ֱ�
        }

        m_inventory.ThrowAwayItem(SlotNumber, Number);
    }

    public void SetIsStoreUi(bool IsActive) //����UIȰ��ȭ�� ȣ���ؼ� �Ҹ����� true�� ���� �ݴ�ÿ� false��
    {
        m_IsStoreUIActive = IsActive;
    }

    public void UpgradeInventorySlot() //�κ��丮���԰����ø���
    {
        m_inventory.UpgradeSlotNumber(1);
    }
    public void SavePlayerData()
    {
        m_PlayerEquipment.SaveData();
        m_inventory.SaveData();
        PlayerStatusData.SaveStateData();
    }

    public bool LoadPlayerData()
    {
        if (!m_PlayerEquipment.LoadData()) return false;
        SetPlayerEquipmentStatus();
        if (!m_inventory.LoadData()) return false;
        if(!PlayerStatusData.LoadStateData(PlayerStatusData.PlayerSaveDataName)) return false;

        return true;
    }


}
