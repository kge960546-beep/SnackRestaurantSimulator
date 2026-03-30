using Unity.Netcode;
using UnityEngine;

public class AutoMapGenerator : NetworkBehaviour
{
    [Header("레이어")]
    public string groundLayerName = "Ground";

    [Header("맵 크기")]
    public int mapWidth = 20;
    public int mapHeight = 20;

    [Header("멀티플레이용 시드")]
    public int mapSeed = 1234;

    public GameObject tempFryerPrefab;
    public GameObject tempCounterPrefab;
    
    public override void OnNetworkSpawn()
    {
        GenerateCityMap(mapSeed);

        if (IsServer)
        {
            SpawnKitchenStations();
        }
    }
    
    public void GenerateCityMap(int seed)
    {
        for(int x = 0; x < mapWidth; x++)
        {
            for(int z = 0; z < mapHeight; z++)
            {
                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);

                int layerIndex = LayerMask.NameToLayer(groundLayerName);
                if(layerIndex != -1)
                {
                    tile.layer = layerIndex;
                }

                tile.transform.position = new Vector3(x,0,z);
                tile.transform.localScale = new Vector3(1.0f, 0.1f, 1.0f);
                tile.transform.SetParent(this.transform);

                Renderer renderer = tile.GetComponent<Renderer>();

                if(x < 4)
                {
                    tile.name = $"Road+{x}_{z}";
                    renderer.material.color = new Color(0.1f, 0.1f, 0.1f);
                }
                else if (x >= 8 && x <= 18 && z >= 4 && z <= 15)
                {
                    tile.name = $"RestaurantFloor_{x}_{z}";
                    renderer.material.color = new Color(0.6f, 0.4f, 0.2f);
                    
                    bool isWall = (x == 8 || x == 18 || z == 4 || z == 15);

                    bool isDoor = (x == 8 && z >= 9 && z <= 10);

                    if (isWall && !isDoor)
                    {
                        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        wall.transform.position = new Vector3(x, 1.5f, z);
                        wall.transform.localScale = new Vector3(1f, 3f, 1f);
                        wall.name = $"Wall_{x}_{z}";
                        wall.transform.SetParent(this.transform);
                        wall.GetComponent<Renderer>().material.color = Color.white;
                    }
                }
                else
                {
                    // [나머지 공터] 도로, 인도, 식당이 아닌 곳은 초록색 잔디밭
                    tile.name = $"Grass_{x}_{z}";
                    renderer.material.color = new Color(0.2f, 0.5f, 0.2f);
                }
            }
        }
    }

    public void SpawnKitchenStations()
    {
        if (tempFryerPrefab != null)
        {
            Vector3 fryerPos = new Vector3(10f, 0.5f, 10f); // 바닥보다 살짝 위(0.5f)
            GameObject fryer = Instantiate(tempFryerPrefab, fryerPos, Quaternion.identity);

            // 프리팹을 맵에 찍어낸 뒤, 네트워크 상에 스폰 명령을 내립니다!
            fryer.GetComponent<NetworkObject>().Spawn(); // NGO 기준
        }

        // 2. 조리대 배치 (예: x=12, z=10)
        if (tempCounterPrefab != null)
        {
            Vector3 counterPos = new Vector3(12f, 0.5f, 10f);
            GameObject counter = Instantiate(tempCounterPrefab, counterPos, Quaternion.identity);
            counter.GetComponent<NetworkObject>().Spawn(); // NGO 기준
        }
    }
}
