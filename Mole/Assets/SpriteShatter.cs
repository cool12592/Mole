using System;
using UnityEngine;

public class SpriteShatter : MonoBehaviour
{




    public int rows = 3; // 가로 방향 조각 개수
    public int cols = 3; // 세로 방향 조각 개수
    public float explosionForce = 5f; // 조각들이 튀는 힘
    public float spread = 1f; // 조각들이 퍼지는 정도

    Sprite sprite;

    public void Init(Sprite sprite_)
    {
        sprite = sprite_;
        Shatter();
    }

    public void Shatter()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;

        Texture2D tex = sr.sprite.texture;
        Rect spriteRect = sr.sprite.rect;

        float pieceWidth = spriteRect.width / cols;
        float pieceHeight = spriteRect.height / rows;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                CreatePiece(sr, tex, spriteRect, x, y, pieceWidth, pieceHeight);
            }
        }

        // 원래 스프라이트 숨기기
        sr.enabled = false;
    }

    void CreatePiece(SpriteRenderer original, Texture2D texture, Rect spriteRect, int x, int y, float width, float height)
    {
        GameObject piece = new GameObject("Piece_" + x + "_" + y);
        piece.transform.position = transform.position;
        piece.transform.localScale = new Vector3(0.17f,0.17f,0.17f);

        SpriteRenderer sr = piece.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 2;
        sr.sprite = sprite;
        sr.color = new Color(0.4431373f,0.282353f, 0.2156863f);

        // 조각에 Rigidbody2D 추가해서 떨어지게 만들기
        Rigidbody2D rb = piece.AddComponent<Rigidbody2D>();
        rb.gravityScale = 1f;
        rb.AddForce(new Vector2(UnityEngine.Random.Range(-spread, spread), UnityEngine.Random.Range(0, spread)) * explosionForce, ForceMode2D.Impulse);

        Destroy(piece, 2f); // 2초 후 자동 삭제z
    }

}
