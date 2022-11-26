using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DotsEffect
{
    public class GhostBoneDataAuthoring : MonoBehaviour
    {
    }
    public class GhostBoneDataBaker : Baker<GhostBoneDataAuthoring>
    {
        public override void Bake(GhostBoneDataAuthoring authoring)
        {
            AddComponent(new GhostBoneData{
                boneName = authoring.transform.name
            });
        }
    }
}
