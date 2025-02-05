using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemySO", menuName = "Scriptable Objects/EnemySO")]
public class EnemySO : ScriptableObject
{
    [Header("NameAndID")]
    public string enemyName;
    public string enemyType;

    [Header("Appearence")]
    public Sprite bodySprite;
    public Sprite normalFaceSprite;
    public Sprite madFaceSprite;
    public Sprite particleSprite;

    [Header("ElementalType")]
    public OrbType elementalType;

    [Header("Health")]
    public float maxHealth;

    [Header("Damage")]
    public float maxBasicDamageAttack;
    public float maxHeavyDamageAttack;

    [Header("Attacks")]
    public float basicAttack;
    public float heavyAttack;

    [Header("Weakness")]
    public List<GameObject> elementWeaknessList;
    public OrbType elementalWeakType;

    [Header("Strong")]
    public List<GameObject> elementStrongList;
    public OrbType elementalStrongType;
}
public enum Element
{
    Fire,
    Water,
    Earth,
    Nothing
}
