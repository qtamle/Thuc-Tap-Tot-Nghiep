using UnityEngine;

public class EquipmentSlotsManager : MonoBehaviour
{
    public GameObject slotPrefab; // Prefab của Equipment Slot
    public Transform slotsContainer; // Container chứa các slots

    private EquipmentSlot[] slots;

    void Start()
    {
        InitializeSlots(4); // Khởi tạo 4 ô trang bị
    }

    void InitializeSlots(int slotCount)
    {
        slots = new EquipmentSlot[slotCount];

        for (int i = 0; i < slotCount; i++)
        {
            GameObject newSlot = Instantiate(slotPrefab, slotsContainer);
            slots[i] = newSlot.GetComponent<EquipmentSlot>();
            slots[i].SetEquipment(null); // Ban đầu để trống
        }
    }

    public void EquipItemToSlot(Equipment item, int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < slots.Length)
        {
            slots[slotIndex].SetEquipment(item);
        }
    }
}
