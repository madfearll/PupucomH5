using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public enum EColor
{
    None,
    Red,
    Green,
    Yellow,
    Blue,
}

public interface IPoolable
{
    void OnSpawned();
    void OnDespawned();
}

public class GameCtrl : MonoBehaviour
{
    [SerializeField] private GameSettings _settings;
    [SerializeField] private CinemachineImpulseSource _impulse;
    [SerializeField] private TMP_Text _score;
    [SerializeField] private CinemachineVirtualCamera _cmDead;
    [SerializeField] private SpriteRenderer _boardSprite;
    [SerializeField] private Transform _cross;

    public static GameCtrl Inst { get; private set; }

    public GameSettings Settings => _settings;
    public BubbleGroup Group { get; set; }
    private int m_score;
    public int Score
    {
        get => m_score;
        set
        {
            var score = m_score;
            _score.DOKill();
            DOTween.To(() => score, s =>
            {
                score = s;
                _score.text = $"SCORE: {score:N0}";
            }, value, 0.5f).SetEase(Ease.Linear).SetTarget(_score);
            m_score = value;
        }
    }
    public int Combo { get; set; }

    public int ComboScore => _GetComboInfo().score;
    public Color ComboColor => _GetComboInfo().color;
    public bool GameEnd { get; private set; }

    private Transform m_pool;

    private void Awake()
    {
        Inst = this;
        m_pool = new GameObject("Pool").transform;
        
    }

    private void Start()
    {
        Time.timeScale = 1;
        
        _score.text = $"SCORE: {m_score:N0}";

        _boardSprite.DOFade(0f, 0.6f).SetLoops(-1, LoopType.Yoyo);
        // _boardSprite.transform.DOLocalRotate(new Vector3(0, 0, 360f), 10f, RotateMode.LocalAxisAdd).SetEase(Ease.Linear)
        //     .SetLoops(-1, LoopType.Restart);

        _cross.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (GameEnd) return;
        _UpdateGameState();
    }

    private void _UpdateGameState()
    {
        var showBoarder = false;
        foreach (var bubble in Group.BubbleList)
        {
            var radius = (bubble.transform.position - Group.transform.position).magnitude;
            if (radius > _settings.boarderRadius)
            {
                //不在屏幕内，游戏结束
                GameEnd = true;
                Time.timeScale = 0.5f;
                _cmDead.Follow = bubble.transform;
                _cmDead.Priority = 20;
                _cross.gameObject.SetActive(true);
                _cross.position = bubble.transform.position;
            }
            else
            {
                if (_settings.boarderRadius - radius < 2f)
                {
                    showBoarder = true;
                }
            }
        }

        _boardSprite.gameObject.SetActive(showBoarder);
    }

    public GameObject Spawn(string prefabName, Transform parent = null)
    {
        var prefab = Resources.Load(prefabName);
        var poolable = prefab.GetComponent<IPoolable>();
        if (poolable == null)
        {
            var go = Instantiate(prefab, parent) as GameObject;
            go.name = prefabName;
            return go;
        }
        else
        {
            var go = m_pool.Find(prefabName)?.gameObject;
            if (go == null)
            {
                go = Instantiate(prefab, parent) as GameObject;
                go.name = prefabName;
            }

            go.transform.SetParent(parent);
            go.SetActive(true);
            go.GetComponent<IPoolable>().OnSpawned();
            return go;
        }
    }

    public void Despawn(GameObject go)
    {
        var poolable = go.GetComponent<IPoolable>();
        if (poolable == null)
        {
            Destroy(go);
        }
        else
        {
            go.SetActive(false);
            go.transform.SetParent(m_pool);
            poolable.OnDespawned();
        }
    }

    public T Spawn<T>(string prefabName, Transform parent = null)
    {
        var go = Spawn(prefabName, parent);
        if (go == null) return default;
        return go.GetComponent<T>();
    }

    public void Despawn<T>(T component) where T : Component
    {
        Despawn(component.gameObject);
    }

    public void PlaySfx(string sfx)
    {
        
    }
    
    public void ApplyCameraImpulse(Vector2 vel)
    {
        _impulse.GenerateImpulseWithVelocity(vel);
    }

    private ComboInfo _GetComboInfo()
    {
        ComboInfo comboInfo = null;
        foreach (var ci in _settings.comboInfoList)
        {
            if (Combo >= ci.combo) comboInfo = ci;
        }

        return comboInfo;
    }
}
