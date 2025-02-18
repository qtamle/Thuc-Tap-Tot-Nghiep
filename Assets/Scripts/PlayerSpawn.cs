using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawn : MonoBehaviour
{
    public Transform spawnTransform;
    private bool hasSpawn = false;

    private void Start()
    {
        if (!hasSpawn)
        {
            GameObject player = FindPlayer();

            if (player != null)
            {
                player.transform.position = spawnTransform.position;
                hasSpawn = true;
                StartCoroutine(DisablePlayerMovement(player, 3f));
            }
        }
    }

    GameObject FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) return player;

        foreach (GameObject obj in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (obj.CompareTag("Player"))
                return obj;
        }

        GameObject[] allObjects = FindObjectsOfType<GameObject>(true);
        foreach (GameObject obj in allObjects)
        {
            if (obj.CompareTag("Player") || obj.layer == LayerMask.NameToLayer("Player"))
                return obj;
        }

        return null;
    }

    IEnumerator DisablePlayerMovement(GameObject player, float duration)
    {
        var playerController = player.GetComponent<PlayerMovement>();
        if (playerController != null)
        {
            playerController.isMovementLocked = true;
            playerController.enabled = false;
            yield return new WaitForSeconds(duration);
            playerController.enabled = true;
        }
    }
}
