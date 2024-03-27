using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class CellAuthoring : MonoBehaviour
{
    private class CellAuthoringBaker : Baker<CellAuthoring>
    {
        public override void Bake(CellAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            //AddComponent(entity, new CellPrefabs() { CellPrefab = GetEntity(authoring._prefabCell, TransformUsageFlags.Renderable)});
            AddComponent(entity, new Cell());
            //AddBuffer<CellBuffer>(entity);

        }
    }
}

