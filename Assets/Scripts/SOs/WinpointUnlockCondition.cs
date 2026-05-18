using UnityEngine;

[CreateAssetMenu(fileName = "WinpointUnlockCondition", menuName = "ScriptableObjects/WinpointUnlockCondition", order = 1)]
public class WinpointUnlockCondition : ScriptableObject
{
    public string conditionName;

    void OnValidate()
    {
        conditionName = name;
    }
}