using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SplashWindow : MonoBehaviour
{
    public static SplashWindow Instance;

    [Header("Fields")] 
    public float fadeInDuration;
    public float fadeOutDuration;
    public float lightHoldDuration;
    public float darkHoldDuration;

    [Header("References")]
    public GameObject root;
    public List<GameObject> splashObjects = new List<GameObject>();
    public Image mask;

    private void Awake()
    {
        Instance = this;
    }

    public void PlaySplash(UnityAction callback = null, bool isAutoClose = true)
    {
        StartCoroutine(SplashCoro(callback, isAutoClose));
    }

    IEnumerator SplashCoro(UnityAction callback = null, bool isAutoClose = true)
    {
        //init
        root.SetActive(true);
        splashObjects.ForEach(x => x.SetActive(false));
        mask.color = Color.black;

        for (int i = 0; i < splashObjects.Count; i++)
        {
            //update
            if (i > 0)
                splashObjects[i - 1].SetActive(false);
            splashObjects[i].SetActive(true);

            //darkHold
            yield return new WaitForSecondsRealtime(darkHoldDuration);

            //fadeIn
            yield return mask.DOFade(0, fadeInDuration).WaitForCompletion();

            //lightHold
            yield return new WaitForSecondsRealtime(lightHoldDuration);

            //fadeOut
            yield return mask.DOFade(1, fadeOutDuration).WaitForCompletion();
        }

        yield return new WaitForSecondsRealtime(darkHoldDuration);

        callback?.Invoke();
        
        root.SetActive(!isAutoClose);
    }
}
