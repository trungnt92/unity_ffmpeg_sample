using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Min_Max_Slider;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class MenuManager : MonoBehaviour
{
    public GameObject ProcessingPanel;
    public GameObject TrimPanel;

    private VideoPlayer mVideoPlayer;
    public MinMaxSlider Slider;

    public RawImage firstThumbsnail;
    public RawImage lastThumbsnail;
    private Texture2D mThumbnail;

    private bool thumbnailOk = false;

    private float oldMinVal;
    private float oldMaxVal;

    private string inputPath;

    private void Awake()
    {
        mVideoPlayer = GetComponent<VideoPlayer>();
        mVideoPlayer.frameReady += FrameReady;
        mVideoPlayer.sendFrameReadyEvents = true;
    }

    void FrameReady(VideoPlayer vp, long frameIndex)
    {
        mVideoPlayer.Pause();
        Debug.Log("FrameReady " + frameIndex);

        Get2DTexture();

        thumbnailOk = true;
    }

    private Texture2D Get2DTexture()
    {
        mThumbnail = new Texture2D(mVideoPlayer.texture.width, mVideoPlayer.texture.height, TextureFormat.RGBA32, false);
        RenderTexture cTexture = RenderTexture.active;
        RenderTexture rTexture = new RenderTexture(mVideoPlayer.texture.width, mVideoPlayer.texture.height, 32);
        UnityEngine.Graphics.Blit(mVideoPlayer.texture, rTexture);

        RenderTexture.active = rTexture;
        mThumbnail.ReadPixels(new Rect(0, 0, rTexture.width, rTexture.height), 0, 0);
        mThumbnail.Apply();

        UnityEngine.Color[] pixels = mThumbnail.GetPixels();

        RenderTexture.active = cTexture;

        rTexture.Release();

        return mThumbnail;
    }

    public void PickVideo()
    {
        NativeGallery.Permission permission = NativeGallery.GetVideoFromGallery((path) =>
        {
            Debug.Log("Video path: " + path);
            if (path != null)
            {
                SelectVideo(path);
            }
        }, "Select a video");

        Debug.Log("Permission result: " + permission);

    }

    public void GrayScaleVideo()
    {
        StartCoroutine(ProcessVideo());
    }

    public void ShowTrimMenu()
    {
        TrimPanel.SetActive(true);
    }


    void SelectVideo(string path)
    {
        // copy file input
        var extension = Path.GetExtension(path);

        var inputFilePath = Path.Combine(Application.persistentDataPath, "input" + extension);
        File.Copy(path, inputFilePath, true);
        Debug.Log("Copy path: " + inputFilePath);
        inputPath = inputFilePath;


        mVideoPlayer.url = inputFilePath;
        mVideoPlayer.Prepare();
        StartCoroutine(PrepareVideo());
    }

    IEnumerator PrepareVideo()
    {
        while (!mVideoPlayer.isPrepared)
        {
            yield return null;
        }

        mVideoPlayer.Pause();

        Debug.Log("PrepareVideo");
        thumbnailOk = false;
        mVideoPlayer.Play();
        mVideoPlayer.frame = 0;

        // wait till get first frame
        while (!thumbnailOk)
        {
            yield return null;
        }
        Debug.Log("Update first thumbnail");
        firstThumbsnail.texture = mThumbnail;

        double totalTime = mVideoPlayer.frameCount / mVideoPlayer.frameRate;
        thumbnailOk = false;
        mVideoPlayer.Play();
        mVideoPlayer.time = (int)totalTime;

        // wait till get last frame
        while (!thumbnailOk)
        {
            yield return null;
        }
        Debug.Log("Update last thumbnail");
        lastThumbsnail.texture = mThumbnail;

        
        Debug.Log("Frame total " + mVideoPlayer.frameCount);
        Slider.SetLimits(0, (int)totalTime);
        Slider.SetValues(0, (int)totalTime, false);

        oldMinVal = 0;
        oldMaxVal = (int)totalTime;
    }

    public void SliderChanged()
    {
        if (Slider.Values.minValue != oldMinVal || Slider.Values.maxValue != oldMaxVal)
        {
            Debug.Log("Update values min: " + Slider.Values.minValue + " max: " + Slider.Values.maxValue);
            oldMinVal = Slider.Values.minValue;
            oldMaxVal = Slider.Values.maxValue;
            StartCoroutine(UpdateThumbnails());
        }
    }


    IEnumerator UpdateThumbnails()
    {
        thumbnailOk = false;
        mVideoPlayer.Play();
        mVideoPlayer.time = (long)(Slider.Values.minValue);
        

        // wait till get first frame
        while (!thumbnailOk)
        {
            yield return null;
        }
        Debug.Log("Update first thumbnail");
        firstThumbsnail.texture = mThumbnail;

        thumbnailOk = false;
        mVideoPlayer.Play();
        mVideoPlayer.time = (long)(Slider.Values.maxValue);

        // wait till get first frame
        while (!thumbnailOk)
        {
            yield return null;
        }
        Debug.Log("Update last thumbnail");
        lastThumbsnail.texture = mThumbnail;
    }

    IEnumerator ProcessVideo()
    {
        ProcessingPanel.SetActive(true);

        yield return null;

        var extension = Path.GetExtension(inputPath);

        var outputFilePath = Path.Combine(Application.persistentDataPath, "output" + extension);

        string command = string.Format("-i {0} -y -vf format=gray {1}", inputPath, outputFilePath);

        FfmpegHelper.Excute(command);

        Debug.Log("Output file: " + outputFilePath);

        ProcessingPanel.SetActive(false);

        NativeGallery.SaveVideoToGallery(outputFilePath, "FFmpeg", "grayOutput" + extension);

        Handheld.PlayFullScreenMovie("file://" + outputFilePath);
    }

    public void TrimVideo()
    {
        TimeSpan startTime = TimeSpan.FromSeconds((int)Slider.Values.minValue);
        string startTimeText = startTime.ToString(@"hh\:mm\:ss");

        TimeSpan endTime = TimeSpan.FromSeconds((int)Slider.Values.maxValue);
        string endTimeText = endTime.ToString(@"hh\:mm\:ss");

        var extension = Path.GetExtension(inputPath);
        var outputFilePath = Path.Combine(Application.persistentDataPath, "trimOutput" + extension);

        string command = string.Format("-ss {0} -to {1} -i {2} -y -c copy {3}",
            startTimeText, endTimeText, inputPath, outputFilePath);

        FfmpegHelper.Excute(command);

        NativeGallery.SaveVideoToGallery(outputFilePath, "FFmpeg", "trimOutput" + extension);

        Handheld.PlayFullScreenMovie("file://" + outputFilePath);
    }

    public void Clear()
    {
        TrimPanel.SetActive(false);
    }
}
