using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    float moveSpeed = 5f;
    public float moveTime = 2f;
    public float restTime = 0.5f;

    private float timer;
    private bool isMoving;

    playerScript player;

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
        timer = isMoving ? moveTime : restTime;

        if (isMoving)
        {
            float angle = Random.Range(0f, 360f); // 회전 각도 랜덤
            transform.rotation = Quaternion.Euler(0f, 0f, angle); // Z축만 회전
        }
    }
}
