using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemySO", menuName = "Scriptable Objects/EnemySO")]
public class EnemySO : ScriptableObject
{
    [Header("Name")]
    public string enemyName;

    [Header("Appearence")]
    public Sprite enemyAppearence;

    [Header("ElementalType")]
    public Element elementalType;

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
    public Element elementalWeakType;

    [Header("Strong")]
    public List<GameObject> elementStrongList;
    public Element elementalStrongType;
}
public enum Element
{
    Fire,
    Water,
    Earth,
    Nothing
}
