using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;


[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[UpdateAfter(typeof(CheckNeighbords))]
[BurstCompile]
public partial struct ChangeColorSystem : ISystem
{

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Cell>();
    }

    public void OnUpdate(ref SystemState state)
    {

        var config = SystemAPI.GetSingleton<ConfigGame>();
        new ChangeColorJob
        {
            AliveColor = config.CellAliveColor,
            DeadColor = config.CellDeadColor,
        }.ScheduleParallel();
    }

    [BurstCompile]
    private partial struct ChangeColorJob : IJobEntity
    {
        [ReadOnly] public float4 AliveColor;
        [ReadOnly] public float4 DeadColor;
        public void Execute(in Cell Cells, ref URPMaterialPropertyBaseColor color)
        {
            color.Value = Cells.IsAlive ? AliveColor : DeadColor;
        }
    }
}
