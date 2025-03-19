using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerHealth : MonoBehaviourPunCallbacks
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
    [SerializeField] Sprite _dieSprite;

    public bool PlayerActive => player.isActive;

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

        if (Input.GetKeyDown(KeyCode.H)) Death();

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

    public void Death(string attackerName = "")
    {
        if (attackerName == "")
            return;

        if (player == null || player.isActive == false)
            return;

        player.isActive = false;

        if (PV.IsMine == false) return;

        PV.RPC("Death_RPC", RpcTarget.AllBuffered, attackerName);


    }

    [PunRPC]
    void Death_RPC(string attackerName = "")
    {
        player.isActive = false;

        rigidBody.velocity = Vector2.zero;
        //characterAnim.SetTrigger("death");
        spriteRender.sprite = _dieSprite;

        Color dieColor = Color.white;
        dieColor.a = 0.5f;
        spriteRender.color = dieColor;
        var meshGen = GetComponent<MeshGenerator>();
        meshGen.OnALLDestroy();
        meshGen.enabled = false;

        transform.rotation = Quaternion.identity;
        GetComponent<playerScript>().DisConnectCam();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Collider2D col = GetComponent<Collider2D>();

        col.enabled = false;  // 충돌 비활성화
        rb.velocity = new Vector2(0, 7f);  // 위로 솟구치기
        rb.gravityScale = 0;  // 중력 제거

        StartCoroutine(CoLateDeath(attackerName));
    }

    public IEnumerator CoLateDeath(string attackerName)
    {
        yield return new WaitForSeconds(0.5f);  // 잠시 떠 있음

        GetComponent<Rigidbody2D>().gravityScale = 2f;  // 중력 활성화 (떨어지기 시작)

        yield return new WaitForSeconds(1.5f);  // 일정 시간 후 삭제

        if (PV.IsMine)
        {
            GameManager.Instance.ReportTheKill(attackerName, PV.Owner.NickName);
            GameManager.Instance.ResponePanel.SetActive(true);
            PV.RPC("DestroyRPC", RpcTarget.AllBuffered); // AllBuffered로 해야 제대로 사라져 복제버그가 안 생긴다
        }
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



    private IEnumerator DeathAnimation()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Collider2D col = GetComponent<Collider2D>();

        col.enabled = false;  // 충돌 비활성화
        rb.velocity = new Vector2(0, 10f);  // 위로 솟구치기
        rb.gravityScale = 0;  // 중력 제거

        yield return new WaitForSeconds(0.5f);  // 잠시 떠 있음

        rb.gravityScale = 2f;  // 중력 활성화 (떨어지기 시작)

        yield return new WaitForSeconds(1.5f);  // 일정 시간 후 삭제

        Destroy(gameObject);  // 오브젝트 제거
    }

    ////변수 동기화
    //public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    //{
    //    if (stream.IsWriting)
    //    {
    //        stream.SendNext(healthImage.fillAmount);
    //    }
    //    else
    //    {
    //        healthImage.fillAmount = (float)stream.ReceiveNext();
    //    }
    //}
}