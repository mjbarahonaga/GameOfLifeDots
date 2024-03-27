using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.VisualScripting;
using UnityEngine;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial struct CheckNeighbords : ISystem
{

    public void OnCreate(ref SystemState state)
    {
        //state.RequireForUpdate<CellBuffer>();
        //state.RequireForUpdate<SpawnSystem>();
        //state.Enabled = false;
        state.RequireForUpdate<Cell>();
    }

    public void OnDestroy(ref SystemHandle state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var config = SystemAPI.GetSingleton<ConfigGame>();
        int gridSize = config.GridSize;
        int totalSize = gridSize * gridSize;

        var cellsFutureState = CollectionHelper.CreateNativeArray<bool>(totalSize, state.WorldUpdateAllocator);
        var cellsCurrentState = CollectionHelper.CreateNativeArray<bool>(totalSize, state.WorldUpdateAllocator);
        //var cells = SystemAPI.GetSingletonBuffer<CellBuffer>();

        //var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        //NativeArray<Entity> entities = entityManager.GetAllEntities(Allocator.Temp);
        //_cellbuffers = entityManager.GetBuffer<CellBuffer>(config.Prefab);
        //_cellbuffers.Length = totalSize;
        ////_cellbuffers = entityManager.GetBuffer<CellBuffer>(config.Prefab);
        //foreach (Entity entity in entities)
        //{
        //    if (entityManager.HasComponent<CellBuffer>(entity))
        //    {
        //        DynamicBuffer<CellBuffer> cellbuffer = entityManager.GetBuffer<CellBuffer>(entity);

        //        _cellbuffers.Insert(cellbuffer[0].Index, cellbuffer[0]);
        //    }
        //}



        //var nativeCells = cells.AsNativeArray();

        var fillStates = new FillCurrentState
        {
            CellsCurrentState = cellsCurrentState,
        }.Schedule(state.Dependency);
        fillStates.Complete();

        var checkNeighbords = new NeightbordsJob
        {
            GridSize = gridSize,
            TotalSize = totalSize,
            cellsCurrentState = cellsCurrentState.AsReadOnly(),
            CellsFutureState = cellsFutureState
        }.Schedule(state.Dependency);
        checkNeighbords.Complete();

        var changeState = new ChangeStateCellsJob
        {
            CellsFutureState = cellsFutureState.AsReadOnly(),
            //CellsBuffer = _cellbuffers,
        }.Schedule(state.Dependency);
        changeState.Complete();

        //nativeCells.Dispose();
        cellsFutureState.Dispose();
        cellsCurrentState.Dispose();
    }

    [BurstCompile]
    partial struct FillCurrentState : IJobEntity
    {
        public NativeArray<bool> CellsCurrentState;
        public void Execute(in Cell cell)
        {
            CellsCurrentState[cell.Index] = cell.IsAlive;
        }
    }

    [BurstCompile]
    partial struct NeightbordsJob : IJobEntity
    {
        public int GridSize;    //Same size Row and Collums
        public int TotalSize;
        [ReadOnly] public NativeArray<bool>.ReadOnly cellsCurrentState;
        [WriteOnly] public NativeArray<bool> CellsFutureState;

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
