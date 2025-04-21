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

    public static string GetWeaponNameById(int weaponID)
    {
        switch (weaponID.ToString())
        {
            case "1":
                return "Dagger";
            case "2":
                return "Gloves";
            case "3":
                return "Chainsaw";
            case "4":
                return "Claws";
            case "5":
                return "Energy Orb";
            case "6":
                return "Katana";
            default:
                return "Weapon: No Selected";
        }
    }

}
