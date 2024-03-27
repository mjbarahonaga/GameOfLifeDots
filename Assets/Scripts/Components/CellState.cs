using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct CellBuffer : IBufferElementData
{
    public int Index;
    public bool IsAlive;
}

public struct Cell : IComponentData 
{
    public int Index;
    public bool IsAlive;
}
