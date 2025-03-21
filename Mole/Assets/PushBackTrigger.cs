using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushBackTrigger : MonoBehaviour
{
    public float pushForce = 5f;

    private void OnTriggerStay2D(Collider2D other)
    {
        // Rigidbody가 있는 객체만 밀기
        Rigidbody2D rb = other.attachedRigidbody;
        if (rb != null)
        {
            // 이 오브젝트의 위치 (Trigger 중심) 기준으로 방향 계산
            Vector2 direction = (rb.position - (Vector2)transform.position).normalized;

            // 같은 거리로 계속 밀어내기
            rb.velocity = direction * pushForce;
        }
    }
}
