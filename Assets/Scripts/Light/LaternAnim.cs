using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LaternAnim : MonoBehaviour
{
    private Light2D light2D;

    private float interval = 1f;
    private float fadeDuration = 1.5f;

    private float originalRadius;
    private float targetRadius;

    private void Start()
    {
        light2D = GetComponent<Light2D>();
        originalRadius = light2D.pointLightOuterRadius;
        targetRadius = originalRadius + 1f;

        StartCoroutine(PulseLight());
    }

    private IEnumerator PulseLight()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);

            // Phóng to
            yield return StartCoroutine(AnimateRadius(targetRadius));

            // Thu nhỏ
            yield return StartCoroutine(AnimateRadius(originalRadius));
        }
    }

    private IEnumerator AnimateRadius(float target)
    {
        float elapsedTime = 0f;
        float startRadius = light2D.pointLightOuterRadius;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            light2D.pointLightOuterRadius = Mathf.Lerp(startRadius, target, elapsedTime / fadeDuration);
            yield return null;
        }

        light2D.pointLightOuterRadius = target;
    }
}
