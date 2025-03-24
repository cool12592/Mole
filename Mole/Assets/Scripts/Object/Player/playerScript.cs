using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Unity.Collections; // NativeArray
using Cinemachine;
using System.Drawing;

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
    PlayerHealth health;
    [SerializeField] GamePalette palette;

    // Start is called before the first frame update
    private void Awake()
    {
        health = GetComponent<PlayerHealth>();
        movement = GetComponent<PlayerMovement>();
        NickNameText.text = PV.IsMine ? PhotonNetwork.NickName.ToString() : PV.Owner.NickName.ToString();

        if (PV.IsMine)
        {   
            GameManager.Instance.myplayer = gameObject;
           // GameObject.Find("ObjectPoolParent").transform.GetChild(0).gameObject.SetActive(true);
            InitCamera();            
        }

        StartCoroutine(CoColorSetting());
    }

    private IEnumerator CoColorSetting()
    {
        yield return new WaitUntil(() => GameManager.Instance.FindMyNameIndex(PV.Owner.NickName)!=-1);
        SettingColor(palette.GetColorInfo(GameManager.Instance.FindMyNameIndex(NickNameText.text)));
    }

    void SettingColor(GamePalette.ColorInfo colorInfo)
    {
        if (colorInfo == null)
            return;

        NickNameText.color = colorInfo.color;
        movement._idleSprite = colorInfo.spries[0];
        movement._runSprite = colorInfo.spries[1];
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
    PlayerMovement movement;

    private void OnReadyState()
    {
        isActive = false; //행동 못 하게
        if (_moveParticle != null)
            _moveParticle.SetActive(false);

        if (PhotonNetwork.IsMasterClient)
        {
            TeleportUsers();
        }
    }

    private void OnFightState()
    {
        isActive = true;
        _moveParticle.SetActive(true);
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

    void TeleportUsers()
    {
        if(PhotonNetwork.IsMasterClient == false)
            return;

        List<int> numbers = new List<int>();
        for (int i = 0; i < 9; i++)
        {
            numbers.Add(i);
        }

        // 섞고 앞에서 6개 선택
        Shuffle(numbers);

        playerScript[] allPlayers = FindObjectsOfType<playerScript>();

        int cnt = 0;
        foreach(var player in allPlayers)
        {
            Vector3 pos = GameManager.Instance.startingPositions[numbers[cnt]].position;
            cnt++;
            PV.RPC("TeleportRandomPosition_RPC", RpcTarget.All,pos.x,pos.y,0f, player.PV.Owner.ActorNumber);
        }

        if (PhotonNetwork.IsMasterClient)
        {
            GameStateExecute _stateExecute = FindObjectOfType<GameStateExecute>();
            _stateExecute.OnLateReadyState();
        }
    }

    [PunRPC]
    private void TeleportRandomPosition_RPC(float x, float y, float z, int actorNumber)
    {
        if(PV.Owner.ActorNumber == actorNumber)
        {
            transform.rotation = Quaternion.identity;
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            transform.position = new Vector3(x,y,z);

            meshGenerator.enabled = true;
        }
    }


    void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randIndex = Random.Range(i, list.Count);
            int temp = list[i];
            list[i] = list[randIndex];
            list[randIndex] = temp;
        }
    }
}
