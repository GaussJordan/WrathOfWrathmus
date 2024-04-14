using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public interface ISequenceStep
{
    UniTask Run(CancellationToken token);
}

[Serializable]
public class TestSequence : ISequenceStep
{
    public async UniTask Run(CancellationToken token)
    {
        
    }
}


[CreateAssetMenu(menuName = "Create Sequence", fileName = "Sequence", order = 0)]
public class Sequence : ScriptableObject
{
    [SerializeReference, SerializeReferenceDropdown] private List<ISequenceStep> _steps;

    public async UniTask Run(CancellationToken token)
    {
        foreach (var sequenceStep in _steps) await sequenceStep.Run(token);
    }
}
