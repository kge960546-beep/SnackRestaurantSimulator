using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PoolObjects : MonoBehaviour
{
    [SerializeField] GameObject riceCake;
    [SerializeField] GameObject cube;


    private void Start()
    {
        StartCoroutine(StartPoolCo());
    }

    IEnumerator StartPoolCo()
    {
        yield return new WaitUntil(() => NetworkManager.Singleton != null);
        yield return null;

        PoolManager.instance.CreatePool(riceCake, 100);        
        PoolManager.instance.CreatePool(cube, 100);        
    }
}
