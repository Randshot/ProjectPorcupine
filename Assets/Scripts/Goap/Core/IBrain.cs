using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IBrain
{
    void Init();
	void Tick(IGoap goap, float deltaTime);
    void Release();
    Dictionary<string,bool> NextGoal();
}
