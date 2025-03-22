using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using System;
using UnityEngine.Windows;
public class GameManager : MonoBehaviour
{
    public PhotonView PV;
    private static GameManager instance = null;
    private Text ScreenText;
   // public Text RangkingLogText { get; private set; }
    private Queue<KeyValuePair<string, string>> killLogQueue = new Queue<KeyValuePair<string, string>>();
    private Dictionary<string, float> RankingBoard = new Dictionary<string, float>();
    public Dictionary<string, MeshGenerator> UserMeshMap = new Dictionary<string, MeshGenerator>();

    public GameObject myplayer, ResponePanel, ResultPanel;

    IEnumerator rankingBoardCoroutine;
    WaitForSeconds waitForSecnds = new WaitForSeconds(1f);

    public Dictionary<int, Color> UserColor = new Dictionary<int, Color>();
    int MemberCount = 0;
    HashSet<string> deadPersonSet = new HashSet<string>();

    public void StartGame()
    {
        _isGameEnd = false;
        if (PhotonNetwork.IsMasterClient)
            MemberCount = RankingBoard.Count;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                return null;
            }
            return instance;
        }
    }

    private void Start()
    {
        ScreenText = GameObject.Find("Canvas").transform.Find("ScreenText").gameObject.GetComponent<Text>();
       // RangkingLogText = GameObject.Find("Canvas").transform.Find("RankingLog").gameObject.GetComponent<Text>();
        ResponePanel = GameObject.Find("Canvas").transform.Find("RespawnPanel").gameObject;
        ResultPanel = GameObject.Find("Canvas").transform.Find("ResultPanel").gameObject;
    }

    public void StartRankingBoardCoroutine()
    {
        if (PhotonNetwork.IsMasterClient == false)
            return;

        if (rankingBoardCoroutine != null)
            return;

        rankingBoardCoroutine = RankingBoardCoroutine();
        StartCoroutine(rankingBoardCoroutine);
    }

    IEnumerator RankingBoardCoroutine()
    {
        while (true)
        {
            SynchTimer();
            SynchRankingBoard();
            yield return waitForSecnds;
        }
    }

    private void SynchTimer()
    {
        if (_onTimer == false)
            return;
        if (PhotonNetwork.IsMasterClient)
            PV.RPC("SynchTimer_RPC", RpcTarget.All, timer); 
    }

    [PunRPC]
    private void SynchTimer_RPC(float timer_)
    {
        SetTimer(timer_);
    }

    private void SynchRankingBoard()
    {
        if (PhotonNetwork.IsMasterClient)
            PV.RPC("SynchRankingBoardRPC", RpcTarget.AllBuffered, RankingBoard); //방장떠날때 대비 이것도 동기화해줘야됨
    }

    [PunRPC]
    private void SynchRankingBoardRPC(Dictionary<string, float> rankingBoard)
    {
        RankingBoard = rankingBoard;
    }

    public void UserJoin(string nickName)
    {
        if (PhotonNetwork.IsMasterClient)
        {
           if(RankingBoard.ContainsKey(nickName) == false) 
                RankingBoard.Add(nickName, 0);
            UpdateRankingBoard();

            if (PhotonNetwork.LocalPlayer.NickName != nickName)
                InitGameState(nickName);
        }
    }

    private void InitGameState(string nickName)
    {
        if (PhotonNetwork.IsMasterClient)
            PV.RPC("InitGameStateRPC", RpcTarget.AllBuffered, nickName, (int)GameStateManager.Instance.NowGameState);
    }

    [PunRPC]
    private void InitGameStateRPC(string nickname, int gamestate)
    {
        if(PhotonNetwork.LocalPlayer.NickName == nickname)
        {
            GameStateManager.Instance.ChangeGameState((GameStateManager.GameState)gamestate);
        }
    }

    public void UserLeft(string nickName)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            deadPersonSet.Add(nickName);
            if (RankingBoard.ContainsKey(nickName))
                RankingBoard.Remove(nickName);
            UpdateRankingBoard();
        }
    }

    void WriteDeadPeson(string deadNickName)
    {
        deadPersonSet.Add(deadNickName);

        if(MemberCount == deadPersonSet.Count)
        {
            OnEndGame();
        }
    }

    [SerializeField] Sprite otherRankBackGroundSprite;
    [SerializeField] Sprite myRankBackGroundSprite;
    [SerializeField] Text[] RankTexts;
    [SerializeField] Image[] RankImages;

    [SerializeField] GameObject[] RankObjs;

    private void UpdateRankingBoard()
    {
        if (!PhotonNetwork.IsMasterClient) return;


        if (RankingBoard == null || RankingBoard.Count == 0)
            return;

        var sortedRankingBoard = RankingBoard.OrderByDescending(num => num.Value); //벨류값으로 내림차순


        string[] rankStr = { "", "", "", "", "", "" };

        for(int i=0; i< rankStr.Length;i++)
        {
            rankStr[i] = "";
        }

        int count = 0;

        foreach (var rank in sortedRankingBoard)
        {
            rankStr[count++] = rank.Key +" "+ (int)rank.Value;
        }

        
        PV.RPC("updateRankingTextRPC", RpcTarget.AllBuffered, rankStr,count);
    }


    [PunRPC]
    private void updateRankingTextRPC(string[] rankStr,int length)
    {
        string myNickname = PhotonNetwork.NickName;
        int count = -1;
        int myRank = 999;
        string myStr = "";
        int lastRnak = rankStr.Length;

        for (int i = 0; i < RankObjs.Length; i++)
        {
            RankObjs[i].gameObject.SetActive(false);
        }

        foreach (string str in rankStr)
        {
            count++;
            if (count == length)
                break;

            string[] parts = str.Split(' ');
            string nickName = parts[0];
            string point = parts[1];

            if (count <= 2)
            {
                RankImages[count].sprite = otherRankBackGroundSprite;
                RankTexts[count].text = str;
                RankObjs[count].SetActive(true);
            }

            if (nickName == myNickname)
            {
                myRank = count;
                myStr = str;

                if (2 <= count)
                    break;
            }
        }

        if (myRank <= 2)
        {
            RankImages[myRank].sprite = myRankBackGroundSprite;
        }
        else
        {
            RankObjs[RankObjs.Length - 1].SetActive(true);
            RankImages[RankImages.Length - 1].sprite = myRankBackGroundSprite;
            RankTexts[RankTexts.Length - 1].text = myStr;
        }


    }

    public void ReportTheKill(string killer, string deadPerson)
    {
        PV.RPC("killWriteRPC", RpcTarget.AllBuffered, killer, deadPerson); //마스터가 rank업데이트해야함
    }

    [PunRPC]
    private void killWriteRPC(string killer, string deadPerson)
    {
        RankingBoard[deadPerson] = 0f;
        UpdateRankingBoard();

        if (PhotonNetwork.IsMasterClient)
        {
            deadPersonSet.Add(deadPerson);
            killLogQueue.Enqueue(new KeyValuePair<string, string>(killer, deadPerson));
            killLogOnTheScreen();
        }
    }

    public void ReportTheMakeLand(string nickName, float addArea)
    {
        PV.RPC("LadnWrite_RPC", RpcTarget.AllBuffered, nickName, addArea); //마스터가 rank업데이트해야함
    }

    [PunRPC]
    private void LadnWrite_RPC(string nickName, float addArea)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            RankingBoard[nickName] += addArea;
            UpdateRankingBoard();
        }
    }


    private void killLogOnTheScreen()
    {
        if (killLogQueue.Count() == 0 || ScreenText.text.Length != 0)
            return;
        KeyValuePair<string, string> killLogInfo = killLogQueue.Dequeue();
        PV.RPC("killLogOnTheScreenRPC", RpcTarget.AllBuffered, killLogInfo.Key, killLogInfo.Value);
        StartCoroutine(EraseScreenText(3f));
    }

    IEnumerator EraseScreenText(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        PV.RPC("SetScreenTextRPC", RpcTarget.AllBuffered,"",100);

        if (killLogQueue.Count > 0) //대기하는 애 있으면 출력
        {
            killLogOnTheScreen();
        }
    }

    [PunRPC]
    private void killLogOnTheScreenRPC(string killer, string deadPerson)
    {
        ScreenText.text = killer + "님이 " + deadPerson + "님을 처치했습니다";
    }

    [PunRPC]
    private void SetScreenTextRPC(string str, int fontSize)
    {
        ScreenText.fontSize = fontSize;
        ScreenText.text = str;
    }

    bool _isGameEnd = false;

    private void OnEndGame()
    {
        if (_isGameEnd) return;
        _isGameEnd = true;

        if (PhotonNetwork.IsMasterClient)
        {
            GameStateManager.Instance.ChangeGameStateForAllUser(GameStateManager.GameState.Result);

            //마스터는 마지막으로 rankingboard 초기화
            for (int i = 0; i < RankingBoard.Count; i++)
            {
                RankingBoard[RankingBoard.Keys.ToList()[i]] = 0;
            }
            UpdateRankingBoard();
        }
    }



    ///timer
    ///
    [SerializeField] Text timerText;
    float timer = 0f;
    bool _onTimer = false;
    public void SetTimer(float time_)
    {
        if(time_<=0f)
        {
            time_ = 0f;
        }
        timer = time_;
        timerText.text = ((int)timer).ToString();

        if(timer<=0f)
        {
            OnEndGame();
        }
    }

    public void ActiveTimer()
    {
        SetTimer(15f);
        timerText.gameObject.SetActive(true);
    }

    public void StartTimer()
    {
        _onTimer = true;
    }

    public void EndTimer()
    {
        timerText.gameObject.SetActive(false);
        _onTimer = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (_onTimer && PhotonNetwork.IsMasterClient)
        {
            timer -= Time.deltaTime;
        }
    }

}