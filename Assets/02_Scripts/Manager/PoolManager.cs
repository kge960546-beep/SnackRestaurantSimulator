using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PoolManager : MonoBehaviour
{
    public static PoolManager instance;

    private Dictionary<GameObject, Queue<GameObject>> poolDic = new Dictionary<GameObject, Queue<GameObject>>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CreatePool(GameObject prefab, int count)
    {
        if (!poolDic.ContainsKey(prefab))
        {
            poolDic.Add(prefab, new Queue<GameObject>());
        }

        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefab, this.transform);
            obj.SetActive(false);           
            
            poolDic[prefab].Enqueue(obj);
        }
    }

    public GameObject Get(GameObject poolPrefab, Vector3 pos, Quaternion rot)
    {
        if (!poolDic.ContainsKey(poolPrefab))
        {
            poolDic.Add(poolPrefab, new Queue<GameObject>());
        }

        GameObject obj = (poolDic[poolPrefab].Count > 0) ? poolDic[poolPrefab].Dequeue() : Instantiate(poolPrefab);       

        obj.transform.SetPositionAndRotation(pos, rot);
        obj.transform.SetParent(null);
        obj.SetActive(true);


        var netObj = obj.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            if (NetworkManager.Singleton.IsServer && !netObj.IsSpawned)
            {
                netObj.Spawn();
            }
        }
        

        return obj;
    }

    public void ReturnIt(GameObject poolPrefab, GameObject obj)
    {
        if (!poolDic.ContainsKey(poolPrefab))
        {
            Debug.LogWarning("Pool key missing. Creating new pool for: " + poolPrefab.name);
            poolDic.Add(poolPrefab, new Queue<GameObject>());
        }

        if(obj.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = false;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        var netObj = obj.GetComponent<NetworkObject>();
        if (netObj != null) 
        {
            if (NetworkManager.Singleton.IsServer && netObj.IsSpawned)
            {
                netObj.TryRemoveParent();
                obj.transform.SetParent(null);
                netObj.Despawn(false);
            }           
        }
        else
        {
            obj.transform.SetParent(null);
        }


        obj.SetActive(false);
        //obj.transform.SetParent(this.transform); 
        if (poolDic.ContainsKey(poolPrefab))
        {
            poolDic[poolPrefab].Enqueue(obj);
        }
        
    }
}
