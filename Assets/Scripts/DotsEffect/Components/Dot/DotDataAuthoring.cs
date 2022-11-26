using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DotsEffect
{
    public class DotDataAuthoring : MonoBehaviour
    {
        public float frequency = 3.5f;
        public float damping = 0.8f;
        public float hitSpeedScaler;
        public float hitMoveTime;
        public float stopTime;
        public float followTime;

    }
    public class DotDataBaker : Baker<DotDataAuthoring>
    {
        public override void Bake(DotDataAuthoring authoring)
        {
            AddComponent(new DotData{
                frequency = authoring.frequency,
                damping = authoring.damping,
                hitSpeedScaler = authoring.hitSpeedScaler,
                hitMoveTime = authoring.hitMoveTime,
                stopTime = authoring.stopTime,
                followTime = authoring.followTime,
                nowMoveTime = 0f,
                stateIndicator = 0,
                pushVelocity = float3.zero,
                velocity = float3.zero,
                isInExplode = false
            });
        }
    }
}
