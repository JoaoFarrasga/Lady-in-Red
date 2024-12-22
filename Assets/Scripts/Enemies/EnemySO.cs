using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemySO", menuName = "Scriptable Objects/EnemySO")]
public class EnemySO : ScriptableObject
{
    [Header("Name")]
    public string enemyName;

    [Header("ElementalType")]
    public Element elementalType;

    [Header("Health")]
    public float health;

    [Header("Damage")]
    public float basicDamageAttack;
    public float heavyDamageAttack;

    [Header("Attacks")]
    public float basicAttack;
    public float heavyAttack;

    [Header("Weakness")]
    public Tuple<GameObject> elementWeaknessList;
}
public enum Element
{
    Fire,
    Water,
    Air
}
