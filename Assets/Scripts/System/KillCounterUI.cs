using UnityEngine;
using TMPro; 

public class KillCounterUI : MonoBehaviour
{
    public TMP_Text killCounterText;
    private int currentKillTarget;
    private int currentEnemiesKilled;

    private void Start()
    {
        if (EnemyManager.Instance != null)
        {
            currentKillTarget = EnemyManager.Instance.killTarget;
            currentEnemiesKilled = EnemyManager.Instance.EnemiesKilled;
        }

        UpdateKillCounterUI();
    }

    private void Update()
    {
        if (EnemyManager.Instance != null)
        {
            if (currentEnemiesKilled != EnemyManager.Instance.EnemiesKilled)
            {
                currentEnemiesKilled = EnemyManager.Instance.EnemiesKilled;
                UpdateKillCounterUI();
            }
        }
    }

    private void UpdateKillCounterUI()
    {
        killCounterText.text = $"{currentKillTarget - currentEnemiesKilled}";
    }
}
