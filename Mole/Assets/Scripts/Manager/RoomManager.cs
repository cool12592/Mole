using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
public class RoomManager : MonoBehaviourPunCallbacks
{
    //public static RoomManager Instance { get; private set; }

  //  private const string FLOAT_KEY = "SharedFloat"; // 🔴 방에서 공유할 float 키
    //private float sharedFloat = 0f; // 🔴 공유할 float 값 (초기값 100)

    //private void Awake()
    //{
    //    // 🔴 싱글톤 패턴 구현
    //    if (Instance == null)
    //    {
    //        Instance = this;
    //        DontDestroyOnLoad(gameObject); // 씬 변경 시에도 유지
    //    }
    //    else
    //    {
    //        Destroy(gameObject); // 중복 방지
    //    }
    //}

    //public float GetSharedFloat()
    //{
    //    photonView.RPC("RPC_DecreaseSharedFloat", RpcTarget.AllBuffered); // 🔴 값 감소 요청
    //    return sharedFloat; // 🔴 로컬 값 반환 (즉시 반영)
    //}

    //[PunRPC]
    //void RPC_DecreaseSharedFloat()
    //{
    //    sharedFloat -= 0.001f; // 🔴 모든 클라이언트에서 sharedFloat 값을 감소
    //}

}
