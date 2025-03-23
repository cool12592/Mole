using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class GameStateExecute : MonoBehaviour
{
    public PhotonView PV;

    public GameObject AimJoystick, ResponePanel, ResultPanel;
    [SerializeField] Button MultiReGameButton, StartBtn,ReadyBtn;
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

        WaitInfoText = GameObject.Find("Canvas").transform.Find("WaitText").gameObject.GetComponent<Text>();
    }

    private void OnLobbyState()
    {
        ActiveReadyButton();

        WaitInfoText.text = "Room Number : " + PhotonNetwork.CurrentRoom.Name + "\n Waiting for Host Start...";
    }

    public void ActiveReadyButton()
    {
        if (PhotonNetwork.IsMasterClient)
            ReadyBtn.gameObject.SetActive(true);
    }

    private void OnReadyState()
    {
        GameManager.Instance.StartGame(); 
        GameManager.Instance.ActiveTimer();

        if (GamingUI != null)
            GamingUI.SetActive(true);
        WaitInfoText.text = "";
       
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

        GameManager.Instance.DeactiveResultPanel(GameManager.ResultPanel.MultiDefeat);
        GameManager.Instance.ActiveResultPanel(GameManager.ResultPanel.MultiResult);
        //ResultText.text = "경기 결과\n" + GameManager.Instance.RangkingLogText.text;

    }

    public void ClickReady()
    {
        if (PhotonNetwork.IsMasterClient == false)
            return;

        PV.RPC("ForceReady_RPC", RpcTarget.All);
        ReadyBtn.gameObject.SetActive(false);
        StartBtn.gameObject.SetActive(true);


    }

    public void ClickStart()
    {
        if (PhotonNetwork.IsMasterClient == false)
            return;

        ReadyBtn.gameObject.SetActive(false);
        StartBtn.gameObject.SetActive(false);

        GameStateManager.Instance.ChangeGameStateForAllUser(GameStateManager.GameState.Ready);
    }

    [PunRPC]
    void ForceReady_RPC()
    {
        if (MultiReGameButton.IsActive())
        {
            MultiReGameButton.onClick.Invoke();
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
