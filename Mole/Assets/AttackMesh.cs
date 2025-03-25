using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class AttackMesh : MonoBehaviour
{
    string nickName = "";
    playerScript player;

    private IEnumerator Start()
    {
        yield return new WaitForFixedUpdate();
        Destroy(this);
    }

    public void Init(playerScript player_, string nickName_)
    {
        nickName = nickName_;
        player = player_;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (PhotonNetwork.IsMasterClient == false)
            return;

        if (player == null)
            return;

        if (other.gameObject.layer != LayerMask.NameToLayer("Player"))
            return;

        if (nickName == "")
            return;


        if(other.transform.parent.TryGetComponent<PlayerHealth>(out PlayerHealth playerHealth))
        {
            if (playerHealth.PV == null)
                return;
            if (nickName == playerHealth.PV.Owner.NickName)
                return;


            playerHealth.Death(player, nickName,MeshGenerator.GenerateMeshType.TakeGround);
        }
    }
}
