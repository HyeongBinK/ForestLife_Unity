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

    [SerializeField] private AudioSource BGMPlayer; // 오디오 소스 컴포넌트(브금)
    [SerializeField] private AudioSource EffectPlayer; // 오디오 소스 컴포넌트(효과음)

    [SerializeField] private AudioClip TownSound; //마을,집,상점,장비점 브금
    [SerializeField] private AudioClip ShopAndHomeSound; //집,상점,장비점 브금
    [SerializeField] private AudioClip FieldSound; //일반필드브금
    [SerializeField] private AudioClip BossRoomSound1; //보스룸준비(배틀전)브금
    [SerializeField] private AudioClip BossRoomSound2; //보스룸1~2페이즈브금
    [SerializeField] private AudioClip BossRoomSound3; //보스룸3페이즈브금
    [SerializeField] private AudioClip BossClearBGM; //보스클리어브금

    [SerializeField] private AudioClip PickUpSound; //아이템, 골드 획득효과음
    [SerializeField] private AudioClip m_SaveAndLoadSound; //데이터로드, 세이브시 효과음
    [SerializeField] private AudioClip m_PlayerHitSound; //Player 피격 효과음

    public void PlayBGM() //씬(맵)이름에 따라 자동으로 BGM 플레이
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

    public void BossBGMChange(int Phase) //보스룸에서만 보스페이즈에따라 BGM변경
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
