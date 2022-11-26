using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

namespace DotsEffect
{
    public readonly partial struct GhostBoneMovementAspect : IAspect
    {
        public readonly Entity entity;
        public readonly TransformAspect transformAspect;
        public readonly RefRW<GhostBoneData> boneData;

        public void SolveBoneTransform()
        {
            transformAspect.Position = boneData.ValueRO.targetPosition;
            transformAspect.Rotation = boneData.ValueRO.targetRotation;
            UniformScaleTransform localToWorld = transformAspect.LocalToWorld;
            localToWorld.Scale = boneData.ValueRO.targetLocalScale;
            transformAspect.LocalToWorld = localToWorld;
            boneData.ValueRW.localToWorld = Matrix4x4.TRS(boneData.ValueRO.targetPosition, boneData.ValueRO.targetRotation,  boneData.ValueRO.targetScale);
        }
    }
}
