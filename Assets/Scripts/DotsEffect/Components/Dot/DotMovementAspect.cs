using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

namespace DotsEffect
{
    public readonly partial struct DotMovementAspect : IAspect
    {
        public readonly Entity entity;
        public readonly TransformAspect transformAspect;
        public readonly RefRW<DotData> dotData;

        public void SolveDotMovement(NativeList<BulletData> bulletDatas, NativeHashMap<FixedString32Bytes, GhostBoneData> boneDatas, bool isExplode, float bulletRadius, float groundHeight, float dotScale, float teleportTime, float deltaTime)
        {
            if (isExplode)
            {
                if (!dotData.ValueRO.isInExplode)
                {
                    dotData.ValueRW.isInExplode = true;
                    dotData.ValueRW.pushVelocity = dotData.ValueRO.explodeVelocity;
                    dotData.ValueRW.stateIndicator = 1;
                    dotData.ValueRW.nowMoveTime = 0f;
                }
            }
            else
            {
                dotData.ValueRW.isInExplode = false;
                
                // Calculate dot's velocity due to impact of the bullet
                float3 pushVelocity = float3.zero;
                float bulletRadiusSq = bulletRadius * bulletRadius;
                int bulletsHitCount = 0;
                for (int i = 0; i < bulletDatas.Length; i++)
                {
                    float3 lineStart = bulletDatas[i].lastPosition;
                    float3 lineDirection = bulletDatas[i].nowPosition - lineStart;
                    float lineLength = math.length(lineDirection);
                    lineDirection = math.normalize(lineDirection);

                    float projectLength = math.dot(transformAspect.Position - lineStart, lineDirection);
                    if (projectLength > 0f && projectLength < lineLength)
                    {
                        float3 projectPoint = lineStart + lineDirection * projectLength;

                        float3 verticalOffset = transformAspect.Position - projectPoint;
                        float verticalLengthSq = math.lengthsq(verticalOffset);
                        if (verticalLengthSq < bulletRadiusSq)
                        {
                            pushVelocity += math.normalize(transformAspect.Position - (projectPoint - math.sqrt(bulletRadiusSq - verticalLengthSq) * lineDirection))
                                                * math.length(bulletDatas[i].velocity)
                                                * dotData.ValueRO.hitSpeedScaler;
                            bulletsHitCount++;
                        }
                    }
                }

                if (bulletsHitCount > 0)
                {
                    pushVelocity /= bulletsHitCount;
                    
                    dotData.ValueRW.pushVelocity = pushVelocity;
                    dotData.ValueRW.stateIndicator = 1;
                    dotData.ValueRW.nowMoveTime = 0f;
                }
            }


            float3 targetPosition = transformAspect.Position;
            switch (dotData.ValueRO.stateIndicator)
            {
                // Follow
                case 0:
                    if (teleportTime > dotData.ValueRO.teleportGateTime)
                    {
                        float3 nowTargetPosition = boneDatas[dotData.ValueRO.parentBoneName].localToWorld.MultiplyPoint3x4(dotData.ValueRO.targetLocalPosition);
                        float alpha = (dotData.ValueRO.nowMoveTime + math.EPSILON) / (dotData.ValueRO.followTime + math.EPSILON);
                        float frequency = math.lerp(0f, dotData.ValueRO.frequency, alpha);
                        float damping = math.lerp(0f, dotData.ValueRO.damping, alpha);
                        float3 nowTargetVelocity = (nowTargetPosition - dotData.ValueRO.lastTargetPosition) / deltaTime;

                        dotData.ValueRW.velocity += DotsCommon.CalculatePDForceStable(nowTargetPosition, nowTargetVelocity, transformAspect.Position, dotData.ValueRO.velocity, frequency, damping, deltaTime) * deltaTime;
                        targetPosition += dotData.ValueRO.velocity * deltaTime;

                        dotData.ValueRW.lastTargetPosition = nowTargetPosition;

                        if (dotData.ValueRO.nowMoveTime < dotData.ValueRO.followTime)
                            dotData.ValueRW.nowMoveTime += deltaTime;
                    }
                    else if (dotData.ValueRO.nowMoveTime != 0f)
                    {
                        dotData.ValueRW.nowMoveTime = 0f;
                    }
                    break;
                // Hit
                case 1:
                    dotData.ValueRW.velocity = math.lerp(dotData.ValueRO.pushVelocity, float3.zero, (dotData.ValueRO.nowMoveTime + math.EPSILON) / (dotData.ValueRO.hitMoveTime + math.EPSILON));
                    targetPosition += dotData.ValueRO.velocity * deltaTime;

                    if (dotData.ValueRO.nowMoveTime > dotData.ValueRO.hitMoveTime)
                    {
                        dotData.ValueRW.stateIndicator++;
                        dotData.ValueRW.nowMoveTime = 0f;
                        dotData.ValueRW.lastTargetPosition = transformAspect.Position;
                    }
                    else
                    {
                        dotData.ValueRW.nowMoveTime += deltaTime;
                    }

                    break;
                // Stop
                case 2:
                    targetPosition = dotData.ValueRO.lastTargetPosition;

                    if (dotData.ValueRO.nowMoveTime > dotData.ValueRO.stopTime)
                    {
                        dotData.ValueRW.stateIndicator = 0;
                        dotData.ValueRW.nowMoveTime = 0f;
                        dotData.ValueRW.lastTargetPosition = boneDatas[dotData.ValueRO.parentBoneName].localToWorld.MultiplyPoint3x4(dotData.ValueRO.targetLocalPosition);
                    }
                    else
                    {
                        dotData.ValueRW.nowMoveTime += deltaTime;
                    }
                    break;
            }

            transformAspect.Position = new float3(targetPosition.x, math.max(targetPosition.y, groundHeight), targetPosition.z);
            UniformScaleTransform localToWorld = transformAspect.LocalToWorld;
            localToWorld.Scale = dotScale;
            transformAspect.LocalToWorld = localToWorld;
        }
    }
}
