using UnityEngine;

public class SupplyPickup : MonoBehaviour
{
    public SupplyData supplyData;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) // Kiểm tra nếu người chơi chạm vào supply
        {
            Debug.Log($"Player đã nhặt {supplyData.supplyName}.");

            // Loại bỏ supply khỏi danh sách trong SupplyManager
            SupplyManager.Instance.RemoveSupply(supplyData);

            SupplyManager.Instance.AddToInventory(supplyData);
            // Thực hiện các hành động liên quan khác (ví dụ: tăng máu, mana,...)
            ApplyEffect();

            //// Hủy đối tượng supply
            Destroy(gameObject);
        }
    }

    private void ApplyEffect()
    {
        // Thêm hiệu ứng khi nhặt Supply, nếu có (ví dụ tăng máu, mana,...)
        Debug.Log($"Đã áp dụng hiệu ứng của {supplyData.supplyName}.");
    }
}
