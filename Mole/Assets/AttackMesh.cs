using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackMesh : MonoBehaviour
{
    string nickName;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.05f);
        Destroy(this);
    }

    public void Init(string nickName_)
    {
        nickName = nickName_;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Player"))
            return;


        if(other.transform.parent.TryGetComponent<PlayerHealth>(out PlayerHealth playerHealth))
        {
            if (playerHealth.PV == null)
                return;
            if (nickName == playerHealth.PV.Owner.NickName)
                return;
            playerHealth.Death(nickName);
        }
    }
}
