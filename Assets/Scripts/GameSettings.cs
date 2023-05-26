using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class GameSettings : ScriptableObject
{
    public float spring = 5;
    public float damp = 0.9f;
    public float maxForce = 6f;
}
