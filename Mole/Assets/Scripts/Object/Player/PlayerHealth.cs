using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Lofelt.NiceVibrations;

public class PlayerHealth : MonoBehaviourPunCallbacks
{
    
    
    public PhotonView PV;
    [SerializeField]
    private Image healthImage;
    private Rigidbody2D rigidBody;
    private Animator characterAnim;
    public playerScript player;
    private SpriteRenderer spriteRender;

    private bool invincibility = false;
    private string recentAttacker;

    private enum ColorList { Original, DamagedColor };
    Timer.TimerStruct healthTimer = new Timer.TimerStruct(0.35f);
    public Sprite _dieSprite;

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
    // void Update()
    // {
    //     RunTimer();
    //     if (Input.GetKeyDown(KeyCode.T)) TakeDamage(PhotonNetwork.NickName);

    //     //if (Input.GetKeyDown(KeyCode.H)) Death();

    // }

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
        PV.RPC("ChangeColorRPC", RpcTarget.All, (int)color);
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
            //Death();
        }
    }

    public void Death(playerScript attacker, string attackerName, MeshGenerator.GenerateMeshType type)
    {
        if (GameManager.Instance.IsSingleMode ==false && PhotonNetwork.IsMasterClient == false)
            return;

        if (GetComponent<MeshGenerator>().isDrillMode)
            return;

        if (attackerName == "")
            return;

        if (player == null || player.isActive == false)
            return;

        if (attacker.isActive == false) //죽은애가 다시 못죽이게
            return;


        if (GameManager.Instance.IsSingleMode==false)
        {
            if (attackerName == PV.Owner.NickName)
                return;
            
        }
        else
        {
            if (attackerName == player.IsSingleNickName)
                return;
        }

        player.isActive = false;

        if (GameManager.Instance.IsSingleMode==false)
            PV.RPC("Death_RPC", RpcTarget.All, attackerName, (int)type);
        else
            Death_RPC(attackerName,(int)type);
    }

    [SerializeField] Collider2D wallCollider;
    [SerializeField] Collider meshCollider;
    [SerializeField] Collider hitMeshCollider;

    [PunRPC]
    void Death_RPC(string attackerName,int type)
    {
        if (GameManager.Instance.IsSingleMode || PV.IsMine)
        {
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);
        }

        wallCollider.enabled = false;
        meshCollider.enabled = false;
        hitMeshCollider.enabled = false;

        var attackerMesh = GameManager.Instance.UserMeshMap[attackerName];

        if (GameManager.Instance.IsSingleMode==false)
            attackerMesh.TakeAwayLand(PV.Owner.NickName,(MeshGenerator.GenerateMeshType)type );
        else
            attackerMesh.TakeAwayLand(player.IsSingleNickName, (MeshGenerator.GenerateMeshType)type);

        player.isActive = false;

        rigidBody.velocity = Vector2.zero;
        spriteRender.sprite = _dieSprite;

        Color dieColor = Color.white;
        dieColor.a = 0.5f;
        spriteRender.color = dieColor;
        var meshGen = GetComponent<MeshGenerator>();
        //meshGen.OnALLDestroy();
        meshGen.enabled = false;

        transform.rotation = Quaternion.identity;
        player.DisConnectCam();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Collider2D col = GetComponent<Collider2D>();

        col.enabled = false;  // 충돌 비활성화
        rb.velocity = new Vector2(0, 7f);  // 위로 솟구치기
        rb.gravityScale = 0;  // 중력 제거


        if(GameManager.Instance.IsSingleMode)
        {
            GameManager.Instance.ReportTheKill(attackerName, player.IsSingleNickName);
        }
        else if(PV.IsMine)
        {
            GameManager.Instance.ReportTheKill(attackerName, PV.Owner.NickName);
        }
        
        StartCoroutine(CoLateDeath(attackerName));
    }

    public IEnumerator CoLateDeath(string attackerName)
    {
        yield return new WaitForSeconds(0.5f);  // 잠시 떠 있음

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 2f;  // 중력 활성화 (떨어지기 시작)

        yield return new WaitForSeconds(1.5f);  // 일정 시간 후 삭제

        rb.gravityScale = 0f;  
        
        if(GameManager.Instance.IsSingleMode)
        {
            Destroy(player.gameObject);

            if(player == GameManager.Instance.SinglePlayer)
                GameManager.Instance.ActiveResultPanel(GameManager.ResultPanel.SingleDefeat);
        }
        else
        {
            player.Goast();
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