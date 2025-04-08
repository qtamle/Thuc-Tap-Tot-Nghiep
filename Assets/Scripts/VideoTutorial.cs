using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;

public class VideoTutorial : MonoBehaviour
{
    public GameObject video;           
    public Button showVideo;         
    public Button hideVideo;
    private VideoPlayer videoPlayer;  
    private bool isShow;

    private void Start()
    {
        video.SetActive(false);
        videoPlayer = GetComponent<VideoPlayer>();
        showVideo.onClick.AddListener(ShowHideVideo);
        hideVideo.onClick.AddListener(ShowHideVideo);
    }

    private void ShowHideVideo()
    {
        isShow = !isShow;
        video.SetActive(isShow);

        if (isShow)
        {
            StartCoroutine(PlayAfterDelay(0.5f)); 
        }
        else
        {
            videoPlayer.time = 0;
            videoPlayer.frame = 0;
            videoPlayer.Stop();
        }
    }

    private IEnumerator PlayAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        videoPlayer.Play();
    }
}
