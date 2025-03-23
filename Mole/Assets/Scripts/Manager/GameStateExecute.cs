using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class GameStateExecute : MonoBehaviour
{
    public PhotonView PV;

    public GameObject AimJoystick, ResponePanel, ResultPanel;
    public Button ReGameButton;
    public Text ResultText;

    WaitForSeconds waitForSecond = new WaitForSeconds(1f);
    Text WaitInfoText;
    GameObject GamingUI;

    private void Start()
    { 
        init();
        GameStateManager.Instance.LobbyStateAction += OnLobbyState;
        GameStateManager.Instance.ReadyStateAction += OnReadyState;
        GameStateManager.Instance.FightStateAction += OnFightState;
        GameStateManager.Instance.ResultStateAction += OnResultState;
        GamingUI = GameObject.Find("Canvas").transform.Find("Gaming").gameObject;

    }

    private void init()
    {
        PV = GetComponent<PhotonView>();

        AimJoystick = GameObject.Find("Canvas").transform.Find("Aim_Joystick").gameObject;
        ResponePanel = GameObject.Find("Canvas").transform.Find("RespawnPanel").gameObject;
        ResultPanel = GameObject.Find("Canvas").transform.Find("ResultPanel").gameObject;

        WaitInfoText = GameObject.Find("Canvas").transform.Find("WaitText").gameObject.GetComponent<Text>();
        ReGameButton = ResultPanel.transform.Find("regameBTN").gameObject.GetComponent<Button>();
        ResultText = ResultPanel.transform.Find("resultText").gameObject.GetComponent<Text>();
    }

    private void OnLobbyState()
    {
        GameStateManager.Instance.ActiveStartBtn();

        WaitInfoText.text = "Room Number : " + PhotonNetwork.CurrentRoom.Name + "\n Waiting for Host Start...";
    }

    private void OnReadyState()
    {
        GameManager.Instance.StartGame(); 
        GameManager.Instance.ActiveTimer();

        if (GamingUI != null)
            GamingUI.SetActive(true);
        WaitInfoText.text = "";
        if (ReGameButton.IsActive())
            ReGameButton.onClick.Invoke();

        GameStateManager.Instance.DeactiveStartBtn();


        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(ReadyCoroutine());
    }

    private void OnFightState()
    {
        GameManager.Instance.StartTimer();
       // AimJoystick.SetActive(true);
    }

    private void OnResultState()
    {
        GameManager.Instance.EndTimer();

        if (GamingUI != null)
            GamingUI.SetActive(false);

        AimJoystick.SetActive(false);
        ResponePanel.SetActive(false);
        ResultPanel.SetActive(true);
        //ResultText.text = "경기 결과\n" + GameManager.Instance.RangkingLogText.text;

    }

    public void ChangeReadyState()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameStateManager.Instance.ChangeGameStateForAllUser(GameStateManager.GameState.Ready);
        }
    }

    [SerializeField] GameObject[] CountDownImages;

    IEnumerator ReadyCoroutine()
    {
        GameManager.Instance.PV.RPC("SetScreenTextRPC", RpcTarget.All, "", 100);

        PV.RPC("SetCountDown_RPC", RpcTarget.All, 0);
        yield return waitForSecond;
        PV.RPC("SetCountDown_RPC", RpcTarget.All, 1);
        yield return waitForSecond;
        PV.RPC("SetCountDown_RPC", RpcTarget.All, 2);
        yield return waitForSecond;
        PV.RPC("SetCountDown_RPC", RpcTarget.All, 3);


        PV.RPC("ChangeGameStateForAllUser", RpcTarget.All, GameStateManager.GameState.Fight);

        yield return waitForSecond;
        PV.RPC("SetCountDown_RPC", RpcTarget.All, 4);
    }

    [PunRPC]
    void SetCountDown_RPC(int ind)
    {
        if(CountDownImages.Length <= ind)
        {
            CountDownImages[CountDownImages.Length-1].SetActive(false);
            return;
        }

        if(ind!=0)
        {
            CountDownImages[ind - 1].SetActive(false);
        }
        CountDownImages[ind].SetActive(true);
    }
}
