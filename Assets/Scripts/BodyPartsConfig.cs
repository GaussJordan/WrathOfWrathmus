using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CreateAssetMenu(menuName = "Create BodyPartsConfig", fileName = "BodyPartsConfig", order = 0)]
public class BodyPartsConfig : ScriptableObject
{
    public List<BossPart> Parts;
}
