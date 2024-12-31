using UnityEngine;

public class PlayerLockMovement : MonoBehaviour
{
    private bool isLocked = false;
    private float lockTimer = 0f;
    private PlayerMovement playerMovement; 

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement script not found!");
        }
    }

    void Update()
    {
        if (isLocked)
        {
            lockTimer -= Time.deltaTime;

            if (lockTimer <= 0f)
            {
                UnlockMovement();
            }
        }
    }

    public void LockMovement(float duration)
    {
        if (playerMovement != null)
        {
            playerMovement.enabled = false; 
        }

        isLocked = true;
        lockTimer = duration;
    }

    private void UnlockMovement()
    {
        if (playerMovement != null)
        {
            playerMovement.enabled = true; 
        }

        isLocked = false;
    }
}
