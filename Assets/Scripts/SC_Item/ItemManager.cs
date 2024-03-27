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

    private const int MAXFIELDITEM = 120; //�ʵ忡 �ִ� ��ӵɼ��ִ� �������� ��(������,��� ���ε��� �ݿ�)
    [SerializeField] private GoldItem Gold;
    [SerializeField] private DroppedItem DropItem;
    [SerializeField] private Transform ItemPoolingLocation; //�̸�Ǯ���� �����۵��� �����Ǵ� ������Ʈ��ġ
    Queue<GoldItem> GoldItemPooling = new Queue<GoldItem>(); //���忡 ��ӵ� ������ Ǯ��
    Queue<DroppedItem> DropItemPooling = new Queue<DroppedItem>(); //�ʵ忡 ��ӵ� �����۵��� Ǯ��
    private int Cur_FieldDropItem; //�����ʵ忡���(Ȱ��ȭ)�� �������� ��
    private int Cur_FieldGoldItem; //�����ʵ忡���(Ȱ��ȭ)�� ���������� ��
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

    public string MakeItemToolTipText(int SlotNumber, SlotEquipmentData EquipmentData) //�κ��丮�� �����۵����͸� �������� ������ �������
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
                    ToolTipText.AppendLine("���� �����α�ȯ�Ѵ�");
                }
                break;
            case ITEM_TYPE.ETC:
                {
                    ToolTipText.AppendLine("���������̶� ���ٸ� ȿ��������.");
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

    public string MakeItemToolTipByUniqueNumber(int ItemUniqueNumber) //�����۰�����ȣ�� �������� �������� �Ǹ��ϴ� �������� ������������
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
                    ToolTipText.AppendLine("���� �����α�ȯ�Ѵ�");
                }
                break;
            case ITEM_TYPE.ETC:
                {
                    ToolTipText.AppendLine("���������̶� ���ٸ� ȿ��������.");
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

    private void MakeDropItem() //���������Ǯ���� �ϳ� �߰�
    {
        DroppedItem NewDropItem = Instantiate(DropItem, ItemPoolingLocation);
        DropItemPooling.Enqueue(NewDropItem);
        NewDropItem.gameObject.SetActive(false);
    }

    private void MakeDropGold() //��������Ǯ���� �ϳ��߰�
    {
        GoldItem NewDropGold = Instantiate(Gold, ItemPoolingLocation);
        GoldItemPooling.Enqueue(NewDropGold);
        NewDropGold.gameObject.SetActive(false);

    }

    private void MakeDropItemPooling() //�ִ�����ɼ��ִ°�����ŭ �̸�Ǯ��
    {
        for (int i = 0; i < MAXFIELDITEM; i++)
        {
            MakeDropItem();
        }
    }

    private void MakeDropGoldPooling() //�ִ�����ɼ��ִ°�����ŭ �̸�Ǯ��
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

    public void CreateGoldItem(int NewGold, Vector3 Position) //����Ʈ������Ǿ�� ��ġ�� �޾ƿͼ� �ʵ忡 ����(�÷��̾ �ʵ忡 ��带 �����±�� ������ ��뿹��)
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
  
    public void CreateRandomGoldItem(int NewGold, Vector3 Position) //����ġ��(���������� 80���ο��� 120���α��� ������ġ�� ��ȯ) �����Ͽ� �ʵ忡 ���(���͵�Ӱ��)
    {
        NewGold = Random.Range((int)(NewGold * 0.8), (int)(NewGold * 1.2));
        CreateGoldItem(NewGold, Position);
    }

    public void CreateDropItem(int ItemUniqueNumber, int Quantity, Vector3 Position) //�������� ������ȣ�� ����, ����Ʈ����ġ�� �޾ƿͼ� ����(�̶� ���������ǰ�� Quantity�� 1���� ���� ���� error!)
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

    public void CreateDropItemWithDropRate(int ItemUniqueNumber, int Quantity, int DropRate, Vector3 Position) //����óġ�� �����۵�ӷ����� �޾ƿͼ� Ȯ����� �����۵��
    {
        int ItemDropResult = UnityEngine.Random.Range(0, 100); // 0~100 ���� ������ġ
       
        if (ItemDropResult < DropRate) //�������� ������ ��ġ�� ���Ȯ������ ������ ���
        {
            CreateDropItem(ItemUniqueNumber, Quantity, Position); //
        }
    }
}
