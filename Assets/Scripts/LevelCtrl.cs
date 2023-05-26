using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelCtrl : MonoBehaviour
{
    private GameSettings m_settings;

    private float m_timer;
    
    private void Start()
    {
        m_settings = GameCtrl.Inst.Settings;
    }

    private void Update()
    {
        m_timer += Time.deltaTime;
        if (m_timer > 1)
        {
            m_timer = 0;
            var bubble = GameCtrl.Inst.Spawn<BubbleItem>("Bubble");
            bubble.Init(Vector3.right * 10, Constants.COLOR_LIST.Random(), Vector2.left * 2);
        }
    }
}
