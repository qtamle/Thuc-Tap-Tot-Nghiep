using UnityEngine;

public interface ISupplyActive 
{

    void Active();
    void CanActive();
    bool IsReady(); // Kiểm tra xem supply có thể kích hoạt không
    float CooldownTime { get; } // Thời gian cooldown
}
