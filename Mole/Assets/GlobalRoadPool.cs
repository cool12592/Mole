using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalRoadPool : MonoBehaviour
{
    public static GlobalRoadPool Instance { get; private set; }

    [SerializeField] private Road roadPrefab;
    private int initialPoolSize = 100;

    private Stack<Road> pool = new Stack<Road>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

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
            var obj = Instantiate(roadPrefab);
            obj.transform.SetParent(transform);
            obj.gameObject.SetActive(false);
            pool.Push(obj);
            obj.IsInPool = true;

        }
    }

    public Road GetRoad(Vector3 position, Vector3 scale)
    {
        if (pool.Count == 0)
        {
            CreateNewObject(100);
        }

        Road obj = pool.Pop();
        obj.IsInPool = false;

        obj.transform.position = position;
        obj.transform.rotation = Quaternion.identity;
        obj.transform.localScale = scale;
        obj.Init();
       // obj.transform.SetParent(parent);
        obj.gameObject.SetActive(true);

        return obj;
    }

    public void Release(Road obj)
    {
        if (obj.IsInPool) //중복삽입 방지
            return;

        obj.gameObject.SetActive(false);
       // obj.transform.SetParent(transform); // 풀의 자식으로 되돌림
        pool.Push(obj);
        obj.IsInPool = true;
    }
}
