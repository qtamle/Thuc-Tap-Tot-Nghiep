using UnityEngine;
using UnityEngine.UI;

public class WeaponManager : MonoBehaviour
{
    public WeaponData weaponData;
    public SnapToWeapon snapWeapon;

    [Header("UI Elements")]
    public Text weaponNameText;

    //public Image weaponSprite;
    public Text weaponPriceText;

    void Start()
    {
        // Đăng ký sự kiện snap
        if (snapWeapon != null)
        {
            snapWeapon.OnSnapChanged += UpdateWeaponInfo;
        }
    }

    private void OnDestroy()
    {
        if (snapWeapon != null)
        {
            snapWeapon.OnSnapChanged -= UpdateWeaponInfo;
        }
    }

    public void UpdateWeaponInfo(WeaponData newWeaponData)
    {
        if (newWeaponData == null)
            return;

        // Cập nhật UI với dữ liệu vũ khí mới
        weaponNameText.text = newWeaponData.weaponName;
        //weaponSprite. = newWeaponData.weaponSprite;
        weaponPriceText.text = $"Price: {newWeaponData.basePrice}";

        weaponData = newWeaponData;
    }
}
