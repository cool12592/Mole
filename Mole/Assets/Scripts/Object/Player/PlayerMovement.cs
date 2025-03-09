using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviourPunCallbacks, IPunObservable
{
    public PhotonView PV;
    private PlayerHealth health;
    private playerScript player;
    private GameObject dashBtnObject;
    private Button dashBtn;
    private Text dashBtnText;
    private Image dashCoolTimeImage;
    private Animator characterAnim;
    private Rigidbody2D rigidBody;
    public Vector3 receivePos;
    public Transform[] _transformArray; // 대상 트랜스폼들 등록

    private const float originalSpeed = 270f;
    private float moveSpeed = originalSpeed;
    private int dashCount = 2;
    private const float moveCoefficient = 60f;

    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();
        health = GetComponent<PlayerHealth>();
        player = GetComponent<playerScript>();

        characterAnim = GetComponent<Animator>();
        rigidBody = gameObject.GetComponent<Rigidbody2D>();

        if (PV.IsMine)
        {
            dashBtnObject = GameObject.Find("Canvas").transform.Find("DashButton").gameObject;
            dashBtnObject.SetActive(true);
            dashBtn = dashBtnObject.GetComponent<Button>();
            dashBtn.onClick.AddListener(Dash);

            dashCoolTimeImage = GameObject.Find("Canvas").transform.Find("DashCoolTime").gameObject.GetComponent<Image>();
            dashBtnText = GameObject.Find("Canvas").transform.Find("DashButton").transform.Find("Text").GetComponent<Text>();
        }

    }

    // Update is called once per frame
    void Update()
    {
        otherPositionSync();
        if (PV.IsMine)
        {
            RunDashCoolTime();
            SpeedReturnsAfterDash();
            AnimationBranch();
        }
        if (Input.GetKeyDown(KeyCode.R)) Dash();
    }

    //변수 동기화
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
        }
        else
        {
            receivePos = (Vector3)stream.ReceiveNext();
        }
    }

    private void otherPositionSync()
    {
        if (PV.IsMine == false)
        {
            if ((transform.position - receivePos).sqrMagnitude >= 100) transform.position = receivePos; //위치가 동기화 위치랑 너무 멀어지면 동기화 위치로 만듬
            else transform.position = Vector3.Lerp(transform.position, receivePos, Time.deltaTime * 10); //그게 아니면 위치를 동기화 받은위치로 보간시킴
        }
    }

    public void Dash()
    {
        if (player.isActive == false)
            return;

        if (dashCount <= 0)
            return;

        SoundManager.Instance.PlayDashSound();
        health.OnInvincibility();
        ChangeDashCount(--dashCount);
        characterAnim.SetTrigger("Dash");
        PV.RPC("DashRPC", RpcTarget.All); //All모든사람들한테 
    }

    private void ChangeDashCount(int num)
    {
        if (num < 0 || 2 < num)
            return;

        dashCount = num;
        dashBtnText.text = "대쉬" + dashCount;

        if(dashCount == 2)
            dashCoolTimeImage.fillAmount = 0f;
        else
            dashCoolTimeImage.fillAmount = 1.0f;
    }

    [PunRPC]
    private void DashRPC()
    {
        moveSpeed = 800f;
    }

    private void SpeedReturnsAfterDash()
    {
        if (moveSpeed > originalSpeed)
        {
            moveSpeed -= Time.deltaTime * 800;

            if (moveSpeed <= originalSpeed)
            {
                health.OffInvincibility();
                moveSpeed = originalSpeed;
            }
        }        
    }

    private void RunDashCoolTime()
    {
        if (dashCoolTimeImage.fillAmount > 0f)
        {
            dashCoolTimeImage.fillAmount -= Time.deltaTime;
            if (dashCoolTimeImage.fillAmount <= 0)
            {
                ChangeDashCount(++dashCount);
            }
        }
    }
    public void DashInit() //대쉬쿨초기화 
    {
        health.OffInvincibility();
        ChangeDashCount(2);
        moveSpeed = originalSpeed;
    }

    private void AnimationBranch()
    {
        if (rigidBody.velocity != Vector2.zero)
        {
            characterAnim.SetBool("walk", true);

        }
        else characterAnim.SetBool("walk", false);
    }

    public void Move(Vector2 inputDirection)
    {
        if (player.isActive == false) return;
        rigidBody.velocity = inputDirection * moveSpeed / moveCoefficient;
    }
}
