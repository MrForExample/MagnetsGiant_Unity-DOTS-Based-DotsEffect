using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace DotsEffect
{
    public class DotSpawnerAuthoring : MonoBehaviour
    {
        public int maxDotsCount;
        public GameObject dot;
    }
    public class DotSpawnerBaker : Baker<DotSpawnerAuthoring>
    {
        public override void Bake(DotSpawnerAuthoring authoring)
        {
            AddComponent(new DotSpawner{
                maxDotsCount = authoring.maxDotsCount,
                dot = GetEntity(authoring.dot)
            });
        }
    }
}
