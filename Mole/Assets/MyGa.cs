using System.Collections;
using System.Collections.Generic;
using GameAnalyticsSDK;
using UnityEngine;

public class MyGa : MonoBehaviour
{
    private static MyGa instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 파괴되지 않음
        }
        else
        {
            Destroy(gameObject); // 중복된 경우 삭제 (씬 재로드로 인해 새로 생성된 경우)
        }
    }

    void Start()
    {
        GameAnalytics.Initialize();
    }
}
