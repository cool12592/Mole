using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    float moveSpeed = 6f;

    private float timer;
    private bool isMoving;

    playerScript player;
    [SerializeField] LayerMask obstacleLayer; // 벽 레이어

    void Start()
    {
        player =GetComponent<playerScript>();
        ChooseNextState();
    }

    void Update()
    {
        if(player.isActive==false || GameStateManager.Instance.NowGameState != GameStateManager.GameState.Fight)
            return;
        timer -= Time.deltaTime;

        if (isMoving)
        {
            IsObstacleAvoid();
            transform.position += transform.up * moveSpeed * Time.deltaTime;
        }

        if (timer <= 0f)
        {
            ChooseNextState();
        }
    }

    void ChooseNextState()
    {
        isMoving = Random.value > 0.2f; // 70% 확률로 움직임
        timer = Random.Range(1f,3f);

        if (isMoving)
        {
            float angle = Random.Range(0f, 360f); // 회전 각도 랜덤
            transform.rotation = Quaternion.Euler(0f, 0f, angle); // Z축만 회전
        }
    }

    void IsObstacleAvoid()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, 4f, obstacleLayer);
        if(hit)
            transform.up = Vector2.Reflect(transform.up, hit.normal).normalized;
    }
}
