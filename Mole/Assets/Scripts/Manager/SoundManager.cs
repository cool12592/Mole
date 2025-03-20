using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance = null;
    public PhotonView PV;
    // 오디오 소스 생성해서 추가
    private AudioSource bgm;
    private AudioSource DashSound;
    private AudioSource ShootingSound;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public static SoundManager Instance
    {
        get
        {
            if (instance == null)
            {
                return null;
            }
            return instance;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();
        ShootingSound = transform.Find("shotSound").GetComponent<AudioSource>();
        DashSound = transform.Find("dashSound").GetComponent<AudioSource>();

        bgm = transform.Find("BGM").GetComponent<AudioSource>();
        bgm.Play();
    }

    public void PlayShootingSound()
    {
        if (!ShootingSound.isPlaying)
            PV.RPC("ShotSoundRPC", RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void ShotSoundRPC()
    {
        if (ShootingSound == null)
            return;
        ShootingSound.Play();
    }

    public void PlayDashSound()
    {
        PV.RPC("DashSoundRPC", RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void DashSoundRPC()
    {
        if (DashSound == null)
            return;
        DashSound.Play();
    }
}
