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

    public Button nextButton; // Nút Next
    public Button previousButton; // Nút Previous

    public event Action<WeaponData> OnSnapChanged; // Sự kiện thông báo vũ khí đã snap

    [HideInInspector]
    public string[] ItemNames;

    bool isSnapped;
    public float snapForce;
    float snapSpeed;

    private int currentItem = 0; // Chỉ số vũ khí hiện tại
    private float snapDuration = 0.5f; // Thời gian di chuyển cho LeanTween

    void Start()
    {
        isSnapped = false;
        // Tự động lấy thông tin từ các GameObject vũ khí
        InitializeWeaponNames();

        // Đăng ký sự kiện cho các nút
        nextButton.onClick.AddListener(NextWeapon);
        previousButton.onClick.AddListener(PreviousWeapon);

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
                weaponNames.Add(weaponData.weaponName); // Lấy tên vũ khí
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

        // Cập nhật trạng thái của nút Next và Previous
        UpdateButtonStates();
    }

    private void NotifySnapChanged(WeaponData weaponData)
    {
        // Kiểm tra nếu sự kiện OnSnapChanged có listeners thì gọi
        OnSnapChanged?.Invoke(weaponData);
    }

    private WeaponData GetWeaponData(int index)
    {
        // Tìm WeaponData từ danh sách các GameObject con
        Transform weaponTransform = contentWeaponPanel.GetChild(index);
        return weaponTransform.GetComponent<WeaponData>();
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

        // Gọi sự kiện thông báo vũ khí đã snap
        WeaponData snappedWeaponData = GetWeaponData(currentItem);
        NotifySnapChanged(snappedWeaponData);
    }

    // Cập nhật trạng thái của các nút
    private void UpdateButtonStates()
    {
        // Ẩn/hiện nút Next nếu đang ở cuối danh sách
        nextButton.gameObject.SetActive(currentItem < ItemNames.Length - 1);

        // Ẩn/hiện nút Previous nếu đang ở đầu danh sách
        previousButton.gameObject.SetActive(currentItem > 0);
    }
}
