using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public enum SupplyType
{
    Survival,
    Combat,
    Agility,
}

[CreateAssetMenu(fileName = "SupplyData", menuName = "Supply/SupplyData")]
public class SupplyData : ScriptableObject
{
    public String supplyID;

    public String supplyName;
    public GameObject supplyPrefab;
    public Sprite supplySprite;
    public SupplyType supplyType;
    public float spawnWeight; // Tỷ lệ spawn

    [TextArea]
    public string description;
}

[System.Serializable]
public struct SupplyDataNetwork : INetworkSerializable, IEquatable<SupplyDataNetwork>
{
    public FixedString32Bytes supplyID;
    public FixedString32Bytes supplyName;
    public float spawnWeight;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref supplyID);
        serializer.SerializeValue(ref supplyName);
        serializer.SerializeValue(ref spawnWeight);
    }

    public bool Equals(SupplyDataNetwork other)
    {
        return supplyID.Equals(other.supplyID)
            && supplyName.Equals(other.supplyName)
            && Mathf.Approximately(spawnWeight, other.spawnWeight); // So sánh gần đúng cho float
    }

    public override bool Equals(object obj)
    {
        return obj is SupplyDataNetwork other && Equals(other);
    }

    public override int GetHashCode()
    {
        // Kết hợp hash code của các trường để tránh xung đột
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + supplyID.GetHashCode();
            hash = hash * 23 + supplyName.GetHashCode();
            hash = hash * 23 + spawnWeight.GetHashCode();
            return hash;
        }
    }

    public SupplyDataNetwork(SupplyData data)
    {
        // Chuyển đổi string sang FixedString32Bytes
        supplyID = new FixedString32Bytes(data.supplyID);
        supplyName = new FixedString32Bytes(data.supplyName);
        spawnWeight = data.spawnWeight;
    }
}
