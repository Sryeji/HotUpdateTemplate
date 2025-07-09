using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Settings;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GameLaunchController : MonoBehaviour
{
    [Header("Fields")]
    public string hotUpdateLabel = "HotUpdate";
    public string hotUpdateDLLKey = "HotUpdate.dll";
    public string localizationTableName;
    public string pingUrl;
    public int pingTimeOut;
    public float stepInterval;
    

    private void Start()
    {
        SplashWindow.Instance.PlaySplash(() =>
        {
            StartCoroutine(Launch());
        });
    }
    

    private IEnumerator Launch()
    {
        //1.初始化
        HotUpdateWindow.Instance.Init();
        HotUpdateWindow.Instance.SetMessage(true, GetL10NString("msg_Init"));
        
        var initHandle1 = Addressables.InitializeAsync(false);
        HotUpdateWindow.Instance.UpdateProgressFromHandle(initHandle1, false);
        yield return initHandle1;
        
        var initHandle2 = LocalizationSettings.InitializationOperation;
        HotUpdateWindow.Instance.UpdateProgressFromHandle(initHandle2, false);
        yield return initHandle2;
        
        yield return new WaitForSecondsRealtime(stepInterval);
        
        //2.检查网络连接情况
        
        HotUpdateWindow.Instance.SetMessage(true, GetL10NString("msg_CheckNetwork"));
        
        bool isNetworkValid = true;
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            //物理层面没有网络连接
            isNetworkValid = false;
        }
        else
        {
            //连接超时
            using (UnityWebRequest request = UnityWebRequest.Get(pingUrl))
            {
                request.timeout = pingTimeOut;
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    isNetworkValid = false;
                }
            }
        }

        if (!isNetworkValid)
        {
            PopupWindow.Instance
                .SetContent(GetL10NString("msg_NetworkError"))
                .SetButton1(GetL10NString("btn_Quit"), () => Application.Quit())
                .SetButton2("",null)
                .SetPopupActive(true);
            yield break;
        }
        
        //3.检查并更新目录
        HotUpdateWindow.Instance.SetMessage(true, GetL10NString("msg_CheckCatalog"));
        var checkHandle = Addressables.CheckForCatalogUpdates(false);
        HotUpdateWindow.Instance.UpdateProgressFromHandle(checkHandle, false);
        yield return checkHandle;
        yield return new WaitForSecondsRealtime(stepInterval);
        if (checkHandle.Status == AsyncOperationStatus.Succeeded && checkHandle.Result.Count > 0)
        {
            HotUpdateWindow.Instance.SetMessage(true, string.Format(GetL10NString("msg_UpdateCatalog"), checkHandle.Result.Count));
            var updateHandle = Addressables.UpdateCatalogs(checkHandle.Result);
            HotUpdateWindow.Instance.UpdateProgressFromHandle(updateHandle, false);
            yield return updateHandle;
            yield return new WaitForSecondsRealtime(stepInterval);
        }
        Addressables.Release(checkHandle);
        
        //4.计算下载量
        HotUpdateWindow.Instance.SetMessage(true, GetL10NString("msg_GetDownloadSize"));
        var sizeHandle = Addressables.GetDownloadSizeAsync(hotUpdateLabel);
        HotUpdateWindow.Instance.UpdateProgressFromHandle(sizeHandle, false);
        yield return sizeHandle;
        yield return new WaitForSecondsRealtime(stepInterval);
        long downloadSize = sizeHandle.Result;
        Addressables.Release(sizeHandle);
        
        if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
        {
            bool isUserConfirmed = false;
            PopupWindow.Instance
                .SetContent(GetL10NString("msg_CarrierData"))
                .SetButton1(GetL10NString("btn_Continue"), () => isUserConfirmed = true)
                .SetButton2(GetL10NString("btn_Quit"), () => Application.Quit())
                .SetPopupActive(true);
            yield return new WaitUntil(() => isUserConfirmed);
        }

        //5.开始下载资源
        if (downloadSize > 0)
        {
            //资源欠缺，开始下载资源包
            HotUpdateWindow.Instance.SetMessage(true, GetL10NString("msg_Downloading"));

            var downloadHandle = Addressables.DownloadDependenciesAsync(hotUpdateLabel);
            HotUpdateWindow.Instance.UpdateProgressFromHandle(downloadHandle, true);
            yield return downloadHandle;
            yield return new WaitForSecondsRealtime(stepInterval);
            if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                HotUpdateWindow.Instance.SetMessage(true, GetL10NString("msg_DownloadSucceeded"));
            }
            else
            {
                Debug.LogError("资源下载失败：" + downloadHandle.OperationException?.Message);
                HotUpdateWindow.Instance.SetMessage(true, GetL10NString("msg_DownloadFailed"));
                
                bool retry = false;
                PopupWindow.Instance
                    .SetContent(GetL10NString("msg_Retry"))
                    .SetButton1(GetL10NString("btn_Retry"), () => retry = true)
                    .SetButton2(GetL10NString("btn_Quit"), () => Application.Quit())
                    .SetPopupActive(true);
    
                yield return new WaitUntil(() => retry);
                yield return Launch();
                yield break;
            }
            Addressables.Release(downloadHandle);
        }
        else
        {
            HotUpdateWindow.Instance.SetMessage(true, GetL10NString("msg_LatestBundles"));
        }
        
        yield return new WaitForSecondsRealtime(stepInterval);
        
        HotUpdateWindow.Instance.SetMessage(true, GetL10NString("msg_ReadyToStart"));
        
        //加载热更新程序集
        var dllHandle = Addressables.LoadAssetAsync<TextAsset>(hotUpdateDLLKey);
        dllHandle.Completed += handle =>
        {
            Assembly assembly = Assembly.Load(handle.Result.bytes);
            Type type = assembly.GetType("HotUpdateBridge");
            type.GetMethod("Run").Invoke(null, null);
        };
        yield return dllHandle;
    }

    private string GetL10NString(string key)
        => LocalizationSettings.StringDatabase.GetLocalizedString(localizationTableName, key);
}
