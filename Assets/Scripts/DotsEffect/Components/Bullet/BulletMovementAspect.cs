using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

namespace DotsEffect
{
    public readonly partial struct BulletMovementAspect : IAspect
    {
        public readonly Entity entity;
        public readonly TransformAspect transformAspect;
        public readonly RefRW<BulletData> bulletData;

        public void SolveBulletMovement(float deltaTime)
        {
            if (bulletData.ValueRO.isActive)
            {
                bulletData.ValueRW.lastPosition = transformAspect.Position;
                transformAspect.Position += bulletData.ValueRO.velocity * deltaTime;
                bulletData.ValueRW.nowPosition = transformAspect.Position;
                transformAspect.Rotation = bulletData.ValueRO.targetRotation;
                bulletData.ValueRW.activeTime += deltaTime;

                if (bulletData.ValueRO.activeTime > bulletData.ValueRO.maxActiveTime)
                {
                    ActivateBulletData(false);
                }
            }
        }
        
        public void ActivateBulletData(bool isActive = false)
        {
            bulletData.ValueRW.isActive = isActive;
            bulletData.ValueRW.activeTime = 0f;
        }
    }
}
