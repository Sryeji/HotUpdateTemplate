using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// 热更新程序集桥接类，负责启动热更新程序集
/// </summary>
public static class HotUpdateBridge
{
    
    /// <summary>
    /// 启动游戏
    /// </summary>
    public static void Run()
    {
        Debug.Log("HotUpdateBridge.Run()");
        Addressables.LoadSceneAsync("TestScene").Completed += _ =>
        {
            GameObject go = new GameObject("Test");
            go.AddComponent<Test>();
        };
    }
}
