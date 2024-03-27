using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject _prefabCell;
    [SerializeField] private Color _cellAliveColor;
    [SerializeField] private Color _cellDeadColor = Color.black;

    [SerializeField] private float _cellSize = 0.1f;
    [SerializeField] private int _gridSize = 256;
    [SerializeField] private bool _isRunning = false;
    private World _world;
    private Entity _configData;


    private void OnEnable()
    {
        _world = World.DefaultGameObjectInjectionWorld;
    }

    private class CellPrefabBaker : Baker<GameManager>
    {
        public override void Bake(GameManager authoring)
        {
            authoring._configData = GetEntity(authoring ,TransformUsageFlags.None);
            var entity = GetEntity(authoring._prefabCell, TransformUsageFlags.Renderable);
            //AddBuffer<CellState>(entity);
            AddComponent(authoring._configData, new ConfigGame
            {
                Prefab = entity,
                GridSize = authoring._gridSize,
                CellSize = authoring._cellSize,
                CellAliveColor = (Vector4)authoring._cellAliveColor,
                CellDeadColor = (Vector4)authoring._cellDeadColor
            });
            
        }
    }

    private void StartSimulation()
    {
        if (_isRunning) return;

        //_configData = _world.EntityManager.CreateEntity();
        //_world.EntityManager.AddComponentData(_configData, new ConfigGame
        //{
        //    Prefab = _prefab,
        //    GridSize = _gridSize,
        //    CellSize = _cellSize,
        //    CellAliveColor = (Vector4)_cellAliveColor,
        //    CellDeadColor = (Vector4)_cellDeadColor
        //});

        if (_world.IsCreated)
        {
            // Spawn Cells
            SystemHandle spawnSystem = _world.GetExistingSystem<SpawnSystem>();
            _world.Unmanaged.ResolveSystemStateRef(spawnSystem).Enabled = true;   //Call execute OnUpdate
        }
        _isRunning = true;
    }

    private void FinishSimulation()
    {
        if (!_isRunning) return;

        if (_world.IsCreated)
        {
            if (_world.EntityManager.Exists(_configData))
            {
                _world.EntityManager.DestroyEntity(_configData);
            }
            _isRunning = false;
        }
    }
}
