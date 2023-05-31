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
    public List<RatingInfo> ratingInfoList;
    public float boarderRadius = 6.5f;
    public EInputType inputType = EInputType.Rotate;
    [ShowIf("@inputType == EInputType.Rotate")] [Min(1)]
    public float mobileRotateScale = 6f;//移动端全屏旋转很难受，需要加倍率优化玩家体验
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
    [Range(0, 180)] public float spawnRange = 45f;
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

[System.Serializable]
public class RatingInfo
{
    public int score;
    public string rating;
    public Color color = Color.white;
}

public enum EInputType
{
    Rotate,
    Slide,
    Joystick,
}

