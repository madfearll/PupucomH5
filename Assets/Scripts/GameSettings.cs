using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu]
public class GameSettings : ScriptableObject
{
    [Title("level")] public float spawnDistance = 10f;
    public List<LevelInfo> levelInfoList;
    public List<ComboInfo> comboInfoList;
    public float boarderRadius = 6.5f;
    public EInputType inputType = EInputType.Rotate;
    [ShowIf("@inputType == EInputType.Slide")] public float slideRange = 360f;

    [Title("effect")]
    [Range(0.1f, 20f)] public float spring = 5;
    [Range(0.1f, 1f)] public float damp = 0.9f;
    public float maxForce = 6f;
    public float rotateForce = 0.5f;
    public float stickImpactForce = 2.5f;
    public float matchImpactForce = 2f;
    public float disconnectImpulse = 5f;
}


[System.Serializable]
public class LevelInfo
{
    public float time;
    public float spawnIntervalMin = 2;
    public float spawnIntervalMax = 3f;
    [Range(1, 6)] public int colorCount = 2;
    public List<ESpawnPosition> spawnPositionList = new() {ESpawnPosition.Left, ESpawnPosition.Right};
    public float speedMin = 2f;
    public float speedMax = 3f;
}

public enum ESpawnPosition
{
    Right,
    TopRight,
    Top,
    TopLeft,
    Left,
    BottomLeft,
    Bottom,
    BottomRight,
}

[System.Serializable]
public class ComboInfo
{
    public int combo = 1;
    public Color color = Color.white;
    public int score = 100;
}

public enum EInputType
{
    Rotate,
    Slide,
}