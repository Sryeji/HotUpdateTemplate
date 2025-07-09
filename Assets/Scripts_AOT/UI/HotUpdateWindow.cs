using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class HotUpdateWindow : MonoBehaviour
{
    public static HotUpdateWindow  Instance;

    [Header("References")] 
    public GameObject root;
    public RectTransform progressUnder;
    public RectTransform progressFill;
    public TextMeshProUGUI msgLeft;
    public TextMeshProUGUI msgRight;


    private void Awake()
    {
        Instance = this;
    }

    public HotUpdateWindow Init()
    {
        root.SetActive(true);
        SetMessage(true, "");
        SetMessage(false, "");
        SetProgress(0);
        return this;
    }
    
    public HotUpdateWindow SetMessage(bool isLeft, string msg)
    {
        (isLeft ? msgLeft : msgRight).text = msg;
        return this;
    }
    
    public HotUpdateWindow SetProgress(float value)
    {
        Vector2 size = progressFill.sizeDelta;
        size.x = progressUnder.rect.width * Mathf.Clamp01(value);
        progressFill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
        return this;
    }

    public HotUpdateWindow UpdateProgressFromHandle(AsyncOperationHandle handle, bool isDownload)
    {
        StopAllCoroutines();
        StartCoroutine(UpdateProgressFromHandleCoro(handle, isDownload));
        return this;
    }
    private IEnumerator UpdateProgressFromHandleCoro(AsyncOperationHandle handle, bool isDownload)
    {
        float lastTime = Time.time;
        if (isDownload)
        {
            var status = handle.GetDownloadStatus();
            long lastBytes = status.DownloadedBytes;
            while (!status.IsDone)
            {
                status = handle.GetDownloadStatus();
                //计算下载速度
                long deltaDownloadBytes = status.DownloadedBytes - lastBytes;
                float deltaTime = Time.time - lastTime;
                
                lastBytes = status.DownloadedBytes;
                lastTime = Time.time;

                string speedStr = FormatByteSize((long)(deltaDownloadBytes / deltaTime)) + "/S";
                
                SetProgress(status.Percent);
                SetMessage(false, $"{speedStr}  {FormatByteSize(status.DownloadedBytes)}/{FormatByteSize(status.TotalBytes)}  {FormatPercentage(status.Percent)}");
                yield return null;
            }
            SetProgress(1f);
            SetMessage(false, "100.00%");
        }
        else
        {
            // 非下载类异步：只用 handle.PercentComplete 就够了
            while (!handle.IsDone)
            {
                SetProgress(handle.PercentComplete);
                SetMessage(false, $"{FormatPercentage(handle.PercentComplete)}");
                yield return null;
            }

            SetProgress(1f);
            SetMessage(false, "100.00%");
        }
    }
    
    private string FormatByteSize(long bytes)
    {
        // 定义单位数组
        string[] units = { "B", "KB", "MB", "GB", "TB" };
    
        // 如果字节数小于1，直接返回 0B
        if (bytes < 1)
        {
            return "0B";
        }

        // 计算单位下标
        int unitIndex = 0;
        double sizeInUnits = bytes;

        while (sizeInUnits >= 1024 && unitIndex < units.Length - 1)
        {
            sizeInUnits /= 1024;
            unitIndex++;
        }

        // 格式化输出，保留小数点后两位
        return string.Format("{0:F2} {1}", sizeInUnits, units[unitIndex]);
    }

    private string FormatPercentage(float value)
    {
        // Clamp 限制在 0~1 之间，避免越界
        value = Mathf.Clamp01(value);

        // 转换为百分比，保留两位小数
        return string.Format("{0:F2}%", value * 100f);
    }
}
