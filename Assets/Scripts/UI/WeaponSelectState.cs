using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public struct WeaponSelectState : INetworkSerializable, IEquatable<WeaponSelectState>
{
    public ulong ClientId;
    public FixedString64Bytes WeaponID;
    public bool IsReady;

    public WeaponSelectState(ulong clientId, FixedString64Bytes weaponID, bool isReady)
    {
        ClientId = clientId;
        WeaponID = weaponID.IsEmpty ? "1" : weaponID;
        IsReady = isReady;
    }

    public bool Equals(WeaponSelectState other)
    {
        return ClientId == other.ClientId && WeaponID == other.WeaponID && IsReady == other.IsReady;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref WeaponID);
        serializer.SerializeValue(ref IsReady);
    }
}
