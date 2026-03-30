using Unity.Netcode;
using UnityEngine;

public class PlayerInteraction : NetworkBehaviour
{
    public Material topMostMaterial;
    private Material originalMaterial;

    private NetworkObject haveItem;

    public Transform handAchor;

    public Camera myPlayerCamera;

    [SerializeField] float throwingForce = 4.0f;

    void Update()
    {
        if (!IsOwner) return;

        Debug.DrawRay(myPlayerCamera.transform.position, myPlayerCamera.transform.forward * 5.0f, Color.red);

        if (haveItem != null)
        {
            haveItem.transform.position = handAchor.position;
            haveItem.transform.rotation = handAchor.rotation;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = new Ray(myPlayerCamera.transform.position, myPlayerCamera.transform.forward);
            bool isHit = Physics.Raycast(ray, out RaycastHit hit, 5.0f);

            if (isHit)
            {
                CookingStation station = hit.collider.GetComponentInParent<CookingStation>();
                IngredientDispenser dispenser = hit.collider.GetComponentInParent<IngredientDispenser>();
                Trash trash = hit.collider.GetComponentInParent<Trash>();


                if (station != null)
                {
                    if (haveItem != null)
                    {
                        if (haveItem.TryGetComponent(out Ingredient ingredient))
                        {
                            RequestPutInStationRpc(station.NetworkObjectId, haveItem.NetworkObjectId);
                            haveItem = null;
                        }
                        else
                        {
                            HandleInteraction(hit);
                        }
                    }
                    else
                    {
                        station.Interact();
                    }
                }
                else if (dispenser != null)
                {
                    if (haveItem == null)
                    {
                        dispenser.Interact();
                    }
                }
                else if(trash != null)
                {
                    if (haveItem != null)
                    {

                        RequestTrashRpc(haveItem.NetworkObjectId);
                        haveItem = null;
                    }
                }

                else
                {
                    HandleInteraction(hit);
                }
            }
            else
            {
                if (haveItem != null)
                {
                    RequestPickDownRpc();
                    haveItem = null;
                }
            }
        }


    }

    [Rpc(SendTo.Server)]
    void RequestPickUpRpc(ulong itemId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(itemId, out NetworkObject targetObj))
        {
            if (targetObj.transform.parent != null)
            {
                if (targetObj.transform.parent != PoolManager.instance.transform &&
                    targetObj.transform.parent.GetComponent<NetworkObject>() != null)
                {
                    Debug.LogWarning("이미 다른 사람이 들고 있는 아이템입니다.");
                    return;
                }
            }

            targetObj.TrySetParent(this.NetworkObject);

            if (targetObj.TryGetComponent(out Rigidbody rb))
            {
                rb.isKinematic = true;
            }

            if (targetObj.TryGetComponent(out Collider col))
            {
                col.isTrigger = true;
            }

            haveItem = targetObj;

            targetObj.transform.position = handAchor.position;
            targetObj.transform.rotation = handAchor.rotation;

            Debug.Log(itemId + "번 아이템을 집었습니다");
        }
    }

    [Rpc(SendTo.Server)]
    void RequestPickDownRpc()
    {
        if (haveItem != null)
        {
            haveItem.TryRemoveParent();

            if (haveItem.TryGetComponent(out Rigidbody rb))
            {
                rb.isKinematic = false;

                rb.AddForce(transform.forward * throwingForce, ForceMode.Impulse);
            }
            if (haveItem.TryGetComponent(out Collider col))
            {
                col.isTrigger = false;
            }

            haveItem = null;
        }
    }

    [Rpc(SendTo.Server)]
    void RequestTrashRpc(ulong itemId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(itemId, out NetworkObject iObj))
        {
            GameObject origin = null;
            
            if (iObj.TryGetComponent(out Ingredient ingredient))
            {
                origin = ingredient.originPrefab;
            }
            else if (iObj.TryGetComponent(out Food food))
            {
                origin = food.originPrefab;
            }
           
            if (origin != null)
            {               
                if (iObj.TryGetComponent(out Collider col))
                {
                    col.isTrigger = false;
                }                
                PoolManager.instance.ReturnIt(origin, iObj.gameObject);
                Debug.Log("아이템을 쓰레기통에 버렸습니다!");
            }
            else
            {                
                Debug.LogWarning("이 아이템에는 originPrefab 꼬리표가 없어서 버릴 수 없습니다!");
                iObj.TryRemoveParent();
                iObj.Despawn();
            }
        }
    }

    void HandleInteraction(RaycastHit hit)
    {
        if (haveItem == null)
        {
            NetworkObject netObj = hit.collider.GetComponentInParent<NetworkObject>();

            if (netObj != null)
            {
                if (netObj.TryGetComponent(out Ingredient ingredient) || netObj.TryGetComponent(out Food food))
                {
                    RequestPickUpRpc(netObj.NetworkObjectId);
                    haveItem = netObj;
                }
            }
        }
        else
        {
            RequestPickDownRpc();
            haveItem = null;
        }
    }

    [Rpc(SendTo.Server)]
    void RequestPutInStationRpc(ulong stationId, ulong itemId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(stationId, out NetworkObject sObj) &&
        NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(itemId, out NetworkObject iObj))
        {
            CookingStation station = sObj.GetComponent<CookingStation>();
            Ingredient ingredient = iObj.GetComponent<Ingredient>();

            if (station != null && ingredient != null)
            {
                station.AddIngredient(ingredient.type);
                PoolManager.instance.ReturnIt(ingredient.originPrefab, iObj.gameObject);
            }
        }
    }
}
