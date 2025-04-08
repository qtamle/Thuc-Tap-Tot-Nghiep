using System.Collections;
using System.Threading.Tasks; // Thêm namespace này
using UnityEngine;
using UnityEngine.Events; // Có thể cần nếu dùng callback Action

public class FadingScript : MonoBehaviour
{
    public static FadingScript Instance { get; private set; }

    [SerializeField]
    private CanvasGroup canvasGroup;

    [SerializeField]
    private float fadeDuration = 0.5f; // Giảm thời gian fade để test nhanh hơn (ví dụ 1 giây)

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            canvasGroup.alpha = 0f; // Đảm bảo bắt đầu trong suốt
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- Các hàm FadeIn/FadeOut cũ (sử dụng Coroutine) ---
    public void FadeIn()
    {
        StartCoroutine(FadeCanvasGroupCoroutine(canvasGroup, canvasGroup.alpha, 0f, fadeDuration));
    }

    public void FadeOut()
    {
        StartCoroutine(FadeCanvasGroupCoroutine(canvasGroup, canvasGroup.alpha, 1f, fadeDuration));
    }

    private IEnumerator FadeCanvasGroupCoroutine(
        CanvasGroup cg,
        float start,
        float end,
        float duration
    )
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(start, end, elapsedTime / duration);
            cg.alpha = alpha;
            yield return null;
        }
        cg.alpha = end;
    }

    // --- Các hàm Fade Async mới ---

    // Hàm Fade Out trả về Task để có thể await
    public async Task FadeOutAsync()
    {
        await FadeCanvasGroupAsync(canvasGroup, canvasGroup.alpha, 1f, fadeDuration);
    }

    // Hàm Fade In trả về Task (nếu cần)
    public async Task FadeInAsync()
    {
        await FadeCanvasGroupAsync(canvasGroup, canvasGroup.alpha, 0f, fadeDuration);
    }

    // Hàm thực hiện fade trả về Task
    private async Task FadeCanvasGroupAsync(CanvasGroup cg, float start, float end, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            // Đảm bảo CanvasGroup được kích hoạt để tương tác và hiển thị
            cg.interactable = true;
            cg.blocksRaycasts = true;

            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(start, end, elapsedTime / duration);
            cg.alpha = alpha;
            await Task.Yield(); // Tương đương yield return null trong Coroutine, cho phép frame khác chạy
        }
        cg.alpha = end; // Đảm bảo giá trị cuối cùng

        // Nếu fade out hoàn toàn (end = 1), có thể tắt tương tác
        if (end >= 1f)
        {
            // Giữ interactable và blocksRaycasts = true để màn hình đen che phủ hoàn toàn
        }
        // Nếu fade in hoàn toàn (end = 0), tắt tương tác để không chặn click
        else if (end <= 0f)
        {
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }
    }
}
