using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorItem : MonoBehaviour
{
    [SerializeField] private EColor _color;
    public EColor Color
    {
        get => _color;
        set
        {
            _color = value;
            _Refresh();
        }
    }

    private Transform m_skins;

    protected virtual void Awake()
    {
        m_skins = transform.FindDeepChild("m_Skins");
    }

    protected virtual void Start()
    {
        _Refresh();
    }

    private void _Refresh()
    {
        if(m_skins == null) return;
        foreach (Transform skin in m_skins)
        {
            skin.gameObject.SetActive(skin.name == Color.ToString());
        }
    }

    private void OnValidate()
    {
        m_skins = transform.FindDeepChild("m_Skins");
        _Refresh();
    }
}
