using UnityEngine;
using System.Threading;
using System.Collections;

public class FrameRateManager : MonoBehaviour
{
    [Header("Frame Settings")]

    int MaxRate = 999;
    public float TargetFrameRate = 60.0f;
    float currentFrameTime;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = MaxRate;
        currentFrameTime = Time.realtimeSinceStartup;
        StartCoroutine("WaitForNextFrame");
    }

    IEnumerator WaitForNextFrame()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            currentFrameTime += 1.0f / TargetFrameRate;
            var t = Time.realtimeSinceStartup;
            var sleepTime = currentFrameTime - t - 0.01f;
            if (sleepTime > 0)
            {
                Thread.Sleep((int)(sleepTime * 1000));
                while (t < currentFrameTime)
                {
                    t = Time.realtimeSinceStartup;
                }
            }
        }
    }
}
