using UnityEngine;

public interface DamageInterface
{
    void TakeDamage(int damage);
    bool CanBeDamaged();
    void SetCanBeDamaged(bool value);
}

public interface DamagePlayerInterface
{
    void DamagePlayer(int damage);
}
