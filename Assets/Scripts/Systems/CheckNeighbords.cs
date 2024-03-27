using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.VisualScripting;
using UnityEngine;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial struct CheckNeighbords : ISystem, ISystemStartStop
{
    int gridSize;
    int totalSize;
    NativeArray<bool> cellsFutureState;
    NativeArray<bool> cellsCurrentState;
    public void OnCreate(ref SystemState state)
    {
        //state.RequireForUpdate<CellBuffer>();
        //state.RequireForUpdate<SpawnSystem>();
        //state.Enabled = false;
        state.RequireForUpdate<Cell>();
    }
    private void OnDestroy(ref SystemState state)
    {
        cellsFutureState.Dispose();
        cellsCurrentState.Dispose();
    }

    public void OnStartRunning(ref SystemState state)
    {
        var config = SystemAPI.GetSingleton<ConfigGame>();
        gridSize = config.GridSize;
        totalSize = gridSize * gridSize;

        cellsFutureState = CollectionHelper.CreateNativeArray<bool>(totalSize, Allocator.Persistent);
        cellsCurrentState = CollectionHelper.CreateNativeArray<bool>(totalSize, Allocator.Persistent);
    }


    public void OnStopRunning(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        


        var fillStates = new FillCurrentState
        {
            CellsCurrentState = cellsCurrentState,
        }.ScheduleParallel(state.Dependency);
        fillStates.Complete();

        var checkNeighbords = new NeightbordsJob
        {
            GridSize = gridSize,
            TotalSize = totalSize,
            cellsCurrentState = cellsCurrentState.AsReadOnly(),
            CellsFutureState = cellsFutureState
        }.ScheduleParallel(state.Dependency);
        checkNeighbords.Complete();

        var changeState = new ChangeStateCellsJob
        {
            CellsFutureState = cellsFutureState.AsReadOnly(),
            //CellsBuffer = _cellbuffers,
        }.ScheduleParallel(state.Dependency);
        changeState.Complete();

        //nativeCells.Dispose();

    }

    [BurstCompile]
    partial  struct FillCurrentState : IJobEntity
    {
        [NativeDisableParallelForRestriction][WriteOnly] public NativeArray<bool> CellsCurrentState;
        public void Execute(in Cell cell)
        {
            CellsCurrentState[cell.Index] = cell.IsAlive;
        }
    }

    [BurstCompile]
    partial struct NeightbordsJob : IJobEntity
    {
        [ReadOnly] public int GridSize;    //Same size Row and Collums
        [ReadOnly] public int TotalSize;
        [ReadOnly] public NativeArray<bool>.ReadOnly cellsCurrentState;
        [NativeDisableParallelForRestriction][WriteOnly] public NativeArray<bool> CellsFutureState;

        public void Execute([EntityIndexInQuery] int index, in Cell cell)
        {
            int tempIndex = 0;
            int cellsAlive = 0;
            int indexGridX = index % GridSize;

            tempIndex = index - GridSize;
            if (tempIndex >= 0) //Top
            {

                if (cellsCurrentState[tempIndex]) ++cellsAlive;


                //TopLeft
                if ((indexGridX - 1) >= 0)   //Overpass Left Side?
                {
                    tempIndex = index - GridSize - 1;

                    if (cellsCurrentState[tempIndex]) ++cellsAlive;
                }

                //TopRight
                if ((indexGridX + 1) < GridSize) //Overpass Right Side?
                {
                    tempIndex = index - GridSize + 1;

                    if (cellsCurrentState[tempIndex]) ++cellsAlive;
                }
            }

            //Left
            if ((indexGridX - 1) >= 0)
            {
                tempIndex = index - 1;

                if (cellsCurrentState[tempIndex]) ++cellsAlive;
            }

            //Right
            if ((indexGridX + 1) < GridSize) //Overpass Right Side?
            {
                tempIndex = index + 1;

                if (cellsCurrentState[tempIndex]) ++cellsAlive;
            }

            //Bottom
            tempIndex = index + GridSize;
            if (tempIndex < TotalSize)
            {

                if (cellsCurrentState[tempIndex]) ++cellsAlive;

                //BottomLeft
                if ((indexGridX - 1) >= 0)
                {
                    tempIndex = index + GridSize - 1;
                    if (cellsCurrentState[tempIndex]) ++cellsAlive;
                }
                //BottomRight
                if ((indexGridX + 1) < GridSize)
                {
                    tempIndex = index + GridSize + 1;
                    if (cellsCurrentState[tempIndex]) ++cellsAlive;
                }
            }

            if (cell.IsAlive)
            {
                if (cellsAlive <= 1) CellsFutureState[index] = false;
                else if (cellsAlive <= 3) CellsFutureState[index] = true;
                else CellsFutureState[index] = false;
            }
            else
            {
                if (cellsAlive == 3) CellsFutureState[index] = true;
            }

        }
    }

    [BurstCompile]
    private partial struct ChangeStateCellsJob : IJobEntity
    {
        //public DynamicBuffer<CellBuffer> CellsBuffer;
        [ReadOnly] public NativeArray<bool>.ReadOnly CellsFutureState;

        public void Execute([EntityIndexInQuery] int index, ref Cell cell)
        {
            bool isAlive = CellsFutureState[index];
            if (cell.IsAlive != isAlive)
            {
                cell.IsAlive= isAlive;
            }
            
        }
    }
}
