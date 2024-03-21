using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct CellState : IComponentData
{
    public bool IsAlive;
}
