using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.IO;

public class NetworrkManager : MonoBehaviourPunCallbacks
{
    public InputField NickNameInput;
    public GameObject DisconnectPanel;
    public GameObject RespawnPanel;
    public GameObject GameEndPanel;

    [SerializeField] AudioSource BGM;

    private void Awake()
    {
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (PhotonNetwork.IsConnected)
                PhotonNetwork.Disconnect();
            else
                Application.Quit();
        }
    }

    public void Connect()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.LocalPlayer.NickName = NickNameInput.text;
        RoomOptions roomoption = new RoomOptions { MaxPlayers = 20 };
        PhotonNetwork.JoinOrCreateRoom("Room", roomoption, null);
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
            spawnPosition = new Vector3(Random.Range(-12f, 12f), Random.Range(-12f, 12f),0f); // 3D 좌표

            // 스폰 위치에 플레이어가 있는지 체크
            bool hasPlayer = Physics.CheckSphere(spawnPosition, checkRadius, LayerMask.GetMask("Player"));

            if (!hasPlayer) // 아무도 없으면 스폰
            {
                PhotonNetwork.Instantiate("Player", spawnPosition, Quaternion.identity);
                GameManager.Instance.ResponePanel.SetActive(false);
                return;
            }

            attempt++;

        } while (attempt < maxAttempts);

        Debug.LogWarning("스폰할 수 있는 위치를 찾을 수 없습니다.");
    }

    public void NewGameSpawn()
    {
        PhotonNetwork.Instantiate("Player", new Vector3(Random.Range(-10f, 10f), Random.Range(-5f, 5f), 0), Quaternion.identity);
        GameManager.Instance.ResultPanel.SetActive(false);

        GameStateManager.Instance.ChangeGameState(GameStateManager.GameState.Lobby);
    }
}
