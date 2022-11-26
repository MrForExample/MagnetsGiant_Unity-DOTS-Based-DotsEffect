using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace DotsEffect
{
    public struct DotData : IComponentData
    {
        public float frequency;
        public float damping;
        public float hitSpeedScaler;
        public float hitMoveTime;
        public float stopTime;
        public float followTime;
        public float nowMoveTime;
        public float teleportGateTime;
        public int stateIndicator;
        public float3 explodeVelocity;
        public float3 pushVelocity;
        public float3 velocity;
        public FixedString32Bytes parentBoneName;
        public float3 targetLocalPosition;
        public float3 lastTargetPosition;
        public bool isInExplode;
    }
}
