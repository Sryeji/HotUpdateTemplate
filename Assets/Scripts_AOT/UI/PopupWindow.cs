using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PopupWindow : MonoBehaviour
{
    public static PopupWindow Instance;
    
    public GameObject root;
    public TextMeshProUGUI content;
    public Button btn1;
    public Button btn2;

    private void Awake()
    {
        Instance = this;
        SetPopupActive(false);
    }

    public PopupWindow SetContent(string value)
    {
        content.text = value;
        return this;
    }

    public PopupWindow SetButton1(string showText,UnityAction call,bool isAutoClose = true)
    {
        if (string.IsNullOrEmpty(showText) || call == null)
        {
            btn1.gameObject.SetActive(false);
        }
        else
        {
            btn1.gameObject.SetActive(true);
            btn1.GetComponentInChildren<TextMeshProUGUI>().text = showText;
            btn1.onClick.RemoveAllListeners();
            if (isAutoClose)
                call += () => SetPopupActive(false);
            btn1.onClick.AddListener(call);
        }
        return this;
    }
    
    public PopupWindow SetButton2(string showText,UnityAction call,bool isAutoClose = true)
    {
        if (string.IsNullOrEmpty(showText) || call == null)
        {
            btn2.gameObject.SetActive(false);
        }
        else
        {
            btn2.gameObject.SetActive(true);
            btn2.GetComponentInChildren<TextMeshProUGUI>().text = showText;
            btn2.onClick.RemoveAllListeners();
            if (isAutoClose)
                call += () => SetPopupActive(false);
            btn2.onClick.AddListener(call);
        }
        return this;
    }

    public PopupWindow SetPopupActive(bool value)
    {
        root.SetActive(value);
        return this;
    }
}
