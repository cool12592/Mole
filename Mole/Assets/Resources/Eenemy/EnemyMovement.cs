using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    float moveSpeed = 5f;

    private float timer;
    private bool isMoving = true;

    playerScript player;
    [SerializeField] LayerMask obstacleLayer; // 벽 레이어
    [SerializeField] LayerMask roadLayer;
    [SerializeField] LayerMask playerLayer;
    MeshGenerator meshGenerator;
    PlayerMovement playerMovement;
    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        meshGenerator = GetComponent<MeshGenerator>();
        player =GetComponent<playerScript>();
        ChooseNextState();
    }

    void Update()
    {
        if(player.isActive==false || GameStateManager.Instance.NowGameState != GameStateManager.GameState.Fight)
            return;
        timer -= Time.deltaTime;

    
        if(DetectRoad())
        {

        }
        else if(DetectPlayer())
        {

        }
        else if(IsObstacleAvoid())
        {

        }
        transform.position += transform.up * moveSpeed * Time.deltaTime;
        playerMovement.ChangeAnim();

        if (timer <= 0f)
        {
            ChooseNextState();
        }
    }

    void ChooseNextState()
    {
        timer = Random.Range(1f,3f);

       
        float angle = Random.Range(0f, 360f); // 회전 각도 랜덤
        transform.rotation = Quaternion.Euler(0f, 0f, angle); // Z축만 회전
        
    }

    bool IsObstacleAvoid()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, 4f, obstacleLayer);
        if(hit)
        {
            transform.up = Vector2.Reflect(transform.up, hit.normal).normalized;
            return true;
        }
        return false;
    }

    float scanRadius = 7f;
    [SerializeField] Collider2D[] results;

    bool DetectRoad()
    {
        int hitCount = Physics2D.OverlapCircleNonAlloc(
            transform.position,
            scanRadius,
            results,
            roadLayer
        );

        for (int i = 0; i < hitCount && i< results.Length; i++)
        {
            Collider2D col = results[i];
            if(col.gameObject.TryGetComponent<Road>(out Road road))
            {
                if(road._myOwner != meshGenerator)
                {
                    var dir = (col.gameObject.transform.position - transform.position).normalized;
                    dir.z = 0f;
                    transform.up = dir;
                    return true;
                }
            }
        }

        return false;
    }


    float scanRadius2 = 7f;
    [SerializeField] Collider2D[] results2;

    bool DetectPlayer()
    {
        int hitCount = Physics2D.OverlapCircleNonAlloc(
            transform.position,
            scanRadius2,
            results2,
            playerLayer
        );

        for (int i = 0; i < hitCount && i< results2.Length; i++)
        {
            Collider2D col = results2[i];
            if(col.gameObject.TryGetComponent<playerScript>(out playerScript player_))
            {
                if(player_ != player)
                {
                    var dir = (player_.gameObject.transform.position - transform.position).normalized;
                    dir.z = 0f;
                    transform.up = -dir;
                    return true;
                }
            }
        }

        return false;
    }
}
