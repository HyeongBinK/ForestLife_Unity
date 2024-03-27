using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    private static SoundManager m_instance;
    
    public static SoundManager Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<SoundManager>();
               // DontDestroyOnLoad(m_instance.gameObject);
            }
            return m_instance;
        }
    }

    private void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    [SerializeField] private AudioSource BGMPlayer; // ����� �ҽ� ������Ʈ(���)
    [SerializeField] private AudioSource EffectPlayer; // ����� �ҽ� ������Ʈ(ȿ����)

    [SerializeField] private AudioClip TownSound; //����,��,����,����� ���
    [SerializeField] private AudioClip ShopAndHomeSound; //��,����,����� ���
    [SerializeField] private AudioClip FieldSound; //�Ϲ��ʵ���
    [SerializeField] private AudioClip BossRoomSound1; //�������غ�(��Ʋ��)���
    [SerializeField] private AudioClip BossRoomSound2; //������1~2��������
    [SerializeField] private AudioClip BossRoomSound3; //������3��������
    [SerializeField] private AudioClip BossClearBGM; //����Ŭ������

    [SerializeField] private AudioClip PickUpSound; //������, ��� ȹ��ȿ����
    [SerializeField] private AudioClip m_SaveAndLoadSound; //�����ͷε�, ���̺�� ȿ����
    [SerializeField] private AudioClip m_PlayerHitSound; //Player �ǰ� ȿ����

    public void PlayBGM() //��(��)�̸��� ���� �ڵ����� BGM �÷���
    {
        Scene scene = SceneManager.GetActiveScene();
        BGMPlayer.Stop();

        switch (scene.name)
        {
            case "BossField":
                BGMPlayer.clip = BossRoomSound1;
                break;
            case "Field1":
            case "Field2":
            case "Field3":
                BGMPlayer.clip = FieldSound;
                break;
            case "Town":
                BGMPlayer.clip = TownSound;
                break;
            default:
                BGMPlayer.clip = ShopAndHomeSound;
                break;
        }
        BGMPlayer.Play();
    }

    public void BossBGMChange(int Phase) //�����뿡���� ������������� BGM����
    {
        BGMPlayer.Stop();
        switch (Phase)
        {
            case 1:
            case 2:
                BGMPlayer.clip = BossRoomSound2;
                break;
            case 3:
                BGMPlayer.clip = BossRoomSound3;
                break;
            default:
                BGMPlayer.clip = BossClearBGM;
                break;
        }

        BGMPlayer.Play();
    }

    public void PlayEffectSoundForOnce(AudioClip Sound)
    {
        EffectPlayer.PlayOneShot(Sound);
    }

    public void PlayPickUpSound()
    {
        EffectPlayer.PlayOneShot(PickUpSound);
    }

    public void SaveAndLoadSound()
    {
        EffectPlayer.PlayOneShot(m_SaveAndLoadSound);
    }

    public void PlayPlayerHitSound()
    {
        EffectPlayer.PlayOneShot(m_PlayerHitSound);
    }
}
