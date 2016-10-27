using UnityEngine;
using System.Collections;
using System;

public interface IAgent
{
    void AddAction(GoapAction a);

    GoapAction GetAction(Type action);

    void RemoveAction(GoapAction action);

    void AbortFsm();
}
