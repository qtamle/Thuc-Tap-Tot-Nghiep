using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WeaponData : MonoBehaviour
{
    [Header("Equip Weapon Data")]
    public WeaponSO weaponData;

    [Header("Weapon Details")]
    public string weaponName;
    public int basePrice;
    public Sprite weaponSprite;

    [Header("Upgrade Levels")]
    public int[] upgradePrices = new int[4];

    [Header("Upgrade Status")]
    public int currentLevel;
    public int maxLevel = 4;

    public bool isOwned;

    private int originalLevel;
    private CoinsManager coinsManager;

    public event Action<WeaponData> OnSlotSelected; // Sự kiện khi chọn vũ khí

    public GameObject upgradeSuccessPanel;
    public TextMeshProUGUI upgradeMessageText;

    private SnapToWeapon snapToWeapon;

    public void SelectWeapon()
    {
        OnSlotSelected?.Invoke(this);
    }

    // private void OnEnable()
    // {
    //     SceneManager.sceneLoaded += OnSceneLoaded;
    // }

    // private void OnDisable()
    // {
    //     SceneManager.sceneLoaded -= OnSceneLoaded;
    // }

    private async void Start()
    {
        ResourcesService.ResetWeapons();
        coinsManager = FindFirstObjectByType<CoinsManager>();
        snapToWeapon = FindAnyObjectByType<SnapToWeapon>();

        // Kiểm tra xem dữ liệu đã tồn tại chưa
        bool dataExists = await LoadWeaponData();

        if (!dataExists)
        {
            Debug.Log("Create weaponData");
            CreateWeaponData(); // Chỉ tạo nếu chưa có dữ liệu
        }
        else
        {
            Debug.Log("weapon exist");
        }
        // OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);

        if (!SceneManager.GetSceneByName("Lobby").IsValid())
        {
            upgradeSuccessPanel.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        ResourcesService.ResetWeapons();
        // SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    // {
    //     coinsManager = FindFirstObjectByType<CoinsManager>();

    //     if (weaponData != null)
    //     {
    //         originalLevel = weaponData.currentLevel;
    //         weaponName = weaponData.weaponName;
    //         weaponSprite = weaponData.weaponSprite;
    //         isOwned = weaponData.isOwned;
    //         // Debug.Log($"Weapon data updated: {weaponName} - Level {currentLevel}");
    //     }
    //     else
    //     {
    //         // Debug.LogWarning("Weapon data is null. Please assign a WeaponSO asset.");
    //     }
    // }

    private void OnValidate()
    {
        if (weaponData != null)
        {
            weaponName = weaponData.weaponName;
            weaponSprite = weaponData.weaponSprite;
            upgradePrices = weaponData.upgradeCosts;
        }
    }

    public async Task BuyWeapon()
    {
        if (weaponData == null || weaponData.isOwned)
        {
            // Debug.LogWarning($"{weaponName} is already owned or weaponData is null.");
            return;
        }

        if (coinsManager == null)
        {
            // Debug.LogError("CoinsManager is not set up properly!");
            return;
        }

        // 🔹 Mua vũ khí bằng TryPurchase mới
        bool purchaseSuccess = await coinsManager.TryPurchase(basePrice);
        if (!purchaseSuccess)
        {
            Debug.LogWarning($"Not enough coins to buy {weaponName}. Price: {basePrice}");
            return;
        }

        // 🔹 Tải dữ liệu hiện có từ cloud
        var data = await SaveService.GetWeaponID(weaponData.WeaponID);
        if (data == null)
        {
            data = new CharacterWeaponData
            {
                weaponName = weaponData.weaponName,
                currentLevel = currentLevel,
                WeaponID = weaponData.WeaponID,
                isOwned = true, // Mua vũ khí
            };
        }
        else
        {
            data.isOwned = true; // Cập nhật trạng thái sở hữu
        }

        // 🔹 Lưu lại dữ liệu đã cập nhật
        await SaveService.SaveWeaponData(data);
        weaponData.isOwned = true;
        isOwned = true;
        // Debug.Log($"✅ {weaponName} has been purchased for {basePrice} coins!");

        upgradeMessageText.text = "Successful Purchase!";
        upgradeSuccessPanel.SetActive(true);
        StartCoroutine(HideUpgradeSuccessPanel());
    }

    public async void UpgradeWeapon()
    {
        if (weaponData == null || !weaponData.isOwned)
        {
            Debug.Log($"❌ Cannot upgrade {weaponName}. Weapon is not owned or weaponData is null.");
            return;
        }

        if (currentLevel >= maxLevel)
        {
            Debug.Log($"🔹 Max level reached for {weaponName}.");
            return;
        }

        if (upgradePrices == null || upgradePrices.Length < maxLevel)
        {
            Debug.Log($"❌ Upgrade prices array is not properly set up for {weaponName}.");
            return;
        }

        // ✅ Sử dụng giá của level trước đó
        int upgradeCost = upgradePrices[currentLevel - 1];
        Debug.Log("upgradeCost = " + upgradeCost);
        if (upgradeCost == 0)
        {
            Debug.LogWarning(
                $"❌ Upgrade cost for {weaponName} at level {currentLevel} is invalid."
            );
            return;
        }

        // ✅ Kiểm tra coinsManager không bị null
        if (coinsManager == null)
        {
            Debug.LogError("❌ CoinsManager is null! Cannot upgrade weapon.");
            return;
        }

        bool UpgradeSuccess = await coinsManager.TryUpgrade(upgradeCost);
        if (!UpgradeSuccess)
        {
            Debug.LogWarning($"Not enough coins to upgrade {weaponName}. Price: {upgradeCost}");
            return;
        }

        // 🔹 Tải dữ liệu hiện có từ cloud
        var data = await SaveService.GetWeaponID(weaponData.WeaponID);
        if (data == null)
        {
            Debug.LogError($"❌ No existing data found for {weaponName}. Cannot upgrade.");
            return;
        }

        // 🔹 Cập nhật cấp độ mới
        currentLevel++;
        data.currentLevel = currentLevel;
        weaponData.currentLevel = currentLevel;

        // 🔹 Lưu lại dữ liệu đã cập nhật
        await SaveService.SaveWeaponData(data);

        Debug.Log(
            $"✅ {weaponName} đã nâng cấp lên cấp {currentLevel} với giá {upgradeCost} coins!"
        );

        upgradeMessageText.text = "Successful Upgrade!";
        upgradeSuccessPanel.SetActive(true);
        StartCoroutine(HideUpgradeSuccessPanel());

        FindAnyObjectByType<SnapToWeapon>()?.UpdateButtonStates();
    }

    public void InitWeapon(CharacterWeaponData data)
    {
        if (data == null)
        {
            // Debug.LogError($"❌ CharacterWeaponData bị null! Không thể khởi tạo vũ khí.");
            return;
        }

        // weaponData = ResourcesService.GetWeaponById(data.WeaponID);

        if (weaponData == null)
        {
            // Debug.LogError($"❌ Không tìm thấy WeaponSO với ID {data.WeaponID}!");
            return;
        }

        weaponName = data.weaponName;
        weaponSprite = weaponData.weaponSprite;
        currentLevel = data.currentLevel;
        weaponData.isOwned = data.isOwned;

        originalLevel = currentLevel; // Giữ lại cấp ban đầu để tránh lỗi

        // Debug.Log($"✅ Vũ khí {weaponName} được khởi tạo với cấp {currentLevel}");
    }

    public async void CreateWeaponData()
    {
        if (weaponData == null)
        {
            // Debug.LogError("❌ WeaponSO is not assigned. Cannot create WeaponData.");
            return;
        }

        // 🔹 Kiểm tra dữ liệu đã tồn tại chưa
        bool dataExists = await LoadWeaponData();
        if (dataExists)
        {
            // Debug.Log($"⚠ Dữ liệu vũ khí {weaponName} đã tồn tại. Không cần tạo mới.");
            return;
        }

        // 🔹 Nếu chưa có dữ liệu, tạo mới
        var data = new CharacterWeaponData
        {
            weaponName = weaponData.weaponName,
            currentLevel = currentLevel,
            WeaponID = weaponData.WeaponID,
            isOwned = false,
        };

        upgradeSuccessPanel.SetActive(true);
        StartCoroutine(HideUpgradeSuccessPanel());

        await SaveService.SaveWeaponData(data);
        updateData(data);
        // Debug.Log($"✅ Dữ liệu vũ khí {weaponName} đã được tạo. Level: {currentLevel}");
    }

    public async Task<bool> LoadWeaponData()
    {
        if (weaponData == null)
        {
            // Debug.LogError("❌ WeaponSO is not assigned. Cannot load WeaponData.");
            return false;
        }

        var savedData = await SaveService.GetWeaponID(weaponData.WeaponID);
        if (savedData != null)
        {
            // 🔹 Lấy lại WeaponSO từ ResourcesService để đảm bảo dữ liệu chuẩn
            // weaponData = ResourcesService.GetWeaponById(savedData.WeaponID);

            if (weaponData == null)
            {
                // Debug.LogError($"❌ Không tìm thấy WeaponSO với ID {savedData.WeaponID}!");
                return false;
            }

            // ✅ Cập nhật lại dữ liệu trong Inspector theo dữ liệu từ cloud
            weaponName = savedData.weaponName;
            weaponSprite = weaponData.weaponSprite;
            currentLevel = savedData.currentLevel;
            originalLevel = currentLevel;
            isOwned = savedData.isOwned;

            // Debug.Log(
            //     $"✅ Dữ liệu vũ khí {weaponName} đã tải từ cloud. Level: {currentLevel}, isOwned: {isOwned}"
            // );
            return true;
        }

        return false; // Không tìm thấy dữ liệu
    }

    public void updateData(CharacterWeaponData savedData)
    {
        weaponName = savedData.weaponName;
        weaponSprite = weaponData.weaponSprite;
        currentLevel = savedData.currentLevel;
        weaponData.isOwned = savedData.isOwned;
    }

    private IEnumerator HideUpgradeSuccessPanel()
    {
        yield return new WaitForSeconds(2f);
        upgradeSuccessPanel.SetActive(false);
        upgradeMessageText.text = "";

        if (snapToWeapon != null)
        {
            snapToWeapon.CloseUpgradeInfo();
        }
    }
}
