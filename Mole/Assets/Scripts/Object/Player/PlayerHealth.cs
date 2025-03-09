using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerHealth : MonoBehaviourPunCallbacks, IPunObservable
{
    
    
    public PhotonView PV;
    [SerializeField]
    private Image healthImage;
    private Rigidbody2D rigidBody;
    private Animator characterAnim;
    private playerScript player;
    private SpriteRenderer spriteRender;

    private bool invincibility = false;
    private string recentAttacker;

    private enum ColorList { Original, DamagedColor };
    Timer.TimerStruct healthTimer = new Timer.TimerStruct(0.35f);

    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();

        rigidBody = gameObject.GetComponent<Rigidbody2D>();
        characterAnim = GetComponent<Animator>();
        spriteRender = GetComponent<SpriteRenderer>();
        player = GetComponent<playerScript>();
        
    }

    // Update is called once per frame
    void Update()
    {
        RunTimer();
        if (Input.GetKeyDown(KeyCode.T)) TakeDamage(PhotonNetwork.NickName);

    }

    void RunTimer()
    {
        if (PV.IsMine)
        {
            if(healthTimer.isCoolTime())
            {
                healthTimer.RunTimer();
                if(healthTimer.isCoolTime() == false)
                    ChangeColor(ColorList.Original);
            }
        }
    }

    public void OnInvincibility()
    {
        invincibility = true;
    }
    public void OffInvincibility()
    {
        invincibility = false;
    }

    public void HealHP(float recoverHP)
    {
        healthImage.fillAmount += recoverHP;
        if (healthImage.fillAmount > 1f)
            healthImage.fillAmount = 1f;
    }

    public void TakeDamage(string enemyName)
    {
        if (invincibility) return;

        recentAttacker = enemyName;
        ChangeColor(ColorList.DamagedColor);
        healthTimer.ResetCoolTime();
        ReducedHP(0.1f);
    }

    private void ChangeColor(ColorList color)
    {
        PV.RPC("ChangeColorRPC", RpcTarget.AllBuffered, (int)color);
    }

    [PunRPC]
    private void ChangeColorRPC(int color)
    {
        switch ((ColorList)color)
        {
            case ColorList.Original:
                spriteRender.color = new Color(1f, 1f, 1f, 1f);
                break;
            case ColorList.DamagedColor:
                spriteRender.color = new Color(1f, 0.1f, 0.1f, 0.5f);
                break;
            default:
                break;
        }

    }

    private void ReducedHP(float num)
    {
        healthImage.fillAmount -= num;

        if (healthImage.fillAmount <= 0 && player.isActive)
        {
            Death();
        }
    }

    private void Death()
    {
        player.isActive = false;
        rigidBody.velocity = Vector2.zero;
        GameManager.Instance.ReportTheKill(recentAttacker, PhotonNetwork.NickName);
        characterAnim.SetTrigger("death");
    }

    //death애니메이션끝에이벤트달아놈
    public void LateDeath()
    {
        if (PV.IsMine)
        {
            GameManager.Instance.ResponePanel.SetActive(true);
            PV.RPC("DestroyRPC", RpcTarget.AllBuffered); // AllBuffered로 해야 제대로 사라져 복제버그가 안 생긴다
        }
    }
    [PunRPC]
    private void DestroyRPC() => Destroy(gameObject);

    //변수 동기화
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(healthImage.fillAmount);
        }
        else
        {
            healthImage.fillAmount = (float)stream.ReceiveNext();
        }
    }
}