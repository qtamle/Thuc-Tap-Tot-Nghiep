using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TestScene : MonoBehaviour
{
    public string sceneName;

    public SnapToWeapon snapToWeapon;

    private GameObject currentWeaponInstance;
    public Button loadSceneButton;

    public static WeaponSO weaponDataStore;

    public void LoadTest1()
    {
        if (
            snapToWeapon != null
            && snapToWeapon.currentSnapWeapon != null
            && snapToWeapon.currentSnapWeapon.weaponData != null
        )
        {
            WeaponSO weaponData = snapToWeapon.currentSnapWeapon.weaponData;

            if (!weaponData.isOwned)
            {
                Debug.LogError($"Weapon {weaponData.weaponName} chưa được mua!");
                return;
            }

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

            WeaponInfo weaponInfo = currentWeaponInstance.AddComponent<WeaponInfo>();
            weaponInfo.weaponName = weaponData.weaponName;
            weaponInfo.weaponLevel = weaponData.currentLevel;

            SceneManager.sceneLoaded += OnSceneLoaded;

            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("Không có vũ khí nào được chọn!");
        }
    }

    public void Summary()
    {
        ExperienceManager.Instance.SubmitExperience();
        SceneManager.LoadScene("Shop_Online");
    }

    public void LoadSceneONly(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
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

    private void UpdateButtonState()
    {
        if (loadSceneButton != null)
        {
            if (
                snapToWeapon != null
                && snapToWeapon.currentSnapWeapon != null
                && snapToWeapon.currentSnapWeapon.weaponData != null
            )
            {
                WeaponSO weaponData = snapToWeapon.currentSnapWeapon.weaponData;

                loadSceneButton.interactable = weaponData.isOwned;
            }
            else
            {
                loadSceneButton.interactable = false;
            }
        }
        else
        {
            Debug.LogError("Load Scene Button is not assigned in the Inspector!");
        }
    }

    private void OnEnable()
    {
        if (snapToWeapon != null)
        {
            snapToWeapon.OnWeaponSelected += UpdateButtonState;
        }
    }

    private void OnDisable()
    {
        if (snapToWeapon != null)
        {
            snapToWeapon.OnWeaponSelected -= UpdateButtonState;
        }
    }
}
