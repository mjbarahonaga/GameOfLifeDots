using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using System.Linq;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;
using Unity.Mathematics;
using UnityEngine;

//[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct SpawnSystem : ISystem
{
    private Random _random;
    private NativeArray<Entity> _cells;
    
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ConfigGame>();
        _random = new Random((uint)(float)System.DateTime.Now.TimeOfDay.TotalMilliseconds);
    }

    public void OnDestroy(ref SystemState state)
    {
        _cells.Dispose();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        
        //if (!Input.GetKeyDown(KeyCode.S))
        //{
        //    return;
        //}

        //if (_cells.Length != 0)
        //{
        //    state.EntityManager.DestroyEntity(_cells);
        //}

        state.Enabled = false;

        ConfigGame config = SystemAPI.GetSingleton<ConfigGame>();

        int length = config.GridSize * config.GridSize;
        _cells = state.EntityManager.Instantiate(config.Prefab, length, Allocator.Persistent);

        float gridOffset = (config.GridSize - 1) * config.CellSize / 2;
        var aliveColor = new URPMaterialPropertyBaseColor { Value = config.CellAliveColor };
        var deadColor = new URPMaterialPropertyBaseColor { Value = config.CellDeadColor };

        //int rows = config.GridSize;
        //int collums = config.GridSize;
        //float cellSize = config.CellSize;
        //for (int j = 0; j < rows; ++j)
        //{
        //    for (int i = 0; i < collums; ++i)
        //    {
        //        float3 pos = new float3(i * cellSize - gridOffset, j * cellSize - gridOffset, 0f);
        //        bool alive = _random.NextBool();

        //        int index = i + j * rows;

        //        SystemAPI.SetComponent(cells[index], new CellState
        //        {
        //            IsAlive = alive
        //        });

        //        SystemAPI.SetComponent(cells[index], new URPMaterialPropertyBaseColor
        //        {
        //            Value = alive ? aliveColor.Value : deadColor.Value
        //        });

        //        SystemAPI.SetComponent(cells[index], new LocalTransform
        //        {
        //            Position = pos,
        //            Rotation = quaternion.identity,
        //            Scale = cellSize
        //        });
        //    }
        //}

        state.Dependency = new SpawnCellsJob
        {
            random = _random,
            gridSize = config.GridSize,
            cellSize = config.CellSize,
            gridOffset = gridOffset,
            aliveColor = aliveColor,
            deadColor = deadColor
        }.ScheduleParallel(state.Dependency);

        state.Dependency.Complete();

        var system = state.WorldUnmanaged.GetExistingSystemState<CheckNeighbords>();
        system.Enabled = true;

        //entities.Dispose();
    }

    [BurstCompile]
    private partial struct SpawnCellsJob : IJobEntity
    {
        public Random random;
        public int gridSize;
        public float cellSize;
        public float gridOffset;
        public URPMaterialPropertyBaseColor aliveColor;
        public URPMaterialPropertyBaseColor deadColor;

        private void Execute([EntityIndexInQuery] int index, ref Cell cell, ref URPMaterialPropertyBaseColor color, ref LocalTransform transform)
        {
            cell.IsAlive = random.NextBool();
            cell.Index = index;
            color.Value = cell.IsAlive ? aliveColor.Value : deadColor.Value;
            ushort x = (ushort)(index % gridSize);
            ushort y = (ushort)(index / gridSize);
            transform.Position = new float3(x * cellSize - gridOffset, y * cellSize - gridOffset, 0f);
            transform.Scale = cellSize;
            transform.Rotation = quaternion.identity;
        }
    }


}
