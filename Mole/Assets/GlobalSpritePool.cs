using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalSpritePool : MonoBehaviour
{
    public static GlobalSpritePool Instance { get; private set; }

    [SerializeField] private SpritePiece spritePrefab;
    private int initialPoolSize = 100;

    private Queue<SpritePiece> pool = new Queue<SpritePiece>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitPool();
    }

    private void InitPool()
    {
        CreateNewObject(initialPoolSize);
    }

    private void CreateNewObject(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var obj = Instantiate(spritePrefab);
            obj.transform.SetParent(transform);
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public SpritePiece GetPiece(Vector3 position, Transform parent = null)
    {
        SpritePiece obj;

        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            CreateNewObject(100);
            obj = pool.Dequeue();
        }

        obj.transform.position = position;
        obj.transform.rotation = Quaternion.identity;
        obj.transform.SetParent(parent);
        obj.gameObject.SetActive(true);

        return obj;
    }

    public void Release(SpritePiece obj, float time)
    {
        StartCoroutine(CoRelease(obj, time));
    }

    public IEnumerator CoRelease(SpritePiece obj, float time)
    {
        yield return new WaitForSeconds(time);
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(transform); // 풀의 자식으로 되돌림
        pool.Enqueue(obj);
    }
}
