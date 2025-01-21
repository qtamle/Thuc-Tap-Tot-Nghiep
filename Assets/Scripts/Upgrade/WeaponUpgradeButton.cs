using UnityEngine;
using UnityEngine.UI;

public class WeaponUpgradeButton : MonoBehaviour
{
    public Button upgradeButton;  // Nút nâng cấp
    public WeaponData weaponData;  // Dữ liệu vũ khí sẽ nâng cấp khi nhấn nút

    private void Start()
    {
        // Liên kết sự kiện khi nhấn nút với hàm nâng cấp
        upgradeButton.onClick.AddListener(UpgradeWeapon);
    }

    private void UpgradeWeapon()
    {
        // Kiểm tra vũ khí có dữ liệu không và gọi hàm nâng cấp
        if (weaponData != null)
        {
            weaponData.UpgradeWeapon();
        }
    }
}
