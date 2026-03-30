using Unity.Netcode;
using UnityEngine;

public class MapManager : NetworkBehaviour
{
    public GameObject cookingStationPrefab;

    public void GenerateMap(int seed)
    {
        Random.InitState(seed);

        for (int x = 0; x < 10; x++)
        {
            for (int z = 0; z < 10; z++)
            {
                GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floor.transform.position = new Vector3(x, 0, z);

                if (IsServer)
                {
                    if (Random.value < 0.05f)
                    {
                        Vector3 stationPos = new Vector3(x, 0.5f, z);

                        GameObject station = Instantiate(cookingStationPrefab, stationPos, Quaternion.identity);

                        station.GetComponent<NetworkObject>().Spawn();
                    }
                }
            }
        }
    }
}
