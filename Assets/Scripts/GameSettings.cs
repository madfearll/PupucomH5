using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class GameSettings : ScriptableObject
{
    [Header("level")] public List<LevelInfo> levelInfoList;

    [Header("effect")]
    [Range(0.1f, 20f)] public float spring = 5;
    [Range(0.1f, 1f)] public float damp = 0.9f;
    public float maxForce = 6f;
    public float rotateForce = 0.5f;
    public float stickImpactForce = 2.5f;
    public float matchImpactForce = 2f;
}


[System.Serializable]
public class LevelInfo
{
    public float time;
    [Range(1, 4)] public int colorCount = 2;
    public List<ESpawnPosition> spawnPositionList = new ();
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