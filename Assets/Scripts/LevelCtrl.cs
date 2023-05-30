using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelCtrl : MonoBehaviour
{
    private GameSettings m_settings;

    private float m_timer;
    private float m_spawnTimer;
    private LevelInfo m_currentLevel;

    private void Start()
    {
        m_settings = GameCtrl.Inst.Settings;
        m_currentLevel = m_settings.levelInfoList[0];
        _ResetSpawnTimer();
        
    }

    private void Update()
    {
        if (GameCtrl.Inst.GameEnd) return;
        _UpdateLevel();
        _UpdateSpawn();
    }

    private void _UpdateLevel()
    {
        m_timer += Time.deltaTime;
        foreach (var levelInfo in m_settings.levelInfoList)
        {
            if (m_timer > levelInfo.time) m_currentLevel = levelInfo;
        }
    }

    private void _UpdateSpawn()
    {
        m_spawnTimer -= Time.deltaTime;
        if (m_spawnTimer < 0)
        {
            _ResetSpawnTimer();
            var bubble = GameCtrl.Inst.Spawn<BubbleItem>("Bubble");
            var position = _GetSpawnPosition();
            var vel = (GameCtrl.Inst.Group.transform.position - position).normalized *
                      Random.Range(m_currentLevel.speedMin, m_currentLevel.speedMax);
            var color = Constants.COLOR_LIST[Random.Range(0, m_currentLevel.colorCount)];
            bubble.Init(position, color, vel);
        }
    }

    private void _ResetSpawnTimer()
    {
        m_spawnTimer = Random.Range(m_currentLevel.spawnIntervalMin, m_currentLevel.spawnIntervalMax);
    }

    private Vector3 _GetSpawnPosition()
    {
        var range = Random.Range(-m_currentLevel.spawnRange * 0.5f, m_currentLevel.spawnRange * 0.5f);
        var direction = Random.value > 0.5f ? Vector3.up : Vector3.down;
        return Quaternion.Euler(new Vector3(0, 0, range)) * direction * m_settings.spawnDistance;
    }


}
