using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingGround : MonoBehaviour
{
    [SerializeField] float fallSpeed = 2.0f;  // 떨어지는 속도
    [SerializeField] float targetY = -100f;  // 목표 Y 위치
    [SerializeField] float delay = 0.3f;
    private Vector3 startPos;
    private bool isFalling = false;
    private float lerpTime = 0f;

    [SerializeField] RenderTexture meshRender;
    [SerializeField] RenderTexture meshRenderCopy;

    private void Awake()
    {
        startPos = transform.position;
    }

    void Update()
    {
        if (isFalling)
        {
            lerpTime += Time.deltaTime * fallSpeed;
            transform.position = new Vector3(
                startPos.x,
                Mathf.Lerp(startPos.y, targetY, lerpTime),
                startPos.z
            );

            if (Mathf.Abs(transform.position.y - targetY) < 0.01f)
            {
                transform.position = new Vector3(startPos.x, targetY, startPos.z);
                isFalling = false;
                gameObject.SetActive(false);
            }
        }
    }

    public void StartFalling()
    {
        Graphics.Blit(meshRender, meshRenderCopy);

        gameObject.SetActive(true);
        transform.position = startPos; // 오브젝트를 순간이동
        lerpTime = 0f;
        StartCoroutine(StartFallAfterDelay());
    }

    private IEnumerator StartFallAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        isFalling = true;
    }
}
