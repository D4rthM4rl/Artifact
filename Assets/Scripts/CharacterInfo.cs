using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterInfo.asset", menuName = "CharacterInfo/CharacterInfo")]
[System.Serializable]
public class CharacterInfo : ScriptableObject
{
    /// <summary>What type/species of Character this is</summary>
    public string species;
    /// <summary>Brief description of the Character</summary>
    [TextArea(5,10)]
    public string description;
    /// <summary>Stats of this character without any outside changes</summary>
    [Tooltip("Stats of this character without any outside changes")]
    [SerializeField]
    public CharacterStats baseStats;

    public Sprite sprite;
}
