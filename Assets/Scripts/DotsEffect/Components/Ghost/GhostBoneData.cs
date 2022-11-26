using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace DotsEffect
{
    public struct GhostBoneData : IComponentData
    {
        public FixedString32Bytes boneName;
        public float3 targetPosition;
        public quaternion targetRotation;
        public float3 targetScale;
        public float targetLocalScale;
        public Matrix4x4 localToWorld;
    }
}
