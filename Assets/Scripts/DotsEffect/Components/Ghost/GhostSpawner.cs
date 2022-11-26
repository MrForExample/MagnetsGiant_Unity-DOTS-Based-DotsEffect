using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace DotsEffect
{
    public struct GhostSpawner : IComponentData
    {
        public NativeArray<FixedString32Bytes> bonesNames;
        public NativeArray<float3> dotsPositions;
        public NativeArray<float3> dotsLocalPositions;
        public NativeArray<float3> dotsExplodeVelocities;
        public NativeArray<float> dotsTeleportGateTimes;
        public float unit;
        public float ghostScale;
        public Entity ghost;
        public Entity dot;
        public bool isExplode;
        public float teleportTime;
    }
}
