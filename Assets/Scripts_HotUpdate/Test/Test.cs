using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Test : MonoBehaviour
{
    
    void Start()
    {
        Addressables.LoadAssetAsync<GameObject>("Cube").Completed += handle =>
        {
            var obj = Instantiate(handle.Result);
            obj.name = "{An Object From Addressables]";
        };

        Debug.Log("南无阿弥陀佛");
    }
}
