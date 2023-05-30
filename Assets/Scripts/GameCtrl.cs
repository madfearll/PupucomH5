using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum EColor
{
    None,
    Red,
    Green,
    Yellow,
    Blue,
    Purple,
    White,
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
    [SerializeField] private CinemachineVirtualCamera _cmDead;
    [SerializeField] private SpriteRenderer _boardSprite;
    [SerializeField] private Transform _cross;

    //hud ui
    [SerializeField] private GameObject _hudUI;
    [SerializeField] private TMP_Text _hudScore;

    //game end ui
    [SerializeField] private GameObject _gameEndUI;
    [SerializeField] private Button _restartButton;
    [SerializeField] private TMP_Text _rating;
    [SerializeField] private TMP_Text _gameEndScore;
    [SerializeField] private TMP_Text _bestScore;

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
            _hudScore.DOKill();
            DOTween.To(() => score, s =>
            {
                score = s;
                _hudScore.text = $"SCORE: {score:N0}";
            }, value, 0.5f).SetEase(Ease.Linear).SetTarget(_hudScore);
            m_score = value;
        }
    }

    public int HighScore
    {
        get => PlayerPrefs.GetInt("HighScore", 0);
        set => PlayerPrefs.SetInt("HighScore", value);
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
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        Time.timeScale = 1;
        
        _hudScore.text = $"SCORE: {m_score:N0}";

        _boardSprite.DOFade(0f, 0.6f).SetLoops(-1, LoopType.Yoyo);
        // _boardSprite.transform.DOLocalRotate(new Vector3(0, 0, 360f), 10f, RotateMode.LocalAxisAdd).SetEase(Ease.Linear)
        //     .SetLoops(-1, LoopType.Restart);

        _cross.gameObject.SetActive(false);

        _restartButton.onClick.AddListener(RestartGame);
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

                if (Score > HighScore)
                {
                    HighScore = Score;
                }
                
                //set game end ui
                _gameEndScore.text = $"SCORE: {Score:N0}";
                _bestScore.text = $"BEST: {HighScore:N0}";
                var ratingInfo = _GetRatingInfo(Score);
                if (ratingInfo != null)
                {
                    _rating.text = $"RATING: {ratingInfo.rating}";
                    _rating.color = ratingInfo.color;
                }

                StartCoroutine(_DoGameEndAnim());
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

    private IEnumerator _DoGameEndAnim()
    {
        yield return new WaitForSecondsRealtime(4);
        _hudUI.SetActive(false);
        _gameEndUI.SetActive(true);
        _restartButton.interactable = false;
        var canvasGroup = _gameEndUI.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        canvasGroup.DOFade(1, 0.5f).SetUpdate(true);
        yield return new WaitForSecondsRealtime(0.5f);
        _restartButton.interactable = true;
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

    public void PlaySfx(string sfx, float volume = 0.8f)
    {
        var audioItem = Spawn<AudioItem>("AudioItem");
        audioItem.Play(sfx, volume);
    }
    
    public void ApplyCameraImpulse(Vector2 vel)
    {
        _impulse.GenerateImpulseWithVelocity(vel);
    }

    public void RestartGame()
    {
        DOTween.KillAll();
        SceneManager.LoadScene("Game");
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

    private RatingInfo _GetRatingInfo(int score)
    {
        RatingInfo info = null;
        foreach (var ri in _settings.ratingInfoList)
        {
            if (score >= ri.score) info = ri;
        }

        return info;
    }

#if UNITY_EDITOR
    [MenuItem("Tools/ResetHighScore")]
    private static void ResetHighScore()
    {
        PlayerPrefs.DeleteKey("HighScore");
        Debug.Log("reset high score success");
    }
#endif
}
