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

    public event Action<WeaponData> OnSnapChanged; // Sự kiện thông báo vũ khí đã snap

    [HideInInspector]
    public string[] ItemNames;

    bool isSnapped;
    public float snapForce;
    float snapSpeed;

    void Start()
    {
        isSnapped = false;
        // Tự động lấy thông tin từ các GameObject vũ khí
        InitializeWeaponNames();
    }

    private void InitializeWeaponNames()
    {
        List<string> weaponNames = new List<string>();
        foreach (Transform child in contentWeaponPanel)
        {
            WeaponData weaponData = child.GetComponent<WeaponData>();
            if (weaponData != null)
            {
                weaponNames.Add(weaponData.weaponName); // Lấy tên vũ khí
            }
        }
        ItemNames = weaponNames.ToArray();
    }

    void Update()
    {
        // Tính toán chỉ số item hiện tại
        int currentItem = Mathf.RoundToInt(
            (0 - contentWeaponPanel.localPosition.x) / (sampleListItem.rect.width + HLG.spacing)
        );

        // Đảm bảo chỉ số item hợp lệ
        currentItem = Mathf.Clamp(currentItem, 0, ItemNames.Length - 1);

        // Debug tên vũ khí hiện tại
        Debug.Log($"Snapped to Weapon: {ItemNames[currentItem]}");

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
                isSnapped = true; // Đánh dấu đã snap
                snapSpeed = 0; // Reset tốc độ snap

                // Gọi sự kiện thông báo vũ khí đã snap
                WeaponData snappedWeaponData = GetWeaponData(currentItem);
                NotifySnapChanged(snappedWeaponData);
            }
        }

        // Nếu người dùng kéo mạnh, hủy trạng thái snap
        if (scrollRect.velocity.magnitude > 200)
        {
            isSnapped = false;
            snapSpeed = 0; // Reset tốc độ snap khi hủy snap
        }
    }

    private void NotifySnapChanged(WeaponData weaponData)
    {
        // Kiểm tra nếu sự kiện OnSnapChanged có listeners thì gọi
        if (OnSnapChanged != null)
        {
            OnSnapChanged.Invoke(weaponData);
        }
    }

    private WeaponData GetWeaponData(int index)
    {
        // Tìm WeaponData từ danh sách các GameObject con
        Transform weaponTransform = contentWeaponPanel.GetChild(index);
        return weaponTransform.GetComponent<WeaponData>();
    }
}
