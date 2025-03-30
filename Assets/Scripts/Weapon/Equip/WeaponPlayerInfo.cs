using Unity.Netcode;
using UnityEngine;

public class WeaponPlayerInfo : NetworkBehaviour
{
    public static WeaponPlayerInfo Instance;
    public string weaponName;

    public int weaponLevel;

    void Start()
    {
        // DontDestroyOnLoad(gameObject);
    }

    public void SetWeaponInfo(string name, int level)
    {
        weaponName = name;
        weaponLevel = level;
    }
}
