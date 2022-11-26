using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using UnityEngine;

namespace DotsEffect
{
    [BurstCompile]
    public partial struct DotsEffectSystem : ISystem
    {
        BulletMove bulletMoveJob;
        BoneMove boneMoveJob;
        DotMove dotMoveJob;
        NativeList<BulletData> bulletDatas;
        NativeHashMap<FixedString32Bytes, GhostBoneData> boneDatas;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            bulletMoveJob = new BulletMove();
            boneMoveJob = new BoneMove();
            dotMoveJob = new DotMove();

            bulletDatas = new NativeList<BulletData>(Allocator.Persistent);
            
            dotMoveJob.bulletDatas = bulletDatas;
        }
        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            bulletMoveJob.deltaTime = deltaTime;
            
            JobHandle jobHandle = bulletMoveJob.ScheduleParallel(state.Dependency);
            jobHandle.Complete();

            bulletDatas.Clear();
            foreach (BulletMovementAspect bulletMovementAspect in SystemAPI.Query<BulletMovementAspect>())
            {
                if (bulletMovementAspect.bulletData.ValueRO.isActive)
                    bulletDatas.Add(bulletMovementAspect.bulletData.ValueRO);
            }

            jobHandle = boneMoveJob.ScheduleParallel(state.Dependency);
            jobHandle.Complete();

            GhostSpawner ghostSpawner = SystemAPI.GetSingleton<GhostSpawner>();

            int numberOfMovedBones = ghostSpawner.bonesNames.Length;
            boneDatas = new NativeHashMap<FixedString32Bytes, GhostBoneData>(numberOfMovedBones, Allocator.TempJob);
            foreach (GhostBoneMovementAspect boneMovementAspect in SystemAPI.Query<GhostBoneMovementAspect>())
            {
                boneDatas.Add(boneMovementAspect.boneData.ValueRO.boneName, boneMovementAspect.boneData.ValueRO);
            }

            dotMoveJob.boneDatas = boneDatas;
            dotMoveJob.bulletRadius = SystemAPI.GetSingleton<BulletSpawner>().bulletRadius;
            dotMoveJob.groundHeight = -1f;
            dotMoveJob.dotScale = ghostSpawner.unit;
            dotMoveJob.isExplode = ghostSpawner.isExplode;
            dotMoveJob.teleportTime = ghostSpawner.teleportTime;
            dotMoveJob.deltaTime = deltaTime;

            jobHandle = dotMoveJob.ScheduleParallel(state.Dependency);
            jobHandle.Complete();

            boneDatas.Dispose();
        }
    }

    [BurstCompile]
    public partial struct BulletMove : IJobEntity
    {
        public float deltaTime;
        public void Execute(BulletMovementAspect bulletMovementAspect)
        {
            bulletMovementAspect.SolveBulletMovement(deltaTime);
        }
    }
    [BurstCompile]
    public partial struct BoneMove : IJobEntity
    {
        public void Execute(GhostBoneMovementAspect boneMovementAspect)
        {
            boneMovementAspect.SolveBoneTransform();
        }
    }
    [BurstCompile]
    public partial struct DotMove : IJobEntity
    {
        [NativeDisableParallelForRestriction] public NativeList<BulletData> bulletDatas;
        [NativeDisableParallelForRestriction] public NativeHashMap<FixedString32Bytes, GhostBoneData> boneDatas;
        public bool isExplode;
        public float bulletRadius;
        public float groundHeight;
        public float dotScale;
        public float teleportTime;
        public float deltaTime;
        public void Execute(DotMovementAspect dotMovementAspect)
        {
            dotMovementAspect.SolveDotMovement(bulletDatas, boneDatas, isExplode, bulletRadius, groundHeight, dotScale, teleportTime, deltaTime);
        }
    }
}