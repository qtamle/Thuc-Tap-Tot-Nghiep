using UnityEngine;

public class SupplyPickup : MonoBehaviour
{
    public SupplyData supplyData;

    private void OnMouseDown() // Giả sử dùng tương tác chuột, có thể thay bằng phương pháp khác
    {
        if (SupplyManager.Instance != null)
        {
            SupplyManager.Instance.AddToInventory(supplyData);
            Destroy(gameObject); // Xóa vật phẩm khỏi scene
        }
    }
}
