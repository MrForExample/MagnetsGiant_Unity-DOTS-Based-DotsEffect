using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace DotsEffect
{
    public partial class DotsMeshSpawnSystem : SystemBase
    {
        protected override void OnStartRunning()
        {
            GhostSpawner ghostSpawner = SystemAPI.GetSingleton<GhostSpawner>();
            Entity ghost = EntityManager.Instantiate(ghostSpawner.ghost);

            EntityQuery boneEntityQuery = EntityManager.CreateEntityQuery(typeof(GhostBoneData));
            NativeArray<Entity> boneEntities = boneEntityQuery.ToEntityArray(Allocator.Temp);

            EntityQuery dotEntityQuery = EntityManager.CreateEntityQuery(typeof(DotData));

            while (dotEntityQuery.CalculateEntityCount() < ghostSpawner.dotsPositions.Length)
            {
                Entity dot = EntityManager.Instantiate(ghostSpawner.dot);
            }

            NativeArray<Entity> dotEntities = dotEntityQuery.ToEntityArray(Allocator.Temp);

            for (int i = 0; i < dotEntities.Length; i++)
            {
                Entity dot = dotEntities[i];
                DotMovementAspect dotMovementAspect = EntityManager.GetAspect<DotMovementAspect>(dot);
                dotMovementAspect.dotData.ValueRW.parentBoneName = ghostSpawner.bonesNames[i];
                dotMovementAspect.transformAspect.Position = ghostSpawner.dotsPositions[i];
                dotMovementAspect.dotData.ValueRW.targetLocalPosition = ghostSpawner.dotsLocalPositions[i];
                dotMovementAspect.dotData.ValueRW.lastTargetPosition = ghostSpawner.dotsPositions[i];
                dotMovementAspect.dotData.ValueRW.explodeVelocity = ghostSpawner.dotsExplodeVelocities[i];
                dotMovementAspect.dotData.ValueRW.teleportGateTime = ghostSpawner.dotsTeleportGateTimes[i];
            }

            dotEntities.Dispose();
            boneEntities.Dispose();
        }
        protected override void OnUpdate()
        {

        }
    }
}
