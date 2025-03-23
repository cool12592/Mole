using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerAttack : MonoBehaviourPunCallbacks
{
    public PhotonView PV;
    private playerScript player;
    private GameObject gunObject;
    private Animator characterAnim, gunAni;
    private SpriteRenderer spriteRender;

    Timer.TimerStruct attackTimer = new Timer.TimerStruct(0.25f);

    // Start is called before the first frame update
    private void Start()
    {
        PV = GetComponent<PhotonView>();
        spriteRender = GetComponent<SpriteRenderer>();
        gunObject = transform.Find("gun").gameObject;
        player = GetComponent<playerScript>();

        gunAni = gunObject.GetComponent<Animator>();
        characterAnim = GetComponent<Animator>();
    }

    // Update is called once per frame
    private void Update()
    {
        RunTimer();
        if (Input.GetKeyDown(KeyCode.Space)) Attack();
    }

    void RunTimer()
    {
        if (PV.IsMine)
        {
            if (attackTimer.isCoolTime())
            {
                attackTimer.RunTimer();
                if(attackTimer.isCoolTime() == false)
                    gunAni.SetBool("isShot", false);
            }
        }
    }

    public void Attack()
    {
        if (player.isActive == false) return;
        if (attackTimer.isCoolTime()) return;

        attackTimer.ResetCoolTime();
        SoundManager.Instance.PlayShootingSound();
        OnAttackAnimation();
        PV.RPC("ShootRPC", RpcTarget.All, PhotonNetwork.NickName);
    }

    [PunRPC]
    private void ShootRPC(string name)
    {
        var bullet = ObjectPool.GetObject(name);

        bullet.transform.position = gunObject.transform.position + new Vector3(spriteRender.flipX ? -0.4f : 0.4f, -0.11f, 0);
        bullet.transform.rotation = gunObject.GetComponent<Transform>().rotation;
    }

    private void OnAttackAnimation()
    {
        gunAni.SetBool("isShot", true);
        characterAnim.SetTrigger("shot");
    }
}