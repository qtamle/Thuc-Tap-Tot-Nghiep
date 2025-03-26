using System.Collections;
using UnityEngine;

public class CloneSkill : MonoBehaviour
{
    public float dashSpeed = 5f;

    public IEnumerator DashInDirection(Vector3 direction, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position += direction * dashSpeed * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    public IEnumerator BlinkEffect(GameObject clone, float blinkDuration, float blinkInterval)
    {
        SpriteRenderer spriteRenderer = clone.GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            float elapsedTime = 0f;
            bool isVisible = true;

            while (elapsedTime < blinkDuration)
            {
                spriteRenderer.enabled = isVisible;

                isVisible = !isVisible;

                elapsedTime += blinkInterval;
                yield return new WaitForSeconds(blinkInterval);
            }

            spriteRenderer.enabled = true;
        }
    }
}
