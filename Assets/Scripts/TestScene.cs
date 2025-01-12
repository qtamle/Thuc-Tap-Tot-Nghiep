using UnityEngine;
using UnityEngine.SceneManagement;

public class TestScene : MonoBehaviour
{
    public string sceneName;
    public SnapToWeapon snapToWeapon;

    private GameObject currentWeaponInstance;

    public static WeaponSO weaponDataStore;

    public void LoadTest1()
    {
        if (snapToWeapon != null && snapToWeapon.currentSnapWeapon != null && snapToWeapon.currentSnapWeapon.weaponData != null)
        {
            WeaponSO weaponData = snapToWeapon.currentSnapWeapon.weaponData;
            GameObject weaponPrefab = weaponData.weapon;

            if (weaponPrefab == null)
            {
                Debug.LogError($"Weapon {weaponData.weaponName} không có prefab!");
                return;
            }

            weaponDataStore = weaponData;

            if (currentWeaponInstance != null)
            {
                Destroy(currentWeaponInstance);
            }

            currentWeaponInstance = Instantiate(weaponPrefab);
            DontDestroyOnLoad(currentWeaponInstance); 

            SceneManager.sceneLoaded += OnSceneLoaded;

            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("Không có vũ khí nào được chọn!");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (weaponDataStore != null)
        {
            GameObject spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawn");
            if (spawnPoint != null)
            {
                currentWeaponInstance.transform.position = spawnPoint.transform.position;
                currentWeaponInstance.transform.rotation = spawnPoint.transform.rotation;
            }
            else
            {
                Debug.LogError("Không tìm thấy spawn point trong scene mới!");
            }
        }
        else
        {
            Debug.LogError("Không có vũ khí được lưu trữ!");
        }
    }
}
