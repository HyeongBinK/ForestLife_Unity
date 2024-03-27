using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using UnityEngine.SceneManagement;
// using System;
using System.IO;
using System.Text;
using UnityEngine.SceneManagement;


[System.Serializable]
public class SaveMapNameAndPlayerTransformDataData
{
    public string LastMapName;
    public Vector3 PlayerPosition;
    public Quaternion PlayerRotation;

    public void SetData(string MapName, Transform Tr)
    {
        LastMapName = MapName;
        PlayerPosition = Tr.position;
        PlayerRotation = Tr.rotation;
    }
}

public class DataSaveAndLoad 
{
    private static DataSaveAndLoad m_instance; // �̱����� �Ҵ�� ����
    StringBuilder SavePath = new StringBuilder();
    // �̱��� ���ٿ� ������Ƽ
    public static DataSaveAndLoad Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = new DataSaveAndLoad();
                //DontDestroyOnLoad(m_instance.gameObject);
            }
            return m_instance;
        }
    }
    public void SaveGame()
    {
        GameManager.instance.AddNewLog("����Ϸ�!");
        Player.instance.SavePlayerData(); //�÷��̾��� �������ͽ�, �κ��丮, ���â����������
        UIManager.Instance.GetQuickSlotsData.SaveQuickSlotsData(); //�����Ե���������

        //�÷��̾��� ��������ġ����
        SaveMapNameAndPlayerTransformDataData SaveData = new SaveMapNameAndPlayerTransformDataData();
        Scene CurScene = SceneManager.GetActiveScene();
        SaveData.SetData(CurScene.name, GameManager.instance.playerobject.transform);
        string json = JsonUtility.ToJson(SaveData);
        File.WriteAllText(DataSaveAndLoad.Instance.MakeFilePath("PlayerLocationData", "/SaveData/LastMapAndTransformData/"), json);
        SoundManager.Instance.SaveAndLoadSound(); //���̺�ȿ����
    }

    public bool LoadGame()
    {
        GameManager.instance.AddNewLog("�ҷ�����!");
        string FilePath = MakeFilePath("PlayerLocationData", "/SaveData/LastMapAndTransformData/");
        if (!File.Exists(FilePath))
        {
            Debug.LogError("���̺������� ã�����߽��ϴ�.");
            return false;
        }
        string SaveFile = File.ReadAllText(FilePath);
        SaveMapNameAndPlayerTransformDataData SaveData = JsonUtility.FromJson<SaveMapNameAndPlayerTransformDataData>(SaveFile);

        GameManager.instance.MoveScene(SaveData.LastMapName);
        GameManager.instance.SetPlayerNewPosition(SaveData.PlayerPosition);
        if (!Player.instance.LoadPlayerData()) return false; //�÷��̾��� �������ͽ�, �κ��丮, ���â�����ͺҷ�����
        if (!UIManager.Instance.GetQuickSlotsData.LoadQuickSlotData()) return false; //�����Ե����ͺҷ�����
        SoundManager.Instance.SaveAndLoadSound();
        return true;
    }

    public string MakeFilePath(string FileName, string FileFolder)
    {
        SavePath.Append(Application.streamingAssetsPath);
        SavePath.Append(FileFolder);
        SavePath.Append(FileName);
        SavePath.Append(".Json");

        string Result = SavePath.ToString();
        SavePath.Clear();
        return Result;
    }
}
