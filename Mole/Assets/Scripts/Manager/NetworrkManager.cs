using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon.StructWrapping;
using System;

public class NetworrkManager : MonoBehaviourPunCallbacks
{
    public InputField NickNameInput;
    public InputField PrivateRoomInput;

    public GameObject DisconnectPanel;
    public GameObject RespawnPanel;
    public GameObject GameEndPanel;

    [SerializeField] AudioSource BGM;

    [SerializeField] GameObject MainRobbyUI;
    [SerializeField] GameObject MultiRobbyUI;

    public static readonly List<string> CommonSymbols = new List<string>
    {
        // 대문자 A~Z
        "A", "B", "C", "D", "E", "F", "G", "H", "I", "J",
        "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T",
        "U", "V", "W", "X", "Y", "Z",

        // 소문자 a~z
        "a", "b", "c", "d", "e", "f", "g", "h", "i", "j",
        "k", "l", "m", "n", "o", "p", "q", "r", "s", "t",
        "u", "v", "w", "x", "y", "z"
    };


    public void ActiveMultiUI()
    {
        MainRobbyUI.SetActive(false);
        MultiRobbyUI.gameObject.SetActive(true);
        NickNameInput.text = "";
        PrivateRoomInput.text = "";
    }

    public void ActiveMainUI()
    {
        MainRobbyUI.SetActive(true);
        MultiRobbyUI.gameObject.SetActive(false);
    }

    public void StartButton()
    {
        GameManager.Instance.StartShrinkScaleCoroutine(Vector3.zero, Connect);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        PhotonNetwork.Disconnect();
        GameManager.Instance.StartShrinkScaleCoroutine(Vector3.one * 2f, null);
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!BGM.isPlaying)
            {
                BGM.Stop(); 
            }

