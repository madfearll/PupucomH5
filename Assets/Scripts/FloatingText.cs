using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour, IPoolable
{
    private TMP_Text m_text;
    
    private void Awake()
    {
        m_text = GetComponent<TMP_Text>();
    }

    public void OnSpawned()
    {
        
    }

    public void OnDespawned()
    {
        
    }
    
    public void Init(string text, Vector3 position, Color color)
    {
        m_text.text = text;
        transform.position = position;
        m_text.color = color;

        m_text.alpha = 0;
        m_text.DOFade(1, 0.2f).OnComplete(() =>
        {
            m_text.DOFade(0, 0.25f).SetDelay(2f);
        });
        transform.DOMove(position + Vector3.up * 2f, 3f).OnComplete(() =>
        {
            GameCtrl.Inst.Despawn(this);
        });
    }
}
