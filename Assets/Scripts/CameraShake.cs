using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private bool isShakeEnabled = true;
    private bool isShaking = false; 

    private Camera mainCamera;
    private Transform cameraParent;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        mainCamera = Camera.main; 
        cameraParent = new GameObject("CameraParent").transform;
        transform.SetParent(cameraParent); 
    }

    private void Start()
    {
        originalPosition = mainCamera.transform.localPosition;
        originalRotation = mainCamera.transform.localRotation;
    }

    private void Update()
    {
        if (!isShaking)
        {
            mainCamera.transform.localPosition = originalPosition; 
            mainCamera.transform.localRotation = originalRotation; 
        }
    }

    public IEnumerator Shake(float duration, float magnitude, float zMagnitude, float rotationMagnitude)
    {
        if (!isShakeEnabled) yield break;

        isShaking = true;

        originalPosition = mainCamera.transform.localPosition;
        originalRotation = mainCamera.transform.localRotation;

        float elapsed = 0f;
        float seedX = Random.value * 100f;
        float seedY = Random.value * 100f;
        float seedZ = Random.value * 100f;
        float seedRot = Random.value * 100f;

        while (elapsed < duration)
        {
            float damper = 1f - (elapsed / duration);
            float x = (Mathf.PerlinNoise(seedX, elapsed * 10f) - 0.5f) * 2f * magnitude * damper;
            float y = (Mathf.PerlinNoise(seedY, elapsed * 10f) - 0.5f) * 2f * magnitude * damper;
            float z = (Mathf.PerlinNoise(seedZ, elapsed * 10f) - 0.5f) * 2f * zMagnitude * damper;
            float rotZ = (Mathf.PerlinNoise(seedRot, elapsed * 10f) - 0.5f) * 2f * rotationMagnitude * damper;

            // Di chuyển vị trí camera chính
            mainCamera.transform.localPosition = Vector3.Lerp(mainCamera.transform.localPosition, originalPosition + new Vector3(x, y, z), 0.5f);
            mainCamera.transform.localRotation = Quaternion.RotateTowards(mainCamera.transform.localRotation, Quaternion.Euler(0, 0, rotZ), 5f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.localPosition = originalPosition;
        mainCamera.transform.localRotation = originalRotation;

        isShaking = false;
    }

    public void StartShake(float duration, float magnitude, float zMagnitude, float rotationMagnitude)
    {
        if (!isShakeEnabled) return;

        StopAllCoroutines();
        StartCoroutine(Shake(duration, magnitude, zMagnitude, rotationMagnitude));
    }

    public void EnableShake() => isShakeEnabled = true;
    public void DisableShake() => isShakeEnabled = false;
}
