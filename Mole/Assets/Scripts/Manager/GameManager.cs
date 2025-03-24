using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Linq;
using System;
public class GameManager : MonoBehaviour
{
    public PhotonView PV;
    private static GameManager instance = null;
    private Text ScreenText;
   // public Text RangkingLogText { get; private set; }
    private Queue<KeyValuePair<string, string>> killLogQueue = new Queue<KeyValuePair<string, string>>();
    private Dictionary<string, float> RankingBoard = new Dictionary<string, float>();
    public Dictionary<string, MeshGenerator> UserMeshMap = new Dictionary<string, MeshGenerator>();

    public GameObject myplayer, ResponePanel;

    IEnumerator rankingBoardCoroutine;
    WaitForSeconds waitForSecnds = new WaitForSeconds(1f);

    int AllMemberCount = 0;
    Dictionary<string,int> deadPersonDict = new Dictionary<string,int>(); 

    public string[] UserNames;

    [SerializeField] GameObject[] resultPanels;
    public enum ResultPanel {SingleVictory,SingleDefeat,MultiResult,MultiDefeat}


    public void DeactiveMultiDefeatPanel()
    {
        resultPanels[(int)ResultPanel.MultiDefeat].SetActive(false);
    }
    public void DeactiveResultPanel(ResultPanel resultPanel)
    {
        resultPanels[(int)resultPanel].SetActive(false);
    }


    IEnumerator CoWaitRequest()
    {
        yield return new WaitForSeconds(1f); // 안전빵
        PV.RPC("SetScreenTextRPC", RpcTarget.All, "", 50);

        ActiveMultiResultPanel();
    }

    public void ActiveResultPanel(ResultPanel resultPanel)
    {
        if(resultPanel == ResultPanel.MultiResult)
        {
            StartCoroutine(CoWaitRequest());
            return;
        }

        resultPanels[(int)resultPanel].SetActive(true);
    }

    public void ActiveMultiResultPanel()
    {
        int ind = 0;
        foreach (var obj in ResultObjs)
        {
            ResultImages[ind++].sprite = otherRankBackGroundSprite;
            obj.SetActive(false);
        }

        List<string> sortedKeys = RankingBoard
            .OrderByDescending(pair => pair.Value) // value로 내림차순 정렬
            .Select(pair => pair.Key)              // key만 추출
            .ToList();

        int count = 0;
        foreach (var nick in sortedKeys)
        {
            if (RankingBoard[nick] == 0)
                break;
            ResultObjs[count].SetActive(true);
            ResultTexts[count].text = nick.Substring(2);
            ResultTexts[count].text += " " + ((int)RankingBoard[nick]).ToString();

            if (nick == PhotonNetwork.LocalPlayer.NickName)
            {
                ResultImages[count].sprite = myRankBackGroundSprite;
            }

            count++;
        }

        List<string> sortedKeys2 = deadPersonDict
            .OrderByDescending(pair => pair.Value) // value로 내림차순 정렬
            .Select(pair => pair.Key)              // key만 추출
            .ToList();

        for (int i = count; i < count + sortedKeys2.Count; i++)
        {
            int sortedKeyIndex = i - count;
            ResultObjs[i].SetActive(true);
            ResultTexts[i].text = sortedKeys2[sortedKeyIndex].Substring(2);
            ResultTexts[i].text += " (Die)";

            if (sortedKeys2[sortedKeyIndex] == PhotonNetwork.LocalPlayer.NickName)
            {
                ResultImages[i].sprite = myRankBackGroundSprite;
            }
        }

        resultPanels[(int)ResultPanel.MultiResult].SetActive(true);
    }


