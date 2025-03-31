using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Cinemachine;

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

    private const float originalSpeed = 5f;
    public float moveSpeed = originalSpeed;
    private int dashCount = 2;
    private const float moveCoefficient = 60f;

    // 🔴 receiveRotation 변수 추가
    private Quaternion receiveRotation;

    SpriteRenderer _spriteRenderer;
    public Sprite _idleSprite;
    public Sprite _runSprite;
    bool _isIdle = true;
    [SerializeField] float animChangeTerm = 0.1f;
    float nextChangeAnimTime = 0f;

    // Start is called before the first frame update
    void Start()
    {
        impulseSource = GameObject.Find("CinemachineImpulseSource").GetComponent<CinemachineImpulseSource>();
        if(GameManager.Instance.IsSingleMode==false)
            PV = GetComponent<PhotonView>();
        health = GetComponent<PlayerHealth>();
        player = GetComponent<playerScript>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        characterAnim = GetComponent<Animator>();
        rigidBody = gameObject.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if(GameManager.Instance.IsSingleMode==false)
            otherPositionSync();

        if (GameManager.Instance.IsSingleMode || PV.IsMine)
        {
            Move(JoyStickScript.InputAxis);
        }
    }

    //변수 동기화
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation); // 🔴 회전 값 추가

        }
        else
        {
            receivePos = (Vector3)stream.ReceiveNext();
            receiveRotation = (Quaternion)stream.ReceiveNext(); // 🔴 회전 값 수신

        }
    }

    private void otherPositionSync()
    {
        if (PV.IsMine == false)
        {
            if ((transform.position - receivePos).sqrMagnitude >= 10)
                transform.position = receivePos;
            else
                transform.position = Vector3.Lerp(transform.position, receivePos, Time.deltaTime * 10);

            transform.rotation = Quaternion.Lerp(transform.rotation, receiveRotation, Time.deltaTime * 10); // 🔴 회전 보간 적용
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
        if (player.isActive == false)
            return;

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

    public void ChangeAnim()
    {
        if(nextChangeAnimTime < Time.time)
        {
            nextChangeAnimTime = Time.time + animChangeTerm;
            if(_isIdle)
            {
                _spriteRenderer.sprite = _runSprite;
                _isIdle = false;
            }
            else
            {
                _spriteRenderer.sprite = _idleSprite;
                _isIdle = true;
            }
        }
    }

    public void Move(Vector2 inputDirection)
    {
        if (player.isActive == false && player.isGoast == false) 
            return;

  
        if(player.IsEnemy)
        {
            return;
        }

        float magnitude = 1f;
        if (player.meshGenerator.IsDrillMode)
        {
            magnitude = 1.2f;
        }
        rigidBody.velocity = inputDirection * moveSpeed * magnitude;
 

        if (inputDirection != Vector2.zero)
        {
            float angle = Mathf.Atan2(inputDirection.y, inputDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90);

           // ShakeCamera();

            ChangeAnim();
        }
        else
        {
            _spriteRenderer.sprite = _idleSprite;
            _isIdle = true;
        }
    }

    CinemachineImpulseSource impulseSource;

    public void ShakeCamera()
    {
        impulseSource.GenerateImpulse();
    }





    
}
