using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PoolInfo
{
    public GameObject prefab;
    public int count; // 미리
}

    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance;
        public List<PoolInfo> prewarmList;
        private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();
        void Awake()
        {
            Instance = this;
            InitializePools();
        }
        void InitializePools()
        {
            foreach (var info in prewarmList)
            {
                string key = info.prefab.name;

                if (!poolDictionary.ContainsKey(key))
                {
                    poolDictionary.Add(key, new Queue<GameObject>());
                }

                for (int i = 0; i < info.count; i++)
                {
                    GameObject newObj = CreateNewObject(info.prefab, key);
                    newObj.SetActive(false); // 꺼두고
                    poolDictionary[key].Enqueue(newObj); // 풀에 넣기
                }
            }
        }
        GameObject CreateNewObject(GameObject prefab, string key)
        {
            GameObject newObj = Instantiate(prefab);
            newObj.name = key;
            newObj.transform.SetParent(transform);
            return newObj;
        }

        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)//꺼내기 
        {
            string key = prefab.name;
            if (!poolDictionary.ContainsKey(key))
            {
                poolDictionary.Add(key, new Queue<GameObject>());
            }
            if (poolDictionary[key].Count > 0)
            {
                GameObject obj = poolDictionary[key].Dequeue();
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                obj.SetActive(true);
                return obj;
            }
            else
            {
                GameObject newObj = CreateNewObject(prefab, key);
                newObj.transform.position = position;
                newObj.transform.rotation = rotation;
                return newObj;
            }
        }

        //풀을 나눌 필요가 있을까?

        public void ReturnToPool(GameObject obj)
        {
            string key = obj.name;
            obj.SetActive(false);
            if (!poolDictionary.ContainsKey(key))
            {
                poolDictionary.Add(key, new Queue<GameObject>());
            }
            poolDictionary[key].Enqueue(obj);
            obj.transform.SetParent(transform);
        }
    }
