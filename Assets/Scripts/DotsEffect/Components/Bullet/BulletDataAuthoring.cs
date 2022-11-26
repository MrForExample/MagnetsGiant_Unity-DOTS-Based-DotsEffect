using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DotsEffect
{
    public class BulletDataAuthoring : MonoBehaviour
    {
        public float maxActiveTime = 1f;
    }
    public class BulletDataBaker : Baker<BulletDataAuthoring>
    {
        public override void Bake(BulletDataAuthoring authoring)
        {
            AddComponent(new BulletData{
                maxActiveTime = authoring.maxActiveTime,
                isActive = false,
                activeTime = 0f,
                velocity = float3.zero,
            });
        }
    }
}
