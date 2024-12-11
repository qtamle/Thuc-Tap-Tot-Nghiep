using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipmentSlot : MonoBehaviour, IDropHandler
{
    //public Image itemImage; // Hình ảnh chiến lợi phẩm
    //public Image placeholder; // Hình nền trống
    ////public GameObject tooltip; // Tooltip hiển thị thông tin

    //private Equipment equippedItem; // Chiến lợi phẩm được gắn

    public void OnDrop(PointerEventData eventData)
    {
        if(transform.childCount == 0)
        {
            GameObject dropped = eventData.pointerDrag;
            DraggableEquipment draggableEquipment = dropped.GetComponent<DraggableEquipment>();
            draggableEquipment.parentAfterDrag = transform;
        }
        
    }

    //public void SetEquipment(Equipment newItem)
    //{
    //    equippedItem = newItem;

    //    if (equippedItem != null)
    //    {
    //        itemImage.sprite = equippedItem.equipmentSprite;
    //        itemImage.enabled = true;
    //        placeholder.enabled = false;
    //    }
    //    else
    //    {
    //        itemImage.enabled = false;
    //        placeholder.enabled = true;
    //    }
    //}

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
