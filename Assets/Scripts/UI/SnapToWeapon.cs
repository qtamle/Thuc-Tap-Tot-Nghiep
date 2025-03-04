using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SnapToWeapon : MonoBehaviour
{
    // UI
    public ScrollRect scrollRect;
    public RectTransform contentWeaponPanel;
    public RectTransform sampleListItem;
    public HorizontalLayoutGroup HLG;

    // Button
    public Button nextButton;
    public Button previousButton;
    public Button upgradeButton;
    public Button buyButton;

    // Text
    public Text buyButtonText;

    // Max Level
    public Text upgradeButtonText;
    public Sprite normalUpgradeSprite;
    public Sprite maxLevelSprite;

    // Action
    public event Action<WeaponData> OnSnapChanged;
    public event Action OnWeaponSelected;

    [HideInInspector]
    public string[] ItemNames;

    bool isSnapped;
    public float snapForce;
    float snapSpeed;

    private int currentItem = 0;
    private float snapDuration = 0.5f;

    // Current Weapon
    public WeaponData currentSnapWeapon;

    // Active Level to Buy
    [SerializeField]
    private LevelSystem levelSystem;

    void Start()
    {
        levelSystem = FindAnyObjectByType<LevelSystem>();
        isSnapped = false;

        InitializeWeaponNames();

        // Đăng ký sự kiện cho các nút
        nextButton.onClick.AddListener(NextWeapon);
        previousButton.onClick.AddListener(PreviousWeapon);
        upgradeButton.onClick.AddListener(UpgradeWeapon);
        buyButton.onClick.AddListener(TryBuyWeapon);

        // Cập nhật trạng thái nút khi bắt đầu
        UpdateButtonStates();
    }

    private void InitializeWeaponNames()
    {
        List<string> weaponNames = new List<string>();
        foreach (Transform child in contentWeaponPanel)
        {
            WeaponData weaponData = child.GetComponent<WeaponData>();
            if (weaponData != null)
            {
                weaponNames.Add(weaponData.weaponName);
            }
        }
        ItemNames = weaponNames.ToArray();
    }

    void Update()
    {
        // Tính toán chỉ số item hiện tại
        currentItem = Mathf.RoundToInt(
            (0 - contentWeaponPanel.localPosition.x) / (sampleListItem.rect.width + HLG.spacing)
        );

        // Đảm bảo chỉ số item hợp lệ
        currentItem = Mathf.Clamp(currentItem, 0, ItemNames.Length - 1);

        // Debug tên vũ khí hiện tại
        //Debug.Log($"Snapped to Weapon: {ItemNames[currentItem]}");

        // Nếu tốc độ kéo nhỏ và chưa snap
        if (scrollRect.velocity.magnitude < 200 && !isSnapped)
        {
            float targetPositionX = 0 - (currentItem * (sampleListItem.rect.width + HLG.spacing));
            snapSpeed += snapForce * Time.deltaTime;

            // Di chuyển dần về vị trí mục tiêu
            contentWeaponPanel.localPosition = new Vector3(
                Mathf.MoveTowards(contentWeaponPanel.localPosition.x, targetPositionX, snapSpeed),
                contentWeaponPanel.localPosition.y,
                contentWeaponPanel.localPosition.z
            );

            // Kiểm tra vị trí đã đạt mục tiêu (dùng epsilon thay vì so sánh trực tiếp)
            if (Mathf.Abs(contentWeaponPanel.localPosition.x - targetPositionX) < 0.1f)
            {
                isSnapped = true;
                snapSpeed = 0;

                // Gọi sự kiện thông báo vũ khí đã snap
                WeaponData snappedWeaponData = GetWeaponData(currentItem);
                NotifySnapChanged(snappedWeaponData);
            }
        }

        if (scrollRect.velocity.magnitude > 200)
        {
            isSnapped = false;
            snapSpeed = 0;
        }

        UpdateButtonStates();
    }

    private void NotifySnapChanged(WeaponData weaponData)
    {
        if (weaponData == null)
        {
            Debug.LogError("❌ NotifySnapChanged: weaponData is NULL!");
            return;
        }

        currentSnapWeapon = weaponData;
        ;
        if (currentSnapWeapon.weaponData == null)
        {
            Debug.LogError("⚠️ weaponData của vũ khí hiện tại bị NULL sau khi snap!");
        }

        // Debug.Log($"✅ NotifySnapChanged: Current weapon set to {weaponData.weaponName}");
        OnSnapChanged?.Invoke(weaponData);
    }

    private WeaponData GetWeaponData(int index)
    {
        // Tìm WeaponData từ danh sách các GameObject con
        Transform weaponTransform = contentWeaponPanel.GetChild(index);
        return weaponTransform.GetComponent<WeaponData>();
    }

    private void UpgradeWeapon()
    {
        if (currentSnapWeapon != null)
        {
            currentSnapWeapon.UpgradeWeapon(); // Gọi hàm nâng cấp vũ khí hiện tại
        }
    }

    // Hàm để di chuyển đến vũ khí kế tiếp
    private void NextWeapon()
    {
        if (currentItem < ItemNames.Length - 1)
        {
            currentItem++;
            SnapToCurrentItem();
        }
    }

    // Hàm để di chuyển đến vũ khí trước đó
    private void PreviousWeapon()
    {
        if (currentItem > 0)
        {
            currentItem--;
            SnapToCurrentItem();
        }
    }

    // Hàm thực hiện việc "snap" tới vị trí của vũ khí hiện tại
    private void SnapToCurrentItem()
    {
        float targetPositionX = 0 - (currentItem * (sampleListItem.rect.width + HLG.spacing));

        // Sử dụng LeanTween để di chuyển mượt mà
        LeanTween
            .moveLocalX(contentWeaponPanel.gameObject, targetPositionX, snapDuration)
            .setEase(LeanTweenType.easeInOutQuad);

        WeaponData snappedWeaponData = GetWeaponData(currentItem);
        NotifySnapChanged(snappedWeaponData);
    }

    public void UpdateButtonStates()
    {
        nextButton.gameObject.SetActive(currentItem < ItemNames.Length - 1);
        previousButton.gameObject.SetActive(currentItem > 0);

        if (currentSnapWeapon != null)
        {
            WeaponSO weaponData = currentSnapWeapon.weaponData;
            bool isOwned = weaponData.isOwned;
            bool canBuy = levelSystem.level >= weaponData.requiredLevel;

            bool isMaxLevel = currentSnapWeapon.currentLevel >= currentSnapWeapon.maxLevel;

            upgradeButton.gameObject.SetActive(isOwned);

            if (isOwned)
            {
                if (isMaxLevel)
                {
                    upgradeButton.interactable = false;
                    upgradeButtonText.text = "Max Level";
                    upgradeButton.image.sprite = maxLevelSprite;
                }
                else
                {
                    upgradeButton.interactable = true;
                    upgradeButtonText.text = "Upgrade";
                    upgradeButton.image.sprite = normalUpgradeSprite;
                }
            }

            buyButton.gameObject.SetActive(!isOwned);

            if (!isOwned)
            {
                buyButton.interactable = canBuy;
                buyButtonText.text = canBuy ? "Buy" : $"Require Level {weaponData.requiredLevel}";
            }
        }
    }

    private void TryBuyWeapon()
    {
        if (currentSnapWeapon != null)
        {
            WeaponSO weaponData = currentSnapWeapon.weaponData;

            if (levelSystem.level >= weaponData.requiredLevel)
            {
                _ = currentSnapWeapon.BuyWeapon();
                UpdateButtonStates(); // Cập nhật giao diện sau khi mua
            }
            else
            {
                Debug.LogWarning(
                    $"Cannot buy {weaponData.weaponName}. Level {weaponData.requiredLevel} required."
                );
            }
        }
    }

    public void SelectWeapon(WeaponData weapon)
    {
        currentSnapWeapon = weapon;
        OnWeaponSnapped(weapon);
        OnWeaponSelected?.Invoke();

        Debug.Log($"Weapon selected: {weapon.weaponName}");
    }

    public async void LoadWeaponData(string weaponID)
    {
        var characterWeaponData = await SaveService.GetWeaponID(weaponID);

        if (characterWeaponData != null)
        {
            Debug.Log($"✅ SnapToWeapon: Loaded weapon data for ID {weaponID}.");

            if (currentSnapWeapon != null)
            {
                // currentSnapWeapon.InitWeapon(characterWeaponData);
            }   
            else
            {
                Debug.LogError("❌ SnapToWeapon: currentSnapWeapon is NULL!");
            }
        }
        else
        {
            Debug.Log($"❌ SnapToWeapon: Failed to load weapon data for ID {weaponID}.");
        }
    }

    private void OnWeaponSnapped(WeaponData snappedWeapon)
    {
        if (snappedWeapon == null)
        {
            Debug.LogError("❌ OnWeaponSnapped: snappedWeapon is NULL!");
            return;
        }

        string weaponID = snappedWeapon.weaponData.WeaponID;

        // Debug.Log($"🔄 OnWeaponSnapped: Snapped to weapon ID {weaponID} ");

        LoadWeaponData(weaponID);
    }
}
