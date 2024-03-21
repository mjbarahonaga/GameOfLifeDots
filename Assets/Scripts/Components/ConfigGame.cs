using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct ConfigGame : IComponentData
{
    public Entity Prefab;
    public int GridSize;
    public float CellSize;
    public float4 CellAliveColor;
    public float4 CellDeadColor;
}
