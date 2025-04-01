using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float moveSpeed = 5f;
    float turnSpeedOutside = 100f; // 곡선 회전 속도
    private float timer;

    [Header("Status")]
    private bool wasInHouse = true;              // 이전 프레임에서의 상태
    public bool isHouse => meshGenerator.InHouse; // 외부에서 설정
    private bool isReturning = false;            // 영역 밖에서 다시 돌아오는 중인지

    [Header("Components")]
    private playerScript player;
    private MeshGenerator meshGenerator;
    private PlayerMovement playerMovement;

    [Header("Detection Settings")]
    [SerializeField] LayerMask obstacleLayer;
    [SerializeField] LayerMask roadLayer;
    [SerializeField] LayerMask playerLayer;

    float scanRadius = 3f;
    [SerializeField] float scanRadius2 = 3f;
    [SerializeField] Collider2D[] results;
    [SerializeField] Collider2D[] results2;
    Vector3 startingPoint;
    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        meshGenerator = GetComponent<MeshGenerator>();
        player = GetComponent<playerScript>();
        ChooseNextState();

        startingPoint = transform.position;
    }

    float rightRotation = 1f;

  

    void Update()
    {
        if (!player.isActive || GameStateManager.Instance.NowGameState != GameStateManager.GameState.Fight)
            return;


        timer -= Time.deltaTime;

        if(isHouse)
        {
            lastWasInHousePostion = transform.position;
        }

        
        
        // 상태 전환 체크
        if (isHouse == false && wasInHouse)
        {
            reapeatChecking = false;
            isReturning = true;

            float rightRotation = Random.Range(0, 1);
            if (0.5 < rightRotation)
            {
                rightRotation = 1f;
            }
            else
            {
                rightRotation = -1f;
            }

            turnSpeedOutside = Random.Range(90f * rightRotation, 130f * rightRotation);
            ChooseCurvedExitDirection();
        }

        // 행동 결정
      //  if (DetectRoad()) { }
        if (DetectPlayer()) { }
        else if (IsObstacleAvoid()) { }
        else
        {
            if (!isHouse && isReturning)
            {
                CurveOutwardAndReturn(); // 집 밖에서 곡선 궤적으로 복귀 중
            }
            // 집 안에서는 그냥 직진
        }
        

        // 이동 처리
        transform.position += transform.up * moveSpeed * Time.deltaTime;
        playerMovement.ChangeAnim();

        if ( timer <= 0f && isHouse)
        {
            ChooseNextState();
        }

        // 현재 위치 상태 업데이트
        wasInHouse = isHouse;
    }

    void ChooseNextState()
    {
        timer = Random.Range(1f, 3f);

        if (isHouse)
        {
            float angle = Random.Range(0f, 360f);
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    void ChooseCurvedExitDirection()
    {
        // 약간 바깥 방향으로 튀어나가는 회전
        float angle = Random.Range(-90f, 90f);
        transform.Rotate(0f, 0f, angle);
    }



    private Vector2 lastWasInHousePostion;
    private float returnThresholdSqr1 = 9f; // 얼마나 가까워야 돌아왔다고 볼지 (0.1^2 = 약 0.01 거리)
    private float returnThresholdSqr2 = 4f; // 얼마나 가까워야 돌아왔다고 볼지 (0.1^2 = 약 0.01 거리)

    bool reapeatChecking = false;
    void CurveOutwardAndReturn()
    {
        // 계속 회전하면서 곡선 이동
        transform.Rotate(0f, 0f, turnSpeedOutside * Time.deltaTime);

        Vector2 currentPosition = transform.position;
        float distanceSqr = Vector3.SqrMagnitude(currentPosition - lastWasInHousePostion);

        if (reapeatChecking == false)
        {
            if(returnThresholdSqr1 < distanceSqr)
            {
                reapeatChecking = true;
            }
        }
        else
        {
            if(distanceSqr <= returnThresholdSqr2)
            {
                isReturning = false;

                Vector3 dir = (startingPoint - transform.position).normalized;
                dir.z = 0f;
                transform.up = dir;
            }
        }

        if (isHouse)
        {
            isReturning = false;
        }
    }

    bool IsObstacleAvoid()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, 4f, obstacleLayer);
        if (hit)
        {
            // 영역 밖에서 곡선 이동 중이라면 벽에 닿아도 반사하지 않음
            if (!isHouse && isReturning)
            {
                // 방향만 살짝 틀어줌 (벽 타고 무한 도는 거 방지용)
                transform.Rotate(0f, 0f, turnSpeedOutside * Time.deltaTime * 2f);
                return true;
            }

            // 영역 안에서는 반사 처리
            transform.up = Vector2.Reflect(transform.up, hit.normal).normalized;
            return true;
        }

        return false;
    }

    Road targetRoad = null;
    bool DetectRoad()
    {
        int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, scanRadius, results, roadLayer);

        for (int i = 0; i < hitCount && i < results.Length; i++)
        {
            Collider2D col = results[i];
            if (col.gameObject.TryGetComponent<Road>(out Road road))
            {
                if (road._myOwner != meshGenerator)
                {
                    targetRoad = road;

                    Vector3 dir = (col.transform.position - transform.position).normalized;
                    dir.z = 0f;
                    transform.up = dir;
                    return true;
                }
            }
        }
        return false;
    }

    bool DetectPlayer()
    {
        int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, scanRadius2, results2, playerLayer);
        
        for (int i = 0; i < hitCount && i < results2.Length; i++)
        {
            Collider2D col = results2[i];
            if (col.gameObject.TryGetComponent<playerScript>(out playerScript player_))
            {
                if (player_ != GameManager.Instance.SinglePlayer)
                    continue;

                if (player_ != player)
                {
                    Vector3 dir = (player_.transform.position + Vector3.right*3f - transform.position).normalized;
                    dir.z = 0f;
                    transform.up = dir;
                    return true;
                }
            }
        }
        return false;
    }

    void ForcedDetectPlayer()
    {
        if (GameManager.Instance.SinglePlayer == null)
            return;
        if (Vector3.Distance(GameManager.Instance.SinglePlayer.transform.position, transform.position) < 2f)
            return;
        Vector3 dir = (GameManager.Instance.SinglePlayer.transform.position+ Vector3.right*4f +  - transform.position).normalized;
        dir.z = 0f;
        transform.up = dir;
    }

    //void aaa()
    //{
    //    if (!player.isActive || GameStateManager.Instance.NowGameState != GameStateManager.GameState.Fight)
    //        return;

    //    timer -= Time.deltaTime;

    //    if (isHouse)
    //    {
    //        lastWasInHousePostion = transform.position;
    //    }

    //    // 상태 전환 체크
    //    if (isHouse == false && wasInHouse)
    //    {
    //        reapeatChecking = false;
    //        isReturning = true;

    //        float rightRotation = Random.Range(0, 1);
    //        if (0.5 < rightRotation)
    //        {
    //            rightRotation = 1f;
    //        }
    //        else
    //        {
    //            rightRotation = -1f;
    //        }

    //        turnSpeedOutside = Random.Range(90f * rightRotation, 130f * rightRotation);
    //        ChooseCurvedExitDirection();
    //    }

    //    // 행동 결정
    //    if (DetectRoad()) { }
    //    else if (DetectPlayer()) { }
    //    else if (IsObstacleAvoid()) { }
    //    else
    //    {
    //        if (!isHouse && isReturning)
    //        {
    //            CurveOutwardAndReturn(); // 집 밖에서 곡선 궤적으로 복귀 중
    //        }
    //        // 집 안에서는 그냥 직진
    //    }

    //    // 이동 처리
    //    transform.position += transform.up * moveSpeed * Time.deltaTime;
    //    playerMovement.ChangeAnim();

    //    if (timer <= 0f && isHouse)
    //    {
    //        ChooseNextState();
    //    }

    //    // 현재 위치 상태 업데이트
    //    wasInHouse = isHouse;
    //}
}