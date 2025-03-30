using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Unity.Collections; // NativeArray
using Cinemachine;
using System.Drawing;
using Photon.Pun.Demo.PunBasics;
using static UnityEngine.UI.CanvasScaler;

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
    public MeshGenerator meshGenerator;
    PlayerHealth health;
    [SerializeField] GamePalette palette;

    public string IsSingleNickName = "Player";
    
    public bool IsEnemy = false;

    bool isInit = false;

    public void SetNickText(string str)
    {
        NickNameText.text = str;
    }

    // Start is called before the first frame update
    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        if (isInit)
            return;

        isInit = true;

        health = GetComponent<PlayerHealth>();
        movement = GetComponent<PlayerMovement>();

        if (GameManager.Instance.IsSingleMode == false)
            NickNameText.text = PV.IsMine ? PhotonNetwork.NickName.ToString().Substring(2) : PV.Owner.NickName.ToString().Substring(2);

        if (GameManager.Instance.IsSingleMode || PV.IsMine)
        {
            if (IsEnemy == false)
            {
                GameManager.Instance.myplayer = gameObject;
                InitCamera();
            }
        }

        if (GameManager.Instance.IsSingleMode == false)
            StartCoroutine(CoColorSetting());

    }

    private IEnumerator CoColorSetting()
    {
        yield return new WaitUntil(() => GameManager.Instance.FindMyNameIndex(PV.Owner.NickName)!=-1);
        SettingColor(palette.GetColorInfo(GameManager.Instance.FindMyNameIndex(PV.Owner.NickName)));
    }

    public void SettingColor(GamePalette.ColorInfo colorInfo)
    {
        if (colorInfo == null)
            return;

        Init();

        NickNameText.color = colorInfo.color;
        movement._idleSprite = colorInfo.spries[0];
        movement._runSprite = colorInfo.spries[1];

        movement._curIdleSprite = colorInfo.spries[0];
        movement._curRunSprite = colorInfo.spries[1];

        health._dieSprite = colorInfo.spries[2];

        meshGenerator.SetMyColor(colorInfo.color);
        GetComponent<SpriteRenderer>().sprite = colorInfo.spries[0];
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
        CM.m_Lens.OrthographicSize = Creative.Instance.StartcameraZoom;

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

    [SerializeField] Collider2D wallCollider;
    public void Goast()
    {
        //transform.position = Vector3.zero;

        if (PV.IsMine)
        {
            CM.Follow = transform;
            CM.LookAt = transform;
            CM.m_Lens.OrthographicSize = 18f;

            GameManager.Instance.ActiveResultPanel(GameManager.ResultPanel.MultiDefeat);
        }


        int childCount = transform.childCount;

        for (int i = 0; i < childCount - 1; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        if(wallCollider!=null)
        {
            wallCollider.enabled = true;
            wallCollider.gameObject.layer = LayerMask.NameToLayer("Goast");
        }

        transform.position = Vector3.zero;

        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<PlayerMovement>().moveSpeed = 50;

        isGoast = true;
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
    public PlayerMovement movement;

    private void OnReadyState()
    {
        if (isActive == false)
            return;

        isActive = false; //행동 못 하게
        if (_moveParticle != null)
            _moveParticle.SetActive(false);

        if (PV. Owner.IsMasterClient)
        {
            TeleportUsers();
        }
    }

    private void OnFightState()
    {
        if(GameManager.Instance.IsSingleMode && IsEnemy == false)
            Creative.Instance.StartIntroZoom(CM);

        isActive = true;
        _moveParticle.SetActive(true);
    }

    public void ChangeDrillZoom(float num)
    {
        Creative.Instance.ChangeZoom(CM, CM.m_Lens.OrthographicSize+num,0.5f);
    }

    private void OnResultState()
    {
        meshGenerator.OnALLDestroy();
        meshGenerator.enabled = false;

        PV.RPC("DestroyRPC", RpcTarget.AllBuffered);
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
            CheatGoast();
    }

    [PunRPC]
    public void TeleportRandomPosition_RPC(float x, float y, float z)
    {
        transform.rotation = Quaternion.identity;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        transform.position = new Vector3(x,y,z);

        meshGenerator.enabled = true;
    }

    public void TeleportUsers()
    {
        if (PhotonNetwork.IsMasterClient == false)
            return;

        playerScript[] allPlayers = FindObjectsOfType<playerScript>();
        Vector2 center = Vector2.zero;
        int userCount = allPlayers.Length;
        float minDistance = 7f;


        List<Vector2> positions = new List<Vector2>();
        for (int i = 0; i < userCount; i++)
        {
            positions.Add(Vector2.zero);
        }

        for (int i = 0; i < userCount; i++)
        {
            Vector2 pos;
            int attempts = 0;
            do
            {
                pos = new Vector3(Random.Range(-12f, 15f), Random.Range(-20f, 9f), 0f);
                attempts++;
                if (attempts > 10000)
                {
                    break;
                }
            } while (positions.Exists(p => Vector2.Distance(p, pos) < minDistance));

            positions[i] = pos;
        }

        int cnt = 0;
        foreach (var player in allPlayers)
        {
            Vector3 pos = positions[cnt++];
            player.PV.RPC("TeleportRandomPosition_RPC", RpcTarget.All, pos.x, pos.y, 0f);
        }


        if (PhotonNetwork.IsMasterClient)
        {
            GameStateExecute _stateExecute = FindObjectOfType<GameStateExecute>();
            _stateExecute.OnLateReadyState();
        }
    }


    

}
