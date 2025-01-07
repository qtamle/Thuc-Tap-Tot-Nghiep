using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; 

public class Magnet : MonoBehaviour
{
    public SupplyData supplyData;
    [SerializeField] private bool isActive = true;
    [SerializeField] private float cooldownTime = 5f;
    [SerializeField] private float orbMoveToPlayerSpeed = 10f;

    public float CooldownTime => cooldownTime;
    private string playerTag = "Player";
    private string coinTag = "Coin";

    private Transform playerTransform;

    private void Awake()
    {
        FindPlayer();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        isActive = true;
        Active();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindPlayer();
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            playerTransform = player.transform;
            Debug.Log("Player found in scene: " + player.name);
        }
        else
        {
            Debug.LogError("Player not found! Make sure the Player has the correct tag.");

            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer != -1)
            {
                GameObject[] objectsInLayer = FindObjectsOfType<GameObject>();
                foreach (GameObject obj in objectsInLayer)
                {
                    if (obj.layer == playerLayer)
                    {
                        playerTransform = obj.transform;
                        Debug.Log($"Player found using layer: {obj.name}");
                        break;
                    }
                }
            }
            if (playerTransform == null)
            {
                Debug.LogError("Player not found! Make sure the Player has the correct tag or layer.");
            }
        }
    }

    public void Active()
    {
        if (!IsReady())
        {
            return;
        }

        isActive = false;

        StartCoroutine(HookCoinsContinuously());
        StartCoroutine(CooldownRoutine());
    }

    private IEnumerator HookCoinsContinuously()
    {
        while (true)
        {
            if (isActive)
            {
                GameObject[] allCoins = GameObject.FindGameObjectsWithTag(coinTag);
                foreach (GameObject coin in allCoins)
                {
                    StartCoroutine(MoveCoinToPlayer(coin));
                }
            }

            yield return new WaitForSeconds(2f);
        }
    }

    private IEnumerator MoveCoinToPlayer(GameObject coin)
    {
        Rigidbody2D coinRb = coin.GetComponent<Rigidbody2D>();
        if (coinRb != null)
        {
            if (coinRb.bodyType == RigidbodyType2D.Kinematic)
            {
                coinRb.bodyType = RigidbodyType2D.Kinematic;
                coinRb.gravityScale = 0f;
            }

            while (coin != null && playerTransform != null)
            {
                Debug.Log($"Player Position: {playerTransform.position}, Coin Position: {coin.transform.position}");

                coin.transform.position = Vector3.MoveTowards(
                    coin.transform.position,
                    playerTransform.position,
                    Time.deltaTime * orbMoveToPlayerSpeed
                );

                if (Vector3.Distance(coin.transform.position, playerTransform.position) < 0.5f)
                {
                    CoinsScript coinScript = coin.GetComponent<CoinsScript>();
                    if (coinScript != null)
                    {
                        coinScript.CollectCoin();
                    }
                    yield break;
                }

                yield return null;
            }
        }
        else
        {
            Debug.LogWarning($"GameObject {coin.name} không có Rigidbody2D!");
        }
    }

    public void CanActive()
    {
        isActive = true;
    }

    public bool IsReady()
    {
        return isActive;
    }

    private IEnumerator CooldownRoutine()
    {
        yield return new WaitForSeconds(cooldownTime);
        CanActive();
    }
}