    public void StartLobby()
    {
        ActiveTimer();

        if (PhotonNetwork.IsMasterClient)
        {
            timerUpButton.SetActive(true);
            timerDownButton.SetActive(true);

            //마스터는 마지막으로 rankingboard 초기화
            for (int i = 0; i < RankingBoard.Count; i++)
            {
                RankingBoard[RankingBoard.Keys.ToList()[i]] = 0;
            }
            UpdateRankingBoard();
        }
    }
    public void StartGame()
    {
        deadPersonDict.Clear();
        _isGameEnd = false;
        AllMemberCount = RankingBoard.Count;

        if (PhotonNetwork.IsMasterClient)
        {
            timerUpButton.SetActive(false);
            timerDownButton.SetActive(false);
        }
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
            SynchRankingBoard();
            yield return waitForSecnds;
        }
    }

    private void SynchTimer()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PV.RPC("SynchTimer_RPC", RpcTarget.Others, timer);
            SetTimer(timer);
        }
    }

    [PunRPC]
    private void SynchTimer_RPC(float timer_)
    {
        SetTimer(timer_);
    }

    private void SynchRankingBoard()
    {
        if (PhotonNetwork.IsMasterClient)
            PV.RPC("SynchRankingBoardRPC", RpcTarget.Others, RankingBoard); //방장떠날때 대비 이것도 동기화해줘야됨
    }

    [PunRPC]
    private void SynchRankingBoardRPC(Dictionary<string, float> rankingBoard)
    {
        RankingBoard = rankingBoard;
    }


    private void SynchUserNames()
    {
        string[] userNames_ = { "", "", "", "", "", "" };
        for(int i=0;i< UserNames.Length; i++)
        {
            userNames_[i] = UserNames[i];
        }
        PV.RPC("SynchUserNames_RPC", RpcTarget.AllBuffered, UserNames);

    }

    [PunRPC]
    private void SynchUserNames_RPC(string[] userNames_)
    {
        for (int i = 0; i < userNames_.Length; i++)
        {
            UserNames[i] = userNames_[i];
        }
    }



    public void UserJoin(string nickName)
    {
        if (PhotonNetwork.IsMasterClient)
        {
           if(RankingBoard.ContainsKey(nickName) == false) 
                RankingBoard.Add(nickName, 0);
            UpdateRankingBoard();


            UserNames[FindEmptyNameIndex()] = nickName;
            SynchUserNames();
            if (PhotonNetwork.LocalPlayer.NickName != nickName) //방장입장에서 나 아닌 다른 유저면
                InitGameState(nickName);
        }
    }

    public int FindMyNameIndex(string nick)
    {
        for (int i = 0; i < UserNames.Length; i++)
        {
            if (UserNames[i] == nick)
                return i;
        }

        return -1;
    }

    void DeleteUserName(string nick)
    {
        for (int i = 0; i < UserNames.Length; i++)
        {
            if (UserNames[i] == nick)
                UserNames[i] = "";
        }
    }
    int FindEmptyNameIndex()
    {
        HashSet<string> names = new HashSet<string>();
        foreach(var player in PhotonNetwork.PlayerList)
        {
            names.Add(player.NickName);
        }

        for(int i=0;i<UserNames.Length;i++)
        {
            if(names.Contains(UserNames[i]) == false)
            {
                UserNames[i] = "";
            }
        }

        for (int i = 0; i < UserNames.Length; i++)
        {
            if (UserNames[i] == "")
                return i;
        }

        return 0;
    }

    private void InitGameState(string nickName)
    {
        if (PhotonNetwork.IsMasterClient)
            PV.RPC("InitGameStateRPC", RpcTarget.Others, nickName, (int)GameStateManager.Instance.NowGameState);
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
            DeleteUserName(nickName);
            SynchUserNames();

            WriteDeadPeson(nickName);
            if (RankingBoard.ContainsKey(nickName))
                RankingBoard.Remove(nickName);
            UpdateRankingBoard();
        }
    }

    void WriteDeadPeson(string deadNickName)
    {
        if (PhotonNetwork.IsMasterClient == false)
            return;

        if(deadPersonDict.ContainsKey(deadNickName))
        {
            return;
        }
        deadPersonDict[deadNickName] = deadPersonDict.Count;


        PV.RPC("SynchDeadPerson_RPC", RpcTarget.Others, deadPersonDict);

        if (AllMemberCount == deadPersonDict.Count + 1)
        {
            OnEndGame();
        }
    }

    [PunRPC]
    void SynchDeadPerson_RPC(Dictionary<string,int> deadPersonDict_)
    {
        deadPersonDict = deadPersonDict_;
    }

    [SerializeField] Sprite otherRankBackGroundSprite;
    [SerializeField] Sprite myRankBackGroundSprite;
    [SerializeField] Text[] RankInfoTexts;
    [SerializeField] Image[] RankImages;
    [SerializeField] GameObject[] RankObjs;
    [SerializeField] Text[] RankNumberTexts;

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

        
        PV.RPC("updateRankingTextRPC", RpcTarget.All, rankStr,count);
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
                RankInfoTexts[count].text = str.Substring(2);
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
            RankInfoTexts[RankInfoTexts.Length - 1].text = myStr.Substring(2);

            RankNumberTexts[RankNumberTexts.Length - 1].text = myRank.ToString() + "st";
        }


    }

    public void ReportTheKill(string killer, string deadPerson)
    {
        PV.RPC("killWriteRPC", RpcTarget.All, killer, deadPerson);
    }

    [PunRPC]
    private void killWriteRPC(string killer, string deadPerson)
    {
        RankingBoard[deadPerson] = 0f;
        UpdateRankingBoard();

        if (PhotonNetwork.IsMasterClient)
        {
            WriteDeadPeson(deadPerson);
            killLogQueue.Enqueue(new KeyValuePair<string, string>(killer, deadPerson));
            killLogOnTheScreen();
        }
    }

    public void ReportTheMakeLand(string nickName, float addArea)
    {
        PV.RPC("LadnWrite_RPC", RpcTarget.MasterClient, nickName, addArea); //마스터가 rank업데이트해야함
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
        PV.RPC("killLogOnTheScreenRPC", RpcTarget.All, killLogInfo.Key, killLogInfo.Value);
        StartCoroutine(EraseScreenText(2f));
    }

    IEnumerator EraseScreenText(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        PV.RPC("SetScreenTextRPC", RpcTarget.All,"",50);

        if (killLogQueue.Count > 0) //대기하는 애 있으면 출력
        {
            killLogOnTheScreen();
        }
    }

    [PunRPC]
    private void killLogOnTheScreenRPC(string killer, string deadPerson)
    {
        ScreenText.text = killer.Substring(2) + " Killed " + deadPerson.Substring(2);
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
        if (PhotonNetwork.IsMasterClient == false)
            return;
        
        if (_isGameEnd) 
            return;
        _isGameEnd = true;

        GameStateManager.Instance.ChangeGameStateForAllUser(GameStateManager.GameState.Result);
    }



    ///timer
    ///
    [SerializeField] Text timerText;
    [SerializeField] GameObject timerUpButton;
    [SerializeField] GameObject timerDownButton;

    float timer = 0f;
    bool _onTimer = false;
    public void SetTimer(float time_)
    {
        if(time_<=0f)
        {
            time_ = 0f;
        }
        timer = time_;
        timerText.text = timer.ToString("F2");

        if (timer<=0f)
        {
            OnEndGame();
        }
    }

    public void ActiveTimer()
    {
        SetTimer(90);
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

            SynchTimer();
        }
    }



    [SerializeField] Text[] ResultTexts;
    [SerializeField] GameObject[] ResultObjs;
    [SerializeField] Image[] ResultImages;



    [SerializeField] Transform FadeOutMaskObj;
    bool _isStarting = false;

    [SerializeField] GameObject screenTouch;

    public void StartShrinkScaleCoroutine(Vector3 targetScale, Action onComplete)
    {
        if (_isStarting)
            return;
        _isStarting = true;
        StartCoroutine(ShrinkScaleCoroutine(targetScale, onComplete));
    }

    IEnumerator ShrinkScaleCoroutine(Vector3 targetScale, Action onComplete)
    {
        screenTouch.SetActive(false);

        float duration = 0.5f;
        Vector3 startScale = FadeOutMaskObj.localScale;
        float time = 0f;

        while (time < duration)
        {
            float t = time / duration;
            FadeOutMaskObj.localScale = Vector3.Lerp(startScale, targetScale, t);
            time += Time.deltaTime;
            yield return null;
        }

        FadeOutMaskObj.localScale = targetScale;
        screenTouch.SetActive(true);
        _isStarting = false;

        onComplete?.Invoke();

    }


    public void OnClickUpTimer()
    {
        if(timer <=170)
        {
            timer += 10;
            SynchTimer();
        }
    }
    public void OnClickDownTimer()
    {
        if (20 <= timer)
        {
            timer -= 10;
            SynchTimer();
        }
    }

    public Transform[] startingPositions;

}