            if (GameManager.Instance.IsSingleMode)
            {
                SceneManager.LoadScene("SampleScene");
            }
            else if (PhotonNetwork.IsConnected == false && PhotonNetwork.InRoom == false)
                Application.Quit();
            else
            {
                ExitGame();
            }
        }
    }

    public void Connect()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        string nickName = CommonSymbols[UnityEngine.Random.Range(0, CommonSymbols.Count)] + CommonSymbols[UnityEngine.Random.Range(0, CommonSymbols.Count)];
        if(NickNameInput.text=="")
        {
            nickName += UnityEngine.Random.Range(1000, 10000).ToString();
        }
        else
        {
            nickName += NickNameInput.text;
        }

        PhotonNetwork.LocalPlayer.NickName = nickName;

        if(PrivateRoomInput.text != "")
        {
            RoomOptions roomoption = new RoomOptions { MaxPlayers = 6 };
            roomoption.EmptyRoomTtl = 0; // 방에 아무도 없으면 즉시 삭제
            PhotonNetwork.JoinOrCreateRoom(PrivateRoomInput.text, roomoption, null);
        }
        else
        {
            PhotonNetwork.JoinRandomRoom();
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        CreateNewRoom();
    }

    void CreateNewRoom()
    {
        string roomName = (UnityEngine.Random.Range(0, 10000)).ToString();
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 6 };
        roomOptions.EmptyRoomTtl = 0; // 방에 아무도 없으면 즉시 삭제
        PhotonNetwork.CreateRoom(roomName, roomOptions, null);
    }

    //내입장에서 내가 들어왔을 때 이거호출
    public override void OnJoinedRoom()
    {
        DisconnectPanel.SetActive(false);
        spawn();

        if (!BGM.isPlaying)
        {
            BGM.Play(); // 현재 재생 중이 아닐 때만 Play()
        }

        //방장입장용 (밑에선안됨)
        if (PhotonNetwork.IsMasterClient)
        {
            GameManager.Instance.StartRankingBoardCoroutine();
            GameStateManager.Instance.ChangeGameState(GameStateManager.GameState.Lobby);
            GameManager.Instance.UserJoin(PhotonNetwork.LocalPlayer.NickName);
            
        }
    }

    //내 입장에서 남이들어온 상황때 이거 호출
    //방장이첨에들어올땐실행안됨
    //방장말고다른사람입장용 (방장입장에선 다른사람이들어온상황)
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameManager.Instance.UserJoin(newPlayer.NickName);
        }
    }


    //방장이 나가면 다른유저가 방장되고 이거 실행됨 
    //현방장이 전 방장나간걸 입력받는거지

    //그리고 강종은 도저히 방법이없음 죽기전에 기록할수가없음
    //그냥 일정주기로 rankingBoard 동기화가 내 결론
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameManager.Instance.StartRankingBoardCoroutine();
            GameManager.Instance.UserLeft(otherPlayer.NickName);
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if(DisconnectPanel != null)
            DisconnectPanel.SetActive(true);
        if (RespawnPanel != null)
            RespawnPanel.SetActive(false);
    }

    public void spawn()
    {
        Vector3 spawnPosition;
        int maxAttempts = 100;
        int attempt = 0;
        float checkRadius = 2f;

        do
        {
            spawnPosition = new Vector3(UnityEngine.Random.Range(-12f, 15f), UnityEngine.Random.Range(-20f, 9f),0f); // 3D 좌표

            // 스폰 위치에 플레이어가 있는지 체크
            bool hasPlayer = Physics.CheckSphere(spawnPosition, checkRadius, LayerMask.GetMask("Player"));

            if (!hasPlayer) // 아무도 없으면 스폰
            {
                PhotonNetwork.Instantiate("Player", spawnPosition, Quaternion.identity);
                GameManager.Instance.ResponePanel.SetActive(false);


                GameManager.Instance.StartShrinkScaleCoroutine(Vector3.one * 2f, null);

                return;
            }

            attempt++;

        } while (attempt < maxAttempts);

        Debug.LogWarning("스폰할 수 있는 위치를 찾을 수 없습니다.");
    }

    // public void NewGameSpawn()
    // {
    //     PhotonNetwork.Instantiate("Player", new Vector3(UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(-5f, 5f), 0), Quaternion.identity);
    //     GameManager.Instance.DeactiveResultPanel(GameManager.ResultPanel.MultiResult);

    //     GameStateManager.Instance.ChangeGameState(GameStateManager.GameState.Lobby);
    // }


    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        // 여기서 마스터 전용 로직 처리 가능
        if (PhotonNetwork.IsMasterClient)
        {
            if(GameStateManager.Instance.NowGameState == GameStateManager.GameState.Lobby)
            {
                GameStateExecute.ActiveReadyButton();
            }
        }
    }

    [SerializeField] GameStateExecute GameStateExecute;




    public void ExitGame()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom(); // 이 후 OnLeftRoom()에서 Disconnect
        }
        else if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
            SceneManager.LoadScene("SampleScene");

        }
    }


    public override void OnLeftRoom()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
            SceneManager.LoadScene("SampleScene");

        }
    }

    public void SingleConnect()
    {
        GameManager.Instance.StartShrinkScaleCoroutine(Vector3.zero, SingleSpawn);
    }

    public void SingleSpawn()
    {
        GameManager.Instance.SingleClear();

        DisconnectPanel.SetActive(false);
        GameManager.Instance.IsSingleMode = true;
        Vector3 spawnPosition;
        int maxAttempts = 1000;
        int attempt = 0;
        float checkRadius = 7f;

        int cnt = 0;
        do
        {
            spawnPosition = new Vector3(UnityEngine.Random.Range(-12f, 15f), UnityEngine.Random.Range(-20f, 9f),0f); // 3D 좌표

            // 스폰 위치에 플레이어가 있는지 체크
            bool hasPlayer = Physics.CheckSphere(spawnPosition, checkRadius, LayerMask.GetMask("Player"));

            if (!hasPlayer) // 아무도 없으면 스폰
            {
                if(cnt == 0)
                {
                    var player = Instantiate(SinglePlayer, spawnPosition, Quaternion.identity).GetComponent<playerScript>();
                    player.SettingColor(gamePalette.GetColorInfo(cnt));

                    string nickName = CommonSymbols[UnityEngine.Random.Range(0, CommonSymbols.Count)] + CommonSymbols[UnityEngine.Random.Range(0, CommonSymbols.Count)];
                    nickName += "Player";
                    player.IsSingleNickName = nickName;
                    GameManager.Instance.SinglePlayer = player;
                    GameManager.Instance.ResponePanel.SetActive(false);
                    GameManager.Instance.SingleUserJoin(player.IsSingleNickName);

                    cnt++;
                }
                else
                {
                    var player = Instantiate(Enemys, spawnPosition, Quaternion.identity).GetComponent<playerScript>();

                    string nickName = CommonSymbols[UnityEngine.Random.Range(0, CommonSymbols.Count)] + CommonSymbols[UnityEngine.Random.Range(0, CommonSymbols.Count)];

                    player.IsSingleNickName = nickName + cnt.ToString();
                    player.SettingColor(gamePalette.GetColorInfo(cnt));
                    cnt++;

                    GameManager.Instance.SingleUserJoin(player.IsSingleNickName);

                }



                if (cnt==6)
                {
                    GameManager.Instance.SingleAllMemberCount = cnt;
                    
                    GameManager.Instance.StartShrinkScaleCoroutine(Vector3.one * 2f, null);
                    return;
                }
            }

            attempt++;

        } while (attempt < maxAttempts);

        Debug.LogWarning("스폰할 수 있는 위치를 찾을 수 없습니다.");

    }

    public void SingleRestart()
    {
        if (BGM.isPlaying)
        {
            BGM.Stop(); // 현재 재생 중이 아닐 때만 Play()
        }

        MeshGenerator[] allPlayers = FindObjectsOfType<MeshGenerator>();
        foreach (var player in allPlayers)
        {
            player.OnALLDestroy();
            Destroy(player.gameObject);
        }

        GlobalRoadPool.Instance.RestartPool();
        GlobalSpritePool.Instance.RestartPool();

        GameManager.Instance.DeactiveResultPanel(GameManager.ResultPanel.SingleVictory);
        GameManager.Instance.DeactiveResultPanel(GameManager.ResultPanel.SingleDefeat);

        GameStateManager.Instance.ChangeGameState(GameStateManager.GameState.None);

        SingleSpawn();


    }

    [SerializeField] GameObject SinglePlayer;

    [SerializeField] GamePalette gamePalette;
    [SerializeField] playerScript Enemys;


}
