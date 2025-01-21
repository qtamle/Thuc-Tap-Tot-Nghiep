using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SnapToWeapon : MonoBehaviour
{
    public ScrollRect scrollRect;
    public RectTransform contentWeaponPanel;
    public RectTransform sampleListItem;
    public HorizontalLayoutGroup HLG;

    public Button nextButton;
    public Button previousButton;
    public Button upgradeButton;

    public event Action<WeaponData> OnSnapChanged; 

    [HideInInspector]
    public string[] ItemNames;

    bool isSnapped;
    public float snapForce;
    float snapSpeed;

    private int currentItem = 0; 
    private float snapDuration = 0.5f;

    public WeaponData currentSnapWeapon;
    void Start()
    {
        isSnapped = false;
        InitializeWeaponNames();

        // Đăng ký sự kiện cho các nút
        nextButton.onClick.AddListener(NextWeapon);
        previousButton.onClick.AddListener(PreviousWeapon);
        upgradeButton.onClick.AddListener(UpgradeWeapon);

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
        currentSnapWeapon = weaponData;
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
            currentSnapWeapon.UpgradeWeapon();  // Gọi hàm nâng cấp vũ khí hiện tại
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
        LeanTween.moveLocalX(contentWeaponPanel.gameObject, targetPositionX, snapDuration).setEase(LeanTweenType.easeInOutQuad);

        WeaponData snappedWeaponData = GetWeaponData(currentItem);
        NotifySnapChanged(snappedWeaponData);
    }

    private void UpdateButtonStates()
    {
        nextButton.gameObject.SetActive(currentItem < ItemNames.Length - 1);

        previousButton.gameObject.SetActive(currentItem > 0);
    }
}
