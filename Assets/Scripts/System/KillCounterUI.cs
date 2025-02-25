using TMPro;
using UnityEngine;

public class KillCounterUI : MonoBehaviour
{
    public TMP_Text killCounterText;
    private int currentKillTarget;
    private int currentEnemiesKilled;

    private void Start()
    {
        if (EnemyManager.Instance != null)
        {
            currentKillTarget = EnemyManager.Instance.killTarget.Value;
            currentEnemiesKilled = EnemyManager.Instance.enemiesKilled.Value;
        }

        UpdateKillCounterUI();
    }

    private void Update()
    {
        if (EnemyManager.Instance != null)
        {
            if (currentEnemiesKilled != EnemyManager.Instance.enemiesKilled.Value)
            {
                currentEnemiesKilled = EnemyManager.Instance.enemiesKilled.Value;
                UpdateKillCounterUI();
            }
        }
    }

    private void UpdateKillCounterUI()
    {
        killCounterText.text = $"{currentKillTarget - currentEnemiesKilled}";
    }
}
