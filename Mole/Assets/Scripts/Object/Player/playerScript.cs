using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Unity.Collections; // NativeArray
using Cinemachine;

public class playerScript : MonoBehaviourPunCallbacks
{
    public PhotonView PV;
    public Text NickNameText;
    public bool isActive = true;
    public bool isGoast = false;

    [SerializeField]
    private GameObject moveJoystick;
    [SerializeField]
    private GameObject aimJoystick;
    private Button attackButton;

    CinemachineVirtualCamera CM;

    [SerializeField] MeshGenerator meshGenerator;

    // Start is called before the first frame update
    private void Awake()
    {
        NickNameText.text = PV.IsMine ? PhotonNetwork.NickName.ToString() : PV.Owner.NickName.ToString();
        // NickNameText.color = PV.IsMine ? Color.green : Color.red;
        movement = GetComponent<PlayerMovement>();

        if (PV.IsMine)
        {   
            GameManager.Instance.myplayer = gameObject;
           // GameObject.Find("ObjectPoolParent").transform.GetChild(0).gameObject.SetActive(true);
            InitCamera();            
        }

    }

    void InitCamera()
    {
        // 2D 카메라
        if (CM == null)
        {
            CM = GameObject.Find("CMCamera").GetComponent<CinemachineVirtualCamera>();
        }

        CM.Follow = transform;
        CM.LookAt = transform;
        CM.m_Lens.OrthographicSize = 10;

    }

    public void SetNewTargetCamera(GameObject newTarget)
    {
        CM.Follow = newTarget.transform;
        CM.LookAt = newTarget.transform;
    }

    public void DisConnectCam()
    {
        if(CM != null)
        {
            CM.Follow = null;
            CM.LookAt = null;
        }
    }


    public void Goast()
    {
        //transform.position = Vector3.zero;

        if (PV.IsMine)
        {
            CM.Follow = transform;
            CM.LookAt = transform;
            CM.m_Lens.OrthographicSize = 18f;
        }


        int childCount = transform.childCount;

        for (int i = 0; i < childCount - 1; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Goast");

        GetComponent<SpriteRenderer>().enabled = false;
        isGoast = true;
        GetComponent<PlayerMovement>().moveSpeed = 50;
    }

    public void CheatGoast()
    {
        {
            isActive = false;

            var meshGen = GetComponent<MeshGenerator>();
            meshGen.enabled = false;

            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            Collider2D col = GetComponent<Collider2D>();

            col.enabled = false;  // 충돌 비활성화
            rb.gravityScale = 0;  // 중력 제거

        }

        transform.position = Vector3.zero;


        if (PV.IsMine)
        {
            CM.Follow = transform;
            CM.LookAt = transform;
            CM.m_Lens.OrthographicSize = 18f;
            GameObject.Find("GoastWall").transform.GetChild(0).gameObject.SetActive(true);

        }



        int childCount = transform.childCount;

        for (int i = 0; i < childCount - 1; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Goast");

        GetComponent<SpriteRenderer>().enabled = false;
        isGoast = true;
        GetComponent<PlayerMovement>().moveSpeed = 50;
    }

    private void Start()
    {
        GameStateManager.Instance.ReadyStateAction += OnReadyState;
        GameStateManager.Instance.FightStateAction += OnFightState;
        GameStateManager.Instance.ResultStateAction += OnResultState;
    }

    private void OnDestroy()
    {
        if (GameStateManager.Instance == null)
            return;

        GameStateManager.Instance.ReadyStateAction -= OnReadyState;
        GameStateManager.Instance.FightStateAction -= OnFightState;
        GameStateManager.Instance.ResultStateAction -= OnResultState;
    }


    [SerializeField] GameObject _moveParticle;
    PlayerMovement movement;
    private void OnReadyState()
    {
        isActive = false; //행동 못 하게
        meshGenerator.enabled = true;
        if (_moveParticle != null)
            _moveParticle.SetActive(false);
        if (PV.IsMine)
        {
            ChangeRandomPosition();
            movement.DashInit();
        }
    }

    private void OnFightState()
    {
        isActive = true;
        _moveParticle.SetActive(true);
    }

    private void OnResultState()
    {
        meshGenerator.enabled = false;

        PV.RPC("DestroyRPC", RpcTarget.All);
    }

    private void ChangeRandomPosition()
    {
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        transform.position = new Vector3(UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(-5f, 5f), 0);
        GetComponent<Animator>().SetBool("walk", false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
            CheatGoast();
    }
}
