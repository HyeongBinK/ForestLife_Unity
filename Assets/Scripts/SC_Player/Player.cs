using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class Player : MonoBehaviour
{
    private static Player m_instance; // 싱글톤이 할당될 변수

    // 싱글톤 접근용 프로퍼티
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

   [SerializeField] private PlayerStatus PlayerStatusData; //플레이어의 스테이터스 데이터
    public PlayerStatus GetPlayerStatus { get { return PlayerStatusData; } }
    private bool m_bInvincibility = false; //true : 무적상태, false : 피격가능
   // public event Action OnDamage;
   // public event Action OnDeath;
    private bool m_bIsPortalKey = false; // 포탈타는키를 누르면 true, 평상시 false
    private bool m_bIsNpcKey = false; // NPC상호작용키를 누르면 true, 평상시 false

    private Inventory m_inventory = new Inventory(); //플레이어의 인벤토리
    [SerializeField] private PlayerEquipment m_PlayerEquipment; //플레이어의 장비창
    public bool m_IsStoreUIActive { get; private set; } //상점창이 활성화되어있을경우 true 평상시엔 false
    private void Awake()
    {
        if (instance != this)
        {
            Destroy(gameObject);
        }

        m_inventory.DefaultSetting();
        m_IsStoreUIActive = false;
    }

    public void GetDamage(int DamageValue) //피격시 데미지입는함수
    {
        if (m_bInvincibility == false) //무적상태가 아니면
        {
            DamageValue += (int)(UnityEngine.Random.Range(-DamageValue * 0.1f, DamageValue * 0.1f)); //+-10% 랜덤성
            DamageValue -= (int)PlayerStatusData.GetTotalStatus.Def; //데미지 - 방어력
            DamageValue = Mathf.Clamp(DamageValue, 0, GameManager.instance.MaxDammage); //최고데미지가 넘으면 최고데미지로 고정 
            PlayerStatusData.HPDecrease(DamageValue); //체력감소이벤트
            SoundManager.Instance.PlayPlayerHitSound(); //피격음재생
            StartCoroutine(ActiveInvincibility()); //무적상태재생
            UIManager.Instance.SetDamageTextToPlayerAndPlay(DamageValue); //플레이어 머리위에 데미지텍스트 활성화
        }
    }

    public bool IsSkillMPCost(int MPCost) //스킬을 사용할 여유마나가 있는가
    {
        if(GetPlayerStatus.m_iCurrentMP >= MPCost)
        {
            GetPlayerStatus.MPDecrease(MPCost);
            return true;
        }

        GameManager.instance.AddNewLog("마나가부족합니다!");
        return false;
    }

    public bool UseSkill(string SkillName) //스킬사용
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

    public void PushPortalKey() //포탈타기버튼을 눌럿을때
    {
        if (!m_bIsPortalKey)
        {
            m_bIsPortalKey = true;
            StartCoroutine(ResetPortalKey());
        }
    }

    public void PushNpcKey() //Npc상호작용버튼 눌렀을떄
    {
        if (!m_bIsNpcKey)
        {
            m_bIsNpcKey = true;
            StartCoroutine(ResetNpcKey());
        }
    }

    IEnumerator ActiveInvincibility() //피격시 무적상태 잠시 활성화
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

    public void OnPortal(string TargetScene) //포탈타기
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

    public void PickupGold(int Gold) //골드줍기 
    {
        m_inventory.GetGold(Gold);
    }
    
    public void PickupItem(int ItemUniqueNumber, int Quantity) //아이템의 획득(줍기, 상점 구매 등에 사용)
    {
        if (m_inventory.AddItem(ItemUniqueNumber, Quantity))
            return;

        GameManager.instance.AddNewLog("아이템습득에 실패했습니다");
    }

    public bool BuyItem(int ItemUniqueNumber, int Quantity) //아이템구입(구입한 아이템의 고유번호, 갯수)
    {
        int TotalPrice = (DataTableManager.instance.GetItemData(ItemUniqueNumber).Price) * Quantity;
        if (m_inventory.UseGold(TotalPrice))
        {
            if (m_inventory.AddItem(ItemUniqueNumber, Quantity)) //아이템의 획득에 성공했을 경우
            return true;

            //인벤토리가 가득차거나 다른원인에 의해 아이템 획득에 실패했을경우 
            m_inventory.GetGold(TotalPrice); //금액 환불
            return false;
        }
        return false;
    }

    public void SellItem(int SlotNumber, int Quantity)
    {
        m_inventory.SellItem(SlotNumber, Quantity);
    }

    public bool ReturnEquipment(SlotEquipmentData Equipment) //장비창에서 인벤토리로 아이템을 되돌리는 함수(기능)
    {
        if (m_inventory.ReturnEquipmentItem(Equipment))
        {
            m_PlayerEquipment.UpdataPlayerWeaponObject();
            return true;
        }
        GameManager.instance.AddNewLog("인벤토리가 가득차 아이템을 되돌리지 못했습니다.");
        return false;
    }

    public void EquipItem(int SlotNumber, string Tooltip) //아이템장착
    {
      if(m_PlayerEquipment.EquipItem(GetSlotEquipmentData(SlotNumber), Tooltip))
        {
            SetPlayerEquipmentStatus();
            if(m_inventory.UseItem(SlotNumber))
            return;
        }
        GameManager.instance.AddNewLog("인벤토리에서 아이템의 사용/장착처리를 실패했습니다 Error");
    }

    public void SetPlayerEquipmentStatus() //플레이어스테이터스 쪽의 장비들능력치합계 변경
    {
        m_PlayerEquipment.UpdateTotalEquipmentStatus();
        PlayerStatusData.ChangeEquipmentData(m_PlayerEquipment.GetTotalEquipmentStatus);
        UIManager.Instance.RefreshStatusUIInformation();
    }

    IEnumerator ResetPortalKey() //포탈을 타는키를 눌럿을떄 일정시간 경과후 불린변수 변경
    {
        yield return new WaitForSeconds(0.1f);
        m_bIsPortalKey = false;
    }

    IEnumerator ResetNpcKey() //Npc상호작용키를 눌럿을떄 일정시간 경과후 불린변수 변경
    {
        yield return new WaitForSeconds(0.1f);
        m_bIsNpcKey = false;
    }

    public void UseConsumptionItem(int SlotNumber, QuickSlot quickSlot) //포션아이템사용
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


    //플레이어내의 인벤토리 스킬트리 퀵슬롯등의 정보에 접근하기 위한 함수
    public bool GetSlotIsData(int SlotNumber) //슬롯에데이터가 있는지에 대한 데이터를 인벤토리에서 가져오기
    {
        return m_inventory.ExportIsSlotData(SlotNumber);
    }

    public SlotItemData GetSlotItemData(int SlotNumber) //슬롯내 아이템데이터를 인벤토리에서 가져오기
    {
        return m_inventory.ExportSlotData(SlotNumber);
    }

    public SlotEquipmentData GetSlotEquipmentData(int SlotNumber) //장비아이템데이터를 인벤토리에서 가져오기
    {
        return m_inventory.ExportEquipmentData(SlotNumber);
    }

    public int CurActivatedSlotNumber() //현재활성화된 슬롯의숫자
    {
        return m_inventory.GetInventorySlotNumber;
    }

    public int GetCurInventoryGold() //현재 인벤토리내 골드량
    {
        return m_inventory.Gold;
    }

    public void ChangeInventoryData(int slot1, int slot2) //인벤토리의 데이터변경
    {
        m_inventory.SwitchSlotdata(slot1, slot2);
    }

    public void ThrowAwayInventoryItem(int SlotNumber, int Number) //인벤토리내 아이템버리기
    {
        if (DataTableManager.instance.GetItemData(m_inventory.ExportSlotData(SlotNumber).ItemNumber).ItemType == ITEM_TYPE.EQUIPMENT)
            Number = 1;
        else
        {
            //버릴갯수 지정하는 기능만들어서 넣기
        }

        m_inventory.ThrowAwayItem(SlotNumber, Number);
    }

    public void SetIsStoreUi(bool IsActive) //상점UI활성화시 호출해서 불린변수 true로 변경 반대시엔 false로
    {
        m_IsStoreUIActive = IsActive;
    }

    public void UpgradeInventorySlot() //인벤토리슬롯갯수늘리기
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
