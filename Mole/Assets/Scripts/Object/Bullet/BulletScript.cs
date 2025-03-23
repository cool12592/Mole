using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class BulletScript : MonoBehaviour
{
    public PhotonView PV;
    private string ownerName; //오브젝트풀써야되서만듬

    [SerializeField]
    private float bullet_speed = 12f;

    public void naming(string name)
    {
        ownerName = name;
    }

    //활성화 될 때
    private void OnEnable()
    {
        ownerName = "";
        Invoke("inactive", 2f); //시작때는 그냥 각자 자기가 처리해주면됨(rpc안하고) //밑에상호작용때는 rpc로 삭제필요
    }

    private void Update()
    {
        transform.Translate(Vector3.right * bullet_speed * Time.deltaTime);
    }

    private void inactive()
    {
        ObjectPool.ReturnObject(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D col) // col을 RPC의 매개변수로 넘겨줄 수 없다
    {
        if (col.tag == "Ground") 
            PV.RPC("ReturnObjectRPC", RpcTarget.All);

        //총알이 초기화상태아니고 내께 아니고 충돌대상은 player고 걔가 내세상쪽애면
        //맞는쪽입장? (느린쪽이란게 이거 동기화되야하니까 억울함방지)
        if (ownerName!="" && PhotonNetwork.NickName != ownerName && col.tag == "Player" && col.GetComponent<PhotonView>().IsMine) // 느린쪽에 맞춰서 Hit판정
        {
            col.GetComponent<PlayerHealth>().TakeDamage(ownerName);
            PV.RPC("ReturnObjectRPC", RpcTarget.All);
        }
    }

    [PunRPC]
    private void ReturnObjectRPC()
    {
        ObjectPool.ReturnObject(gameObject);
    }
}
