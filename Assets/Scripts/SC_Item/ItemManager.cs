using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
public class ItemManager : MonoBehaviour
{
    private static ItemManager m_instance;
    public static ItemManager instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<ItemManager>();
                DontDestroyOnLoad(m_instance.gameObject);
            }
            return m_instance;
        }
    }

    private const int Min_Reinforce = -5;
    private const int Max_Reinforce = 5;

    private const int MAXFIELDITEM = 120; //필드에 최대 드롭될수있는 아이템의 수(아이템,골드 따로따로 반영)
    [SerializeField] private GoldItem Gold;
    [SerializeField] private DroppedItem DropItem;
    [SerializeField] private Transform ItemPoolingLocation; //미리풀링된 아이템들이 생성되는 오브젝트위치
    Queue<GoldItem> GoldItemPooling = new Queue<GoldItem>(); //핃드에 드롭될 골드들의 풀링
    Queue<DroppedItem> DropItemPooling = new Queue<DroppedItem>(); //필드에 드롭될 아이템들의 풀링
    private int Cur_FieldDropItem; //현재필드에드롭(활성화)된 아이템의 수
    private int Cur_FieldGoldItem; //현재필드에드롭(활성화)된 골드아이템의 수
    private GameObject TrashCan;


    public Status MakeRandomStatus(Status Min_Status, Status Max_Status)
    {
        Status NewStatus;
        NewStatus.MaxHP = Random.Range(Min_Status.MaxHP, Max_Status.MaxHP);
        NewStatus.MaxMP = Random.Range(Min_Status.MaxMP, Max_Status.MaxMP);
        NewStatus.Atk = Random.Range(Min_Status.Atk, Max_Status.Atk);
        NewStatus.Def = Random.Range(Min_Status.Def, Max_Status.Def);
        NewStatus.Def = Mathf.Round(NewStatus.Def);
        NewStatus.Str = Random.Range(Min_Status.Str, Max_Status.Str);
        NewStatus.Dex = Random.Range(Min_Status.Dex, Max_Status.Dex);
        NewStatus.Health = Random.Range(Min_Status.Health, Max_Status.Health);
        NewStatus.Int = Random.Range(Min_Status.Int, Max_Status.Int);
        NewStatus.Critical = Random.Range(Min_Status.Critical, Max_Status.Critical);
        NewStatus.Critical = Mathf.Round(NewStatus.Critical);

        return NewStatus;
    }

    public Status MakeRandomLow_Reinforce(Status OriginalStatus)
    {
        OriginalStatus.MaxHP += Random.Range(5 * Min_Reinforce, 5 * Max_Reinforce);
        OriginalStatus.MaxMP += Random.Range(3 * Min_Reinforce, 3 * Max_Reinforce);
        OriginalStatus.Atk += Random.Range(Min_Reinforce, Max_Reinforce);
        OriginalStatus.Def += Random.Range(Min_Reinforce, Max_Reinforce);
        OriginalStatus.Str += Random.Range(Min_Reinforce, Max_Reinforce);
        OriginalStatus.Dex += Random.Range(Min_Reinforce, Max_Reinforce);
        OriginalStatus.Health += Random.Range(Min_Reinforce, Max_Reinforce);
        OriginalStatus.Int += Random.Range(Min_Reinforce, Max_Reinforce);
        return OriginalStatus;
    }

    public string MakeItemToolTipText(int SlotNumber, SlotEquipmentData EquipmentData) //인벤토리내 아이템데이터를 바탕으로 툴팁을 만들어줌
    {
        int ItemNumber;
        if (SlotNumber == -1)
        {
            ItemNumber = EquipmentData.ItemNumber;
        }
        else
            ItemNumber = Player.instance.GetSlotItemData(SlotNumber).ItemNumber;
        
        var ItemData = DataTableManager.instance.GetItemData(ItemNumber);

        StringBuilder ToolTipText = new StringBuilder();
        ToolTipText.Append("Name : ");
        ToolTipText.AppendLine(ItemData.ItemName);
        ToolTipText.Append("Value : ");
        ToolTipText.AppendLine(ItemData.ItemType.ToString());
        ToolTipText.AppendLine("Discription : ");
        ToolTipText.AppendLine(ItemData.Text);
        ToolTipText.AppendLine("Effect : ");
        switch (ItemData.ItemType)
        {
            case ITEM_TYPE.EQUIPMENT:
                {
                    if(EquipmentData == null)
                    {
                        break;
                    }
                    var EquipmentStatus = EquipmentData.ItemStatus;
                    ToolTipText.Append("HP ");
                    ToolTipText.Append(EquipmentStatus.MaxHP.ToString());
                    ToolTipText.Append(" MP ");
                    ToolTipText.Append(EquipmentStatus.MaxMP.ToString());
                    ToolTipText.Append(" ATK ");
                    ToolTipText.Append(EquipmentStatus.Atk.ToString());
                    ToolTipText.Append(" DEF ");
                    ToolTipText.AppendLine(EquipmentStatus.Def.ToString());
                    ToolTipText.Append("STR ");
                    ToolTipText.Append(EquipmentStatus.Str.ToString());
                    ToolTipText.Append(" DEX ");
                    ToolTipText.Append(EquipmentStatus.Dex.ToString());
                    ToolTipText.Append(" INT ");
                    ToolTipText.Append(EquipmentStatus.Int.ToString());
                    ToolTipText.Append(" HEALTH ");
                    ToolTipText.AppendLine(EquipmentStatus.Health.ToString());
                }
                break;
            case ITEM_TYPE.CONSUMPTION:
                {
                    var ConsumptionData = DataTableManager.instance.GetConsumptionData(ItemNumber);
                    switch (ConsumptionData.Type)
                    {
                        case HEALTYPE.EXP:
                            {
                                ToolTipText.Append("EXP : +");
                                ToolTipText.AppendLine(ConsumptionData.Value.ToString());
                            }
                            break;
                        default:
                            {
                                ToolTipText.Append(ConsumptionData.Type.ToString());
                                ToolTipText.Append(ConsumptionData.Value);
                                ToolTipText.AppendLine("%");
                            }
                            break;
                    }
                }
                break;
            case ITEM_TYPE.SCROLL:
                {
                    ToolTipText.AppendLine("사용시 집으로귀환한다");
                }
                break;
            case ITEM_TYPE.ETC:
                {
                    ToolTipText.AppendLine("재료아이템이라 별다른 효과가없다.");
                }
                break;
        }
     /*   ToolTipText.Append("Quantity : ");
        ToolTipText.AppendLine(SlotData.Quantity.ToString());*/
        ToolTipText.Append("Price : ");
        ToolTipText.Append(ItemData.Price.ToString());
        ToolTipText.Append("Gold");

        return ToolTipText.ToString();
    }

    public string MakeItemToolTipByUniqueNumber(int ItemUniqueNumber) //아이템고유번호를 바탕으로 상점에서 판매하는 아이템의 툴팁을제작함
    {
        var ItemData = DataTableManager.instance.GetItemData(ItemUniqueNumber);

        StringBuilder ToolTipText = new StringBuilder();
        ToolTipText.Append("Name : ");
        ToolTipText.AppendLine(ItemData.ItemName);
        ToolTipText.Append("Value : ");
        ToolTipText.AppendLine(ItemData.ItemType.ToString());
        ToolTipText.AppendLine("Discription : ");
        ToolTipText.AppendLine(ItemData.Text);
        ToolTipText.AppendLine("Effect : ");
        switch (ItemData.ItemType)
        {
            case ITEM_TYPE.EQUIPMENT:
                {
                    var EquipmentMinStatus = DataTableManager.instance.GetEquipmentItemData(ItemUniqueNumber).Min_Status;
                    ToolTipText.Append("HP ");
                    ToolTipText.Append(EquipmentMinStatus.MaxHP.ToString());
                    ToolTipText.Append(" MP ");
                    ToolTipText.Append(EquipmentMinStatus.MaxMP.ToString());
                    ToolTipText.Append(" ATK ");
                    ToolTipText.Append(EquipmentMinStatus.Atk.ToString());
                    ToolTipText.Append(" DEF ");
                    ToolTipText.AppendLine(EquipmentMinStatus.Def.ToString());
                    ToolTipText.Append("STR ");
                    ToolTipText.Append(EquipmentMinStatus.Str.ToString());
                    ToolTipText.Append(" DEX ");
                    ToolTipText.Append(EquipmentMinStatus.Dex.ToString());
                    ToolTipText.Append(" INT ");
                    ToolTipText.Append(EquipmentMinStatus.Int.ToString());
                    ToolTipText.Append(" HEALTH ");
                    ToolTipText.AppendLine(EquipmentMinStatus.Health.ToString());
                }
                break;
            case ITEM_TYPE.CONSUMPTION:
                {
                    var ConsumptionData = DataTableManager.instance.GetConsumptionData(ItemUniqueNumber);
                    switch (ConsumptionData.Type)
                    {
                        case HEALTYPE.EXP:
                            {
                                ToolTipText.Append("EXP : +");
                                ToolTipText.AppendLine(ConsumptionData.Value.ToString());
                            }
                            break;
                        default:
                            {
                                ToolTipText.Append(ConsumptionData.Type.ToString());
                                ToolTipText.Append(ConsumptionData.Value);
                                ToolTipText.AppendLine("%");
                            }
                            break;
                    }
                }
                break;
            case ITEM_TYPE.SCROLL:
                {
                    ToolTipText.AppendLine("사용시 집으로귀환한다");
                }
                break;
            case ITEM_TYPE.ETC:
                {
                    ToolTipText.AppendLine("재료아이템이라 별다른 효과가없다.");
                }
                break;
        }
        /*   ToolTipText.Append("Quantity : ");
           ToolTipText.AppendLine(SlotData.Quantity.ToString());*/
        ToolTipText.Append("Price : ");
        ToolTipText.Append(ItemData.Price.ToString());
        ToolTipText.Append("Gold");

        return ToolTipText.ToString();
    }
    public void ClearField()
    {
        //GameObject TrashCan = new GameObject();
        int LoopTime = DropItemPooling.Count;
        for (int i = 0; i < LoopTime; i++)
        {
            DropItemPooling.Dequeue().gameObject.transform.parent = TrashCan.transform;
        }
        LoopTime = GoldItemPooling.Count;
        for (int i = 0; i < LoopTime; i++)
        {
            GoldItemPooling.Dequeue().gameObject.transform.parent = TrashCan.transform;
        }
        DropItemPooling.Clear();
        GoldItemPooling.Clear();
        TrashCan = null;
    }

    private void MakeDropItem() //드랍아이템풀링에 하나 추가
    {
        DroppedItem NewDropItem = Instantiate(DropItem, ItemPoolingLocation);
        DropItemPooling.Enqueue(NewDropItem);
        NewDropItem.gameObject.SetActive(false);
    }

    private void MakeDropGold() //골드아이템풀링에 하나추가
    {
        GoldItem NewDropGold = Instantiate(Gold, ItemPoolingLocation);
        GoldItemPooling.Enqueue(NewDropGold);
        NewDropGold.gameObject.SetActive(false);

    }

    private void MakeDropItemPooling() //최대생성될수있는갯수만큼 미리풀링
    {
        for (int i = 0; i < MAXFIELDITEM; i++)
        {
            MakeDropItem();
        }
    }

    private void MakeDropGoldPooling() //최대생성될수있는갯수만큼 미리풀링
    {
        for (int i = 0; i < MAXFIELDITEM; i++)
        {
            MakeDropGold();
        }
    }

    private void Awake()
    {
        if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void ReadyToMakeItem()
    {
        GoldItemPooling.Clear(); 
        DropItemPooling.Clear();
        MakeDropItemPooling();
        MakeDropGoldPooling();
        Cur_FieldDropItem = 0;
        Cur_FieldGoldItem = 0;
        TrashCan = new GameObject();
        TrashCan.name = "TrashCan";
    }

    public void CreateGoldItem(int NewGold, Vector3 Position) //떨어트릴골드의양과 위치를 받아와서 필드에 생성(플레이어가 필드에 골드를 버리는기능 구현시 사용예정)
    {
        if(MAXFIELDITEM > Cur_FieldGoldItem)
        {
            Cur_FieldGoldItem++;
            GoldItem NewGoldItem = GoldItemPooling.Dequeue();
            NewGoldItem.gameObject.transform.parent = TrashCan.transform;
            NewGoldItem.SetData(NewGold, (Position + new Vector3(0, 1, 0)));
            NewGoldItem.GoldItemDisable += () => GoldItemPooling.Enqueue(NewGoldItem);
            NewGoldItem.GoldItemDisable += () => Cur_FieldGoldItem--;
        }
    }
  
    public void CreateRandomGoldItem(int NewGold, Vector3 Position) //골드수치를(정해진양의 80프로에서 120프로까지 랜덤수치로 변환) 변경하여 필드에 드롭(몬스터드롭골드)
    {
        NewGold = Random.Range((int)(NewGold * 0.8), (int)(NewGold * 1.2));
        CreateGoldItem(NewGold, Position);
    }

    public void CreateDropItem(int ItemUniqueNumber, int Quantity, Vector3 Position) //아이템의 고유번호와 갯수, 떨어트릴위치를 받아와서 생성(이때 장비아이템의경우 Quantity에 1외의 값이 들어가면 error!)
    {
        if (MAXFIELDITEM > Cur_FieldDropItem)
        {
            Cur_FieldDropItem++;
            DroppedItem NewDropItem = DropItemPooling.Dequeue();
            NewDropItem.gameObject.transform.parent = TrashCan.transform;
            NewDropItem.ItemDataInit(ItemUniqueNumber, Quantity, (Position+ new Vector3(0,1,0)));
            NewDropItem.DropItemDisalbe += () => DropItemPooling.Enqueue(NewDropItem);
            NewDropItem.DropItemDisalbe += () => Cur_FieldDropItem--;
        }
    }

    public void CreateDropItemWithDropRate(int ItemUniqueNumber, int Quantity, int DropRate, Vector3 Position) //몬스터처치시 아이템드롭률까지 받아와서 확률대로 아이템드롭
    {
        int ItemDropResult = UnityEngine.Random.Range(0, 100); // 0~100 사이 랜덤수치
       
        if (ItemDropResult < DropRate) //랜덤으로 생성된 수치가 드랍확률보다 낮았을 경우
        {
            CreateDropItem(ItemUniqueNumber, Quantity, Position); //
        }
    }
}
