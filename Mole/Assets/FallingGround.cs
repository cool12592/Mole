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

    [SerializeField] RenderTexture meshTexture;
    [SerializeField] RenderTexture meshTextureCopy;

    [SerializeField] RenderTexture _roadTexture;
    [SerializeField] RenderTexture _finishRoadTexture;

    [SerializeField] RenderTexture paintTexture;
    [SerializeField] Material blitMaterial;

    [SerializeField] SpriteShatter SpriteShatter;

    private void Awake()
    {
        startPos = transform.position;
        startPos = new Vector3(startPos.x, startPos.y, 5f);
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

    public void BlendRenderTextures()
    {
        blitMaterial.SetTexture("_MainTex", meshTexture);
        blitMaterial.SetTexture("_MaskTex1", paintTexture);
        blitMaterial.SetTexture("_MaskTex2", _roadTexture);
        blitMaterial.SetTexture("_MaskTex3", _finishRoadTexture);

        Graphics.Blit(meshTexture, meshTextureCopy, blitMaterial);
    }

    public void StartFalling()
    {
        gameObject.SetActive(true);
        transform.position = startPos; // 오브젝트를 순간이동
        lerpTime = 0f;
       // isFalling = true;

         StartCoroutine(StartFallAfterDelay());
    }

    private IEnumerator StartFallAfterDelay()
    {
        yield return new WaitForSeconds(0.2f);
        gameObject.SetActive(false);
    }

    //private IEnumerator StartFallAfterDelay()
    //{
    //    yield return new WaitForSeconds(delay);
    //    isFalling = true;
    //}
}
