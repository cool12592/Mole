using System;
using UnityEngine;

public class SpriteShatter : MonoBehaviour
{




     int rows = 2; // 가로 방향 조각 개수
     int cols = 2; // 세로 방향 조각 개수
     float explosionForce = 4f; // 조각들이 튀는 힘
     float spread = 1f; // 조각들이 퍼지는 정도
    bool isDrill;
    [SerializeField] SpriteRenderer sr;
    Sprite sprite;
    Vector3 playerUpVector;
    public void Init(Sprite sprite_, Vector3 playerUpVector_, bool isDrill_ = false)
    {
        sprite = sprite_;
        playerUpVector = playerUpVector_;
        isDrill = isDrill_;
        Shatter();
    }

    public void Shatter()
    {
        if (sr == null || sr.sprite == null) return;

        if (isDrill)
        {
            rows *= 2;
            cols *= 2;
            explosionForce = 5f;
        }
        
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
        //sr.enabled = false;
    }

    void CreatePiece(SpriteRenderer original, Texture2D texture, Rect spriteRect, int x, int y, float width, float height)
    {
        float dirllPos = 1f;
        if (isDrill)
            dirllPos = 4f;

        float drillSize = 1f;
        if (isDrill)
            drillSize = 2f;

        var pos = transform.position + playerUpVector * dirllPos;;
        pos.z = -1f;
        SpritePiece piece = GlobalSpritePool.Instance.GetPiece(pos);
        piece.transform.localScale = new Vector3(0.17f,0.17f,0.17f) * drillSize;;

        piece.spriteRenderer.sortingOrder = 2;
        piece.spriteRenderer.sprite = sprite;
        piece.spriteRenderer.color = new Color(0.4431373f,0.282353f, 0.2156863f);

        // 조각에 Rigidbody2D 추가해서 떨어지게 만들기
        piece.rigid.gravityScale = 1f;
        piece.rigid.AddForce(new Vector2(UnityEngine.Random.Range(-spread, spread), UnityEngine.Random.Range(0, spread)) * explosionForce, ForceMode2D.Impulse);

        GlobalSpritePool.Instance.Release(piece, 1f); // 2초 후 자동 삭제z
    }

}




/*
using System;
using UnityEngine;

public class SpriteShatter : MonoBehaviour
{




     int rows = 2; // 가로 방향 조각 개수
     int cols = 2; // 세로 방향 조각 개수
     float explosionForce = 4f; // 조각들이 튀는 힘
     float spread = 1f; // 조각들이 퍼지는 정도

    [SerializeField] SpriteRenderer sr;
    Sprite sprite;
    Vector3 playerUpVector;

    bool isDrill = false;
    public void Init(Sprite sprite_, Vector3 playerUpVector_, bool isDrill_ = false)
    {
        sprite = sprite_;
        playerUpVector = playerUpVector_;
        isDrill = isDrill_;
        Shatter();
    }

    bool isBlock = false;
    public void BlockInit(Sprite sprite_)
    {
        isBlock = true;
        rows = 2;
        cols = 2;

        sprite = sprite_;
        playerUpVector = Vector3.zero;

        Shatter();
    }

    public void Shatter()
    {
        if (sr == null || sr.sprite == null) return;

        if (isDrill)
        {
            rows *= 2;
            cols *= 2;
            explosionForce = 5f;
        }

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
        //sr.enabled = false;
    }

    void CreatePiece(SpriteRenderer original, Texture2D texture, Rect spriteRect, int x, int y, float width, float height)
    {
        float dirllPos = 1f;
        if (isDrill)
            dirllPos = 4f;

        float drillSize = 1f;
        if (isDrill)
            drillSize = 2f;

        var pos = transform.position + playerUpVector* dirllPos;
        pos.z = -1f;
        SpritePiece piece = GlobalSpritePool.Instance.GetPiece(pos);

        piece.transform.localScale = GlobalSpritePool.Instance.pieceSize * Vector3.one * drillSize;


        piece.spriteRenderer.sortingOrder = 2;
        piece.spriteRenderer.sprite = sprite;
        piece.spriteRenderer.color = GlobalSpritePool.Instance.pieceColor;

        if(isBlock)
        {
            piece.spriteRenderer.color = Color.white;
        }
        // 조각에 Rigidbody2D 추가해서 떨어지게 만들기
        piece.rigid.gravityScale = 1f;
        piece.rigid.AddForce(new Vector2(UnityEngine.Random.Range(-spread, spread), UnityEngine.Random.Range(0, spread)) * explosionForce, ForceMode2D.Impulse);

        GlobalSpritePool.Instance.Release(piece, 1f); // 2초 후 자동 삭제z
    }

}
*/
