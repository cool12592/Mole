using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerPistol : MonoBehaviourPunCallbacks
{
    public PhotonView PV;
    private GameObject aimObject;
    [SerializeField]
    private GameObject gunObject;
    [SerializeField]
    private SpriteRenderer spriteRender;
    [SerializeField]
    private SpriteRenderer gunSpriteRender;

    private readonly float aimSpeed = 100f;

    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();
          
        if (PV.IsMine)
        {
            aimObject = gameObject.transform.Find("aim").gameObject;
            aimObject.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (PV.IsMine)
        {
            GunLookingAtTheAim();
        }
    }

    private void GunLookingAtTheAim()
    {
        Vector2 direction = new Vector2(
                gunObject.transform.position.x - aimObject.transform.position.x,
                gunObject.transform.position.y - aimObject.transform.position.y
            );

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Quaternion angleAxis = Quaternion.AngleAxis(angle - 180.0f, Vector3.forward);
        Quaternion rotation = Quaternion.Slerp(gunObject.transform.rotation, angleAxis, 20.0f * Time.deltaTime);

        PV.RPC("GunFlipRPC", RpcTarget.All, direction.x);
        PV.RPC("GunLookingAtRPC", RpcTarget.All, rotation);
    }

    [PunRPC]
    private void GunLookingAtRPC(Quaternion rotation)
    {
        gunObject.transform.rotation = rotation;
    }

    [PunRPC]
    private void GunFlipRPC(float x)
    {
        gunSpriteRender.flipY = x > 0f; 
        spriteRender.flipX = gunSpriteRender.flipY; 
    }

    public void AimMove(Vector2 inputDirection)
    {
        aimObject.transform.position = Vector3.Lerp(aimObject.transform.position, transform.position + new Vector3(inputDirection.x * 4f, inputDirection.y * 4f, 0f), Time.deltaTime * aimSpeed);
    }
}
