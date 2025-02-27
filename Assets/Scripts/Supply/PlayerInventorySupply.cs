using System.Collections.Generic;
using UnityEngine;

public class PlayerInventorySupply : MonoBehaviour
{
    public static PlayerInventorySupply Instance { get; private set; }

    [SerializeField]
    public List<SupplyData> playerInventorys = new List<SupplyData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
