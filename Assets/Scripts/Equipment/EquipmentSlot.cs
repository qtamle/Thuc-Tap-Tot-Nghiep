using UnityEngine;
using UnityEngine.UI;

public class EquipmentSlot : MonoBehaviour
{
    public Image itemImage; // Hình ảnh chiến lợi phẩm
    public Image placeholder; // Hình nền trống
    //public GameObject tooltip; // Tooltip hiển thị thông tin

    private Equipment equippedItem; // Chiến lợi phẩm được gắn

    public void SetEquipment(Equipment newItem)
    {
        equippedItem = newItem;

        if (equippedItem != null)
        {
            itemImage.sprite = equippedItem.equipmentSprite;
            itemImage.enabled = true;
            placeholder.enabled = false;
        }
        else
        {
            itemImage.enabled = false;
            placeholder.enabled = true;
        }
    }

    //public void ShowTooltip()
    //{
    //    if (equippedItem != null)
    //    {
    //        tooltip.SetActive(true);
    //        // Gắn thông tin vào tooltip (nếu có Text component trong tooltip)
    //    }
    //}

    //public void HideTooltip()
    //{
    //    tooltip.SetActive(false);
    //}
}
