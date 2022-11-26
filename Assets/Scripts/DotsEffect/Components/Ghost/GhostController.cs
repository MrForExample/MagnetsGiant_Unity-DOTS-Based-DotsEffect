using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using UnityEngine;
using MoreMountains.Feedbacks;

namespace DotsEffect
{
    public class GhostController : MonoBehaviour
    {
        public Transform player;
        public MMF_Player ghostFootStopmFeedbacks;
        public float walkingSpeed = 3f;
        float defaultWalkingAnimSpeed;

        [Space(3)][Header("Teleport")]
        public MMF_Player ghostTeleportFeedbacks;
        public Hovl_Laser laser;
        public float laserShootTime = 0.5f;
        public float minStartTeleportDistance = 20f;
        public float targetTeleportDistance = 10f;
        public float teleportWaitTime = 1f;
        public float explodeTeleportDelayTime = 2f;
        float allTeleportTime, nowTeleportTime;
        bool isInTeleport = false, isReadyToTeleport = false;

        [Space(3)][Header("Attack")]
        public MMF_Player ghostAttack0Feedbacks;
        public MMF_Player ghostAttack1Feedbacks;
        public MMF_Player ghostSmashFeedbacks;
        public MMF_Player ghostPreExplodeFeedbacks;
        public MMF_Player ghostExplodeFeedbacks;
        public int attackCountPriorToExplode = 2;
        public float maxSmashDistance = 3f;
        public float minRandomAttackWaitTime = 6f;
        public float maxRandomAttackWaitTime = 18f;
        float nowRandomAttackWaitTime, nowAttackWaitTime = 0f;
        int attackCount = 0;
        int attackIndex = 0;
        bool isInAttack = false;

        Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();
        Animator animator;
        void Start()
        {
            GhostSpawner ghostSpawner;
            World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(typeof(GhostSpawner)).TryGetSingleton<GhostSpawner>(out ghostSpawner);
            
            transform.localScale = new Vector3(1f, 1f, 1f) * ghostSpawner.ghostScale;

            allTeleportTime = ghostSpawner.teleportTime + teleportWaitTime;
            nowTeleportTime = allTeleportTime;

            Transform[] allBones = GetComponentsInChildren<Transform>();
            foreach (Transform bone in allBones)
                boneMap.Add(bone.name, bone);

            animator = GetComponentInChildren<Animator>();
            defaultWalkingAnimSpeed = animator.GetFloat("WalkingAnimSpeed");

            SetNowAttackWaitTime();
        }

        void Update()
        {
            float deltaTime = Time.deltaTime;
            CopyBoneDataForECS();
            FollowPlayer(deltaTime);
            SetRandomAttack(deltaTime);
        }

        void CopyBoneDataForECS()
        {
            EntityQuery boneEntityQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(typeof(GhostBoneData));
            NativeArray<Entity> boneEntities = boneEntityQuery.ToEntityArray(Allocator.Temp);

            foreach (Entity bone in boneEntities)
            {
                Transform targetBone = boneMap[World.DefaultGameObjectInjectionWorld.EntityManager.GetName(bone)];
                GhostBoneMovementAspect boneMovementAspect = World.DefaultGameObjectInjectionWorld.EntityManager.GetAspect<GhostBoneMovementAspect>(bone);
                boneMovementAspect.boneData.ValueRW.targetPosition = targetBone.position;
                boneMovementAspect.boneData.ValueRW.targetRotation = targetBone.rotation;
                boneMovementAspect.boneData.ValueRW.targetScale = targetBone.lossyScale;
                boneMovementAspect.boneData.ValueRW.targetLocalScale = targetBone.localScale.x;
            }
            boneEntities.Dispose();
        }
        /// <summary>
        /// Follow player closely, teleport to player's back side if player is too far away
        /// </summary>
        void FollowPlayer(float deltaTime)
        {
            Vector3 walkOffset = Vector3.ProjectOnPlane(player.position - transform.position, Vector3.up);
            transform.rotation = Quaternion.LookRotation(walkOffset.normalized, Vector3.up);

            if (!isInAttack)
            {
                if (nowTeleportTime < allTeleportTime)
                {
                    nowTeleportTime += deltaTime;

                    SetDotsTeleport();

                    if (nowTeleportTime >= allTeleportTime)
                    {
                        animator.SetFloat("WalkingAnimSpeed", defaultWalkingAnimSpeed);
                        isInTeleport = false;
                    }

                    if (laser.gameObject.activeSelf && nowTeleportTime >= laserShootTime)
                    {
                        laser.gameObject.SetActive(false);
                    }
                }
                else if (walkOffset.magnitude < maxSmashDistance)
                {
                    SetNowAttack(3);
                }
                else if (walkOffset.magnitude < minStartTeleportDistance)
                {
                    transform.position += walkOffset.normalized * walkingSpeed * deltaTime;
                }
                else
                {
                    // Teleport damage
                    laser.gameObject.SetActive(true);
                    laser.transform.position = new Vector3(transform.position.x, laser.transform.position.y, transform.position.z);
                    laser.transform.rotation = transform.rotation;

                    // Prepare to teleport
                    transform.position = player.position + walkOffset.normalized * targetTeleportDistance;
                    animator.SetFloat("WalkingAnimSpeed", 0f);
                    StartTeleport();

                    ghostTeleportFeedbacks.PlayFeedbacks();
                }
            }
        }

        void StartTeleport()
        {
            nowTeleportTime = 0f;

            SetDotsTeleport();

            isInTeleport = true;
            isInAttack = false;
        }

        void SetDotsTeleport(bool isExplode = false)
        {
            EntityQuery ghostSpawnerQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(typeof(GhostSpawner));
            GhostSpawner ghostSpawner, newGhostSpawner = new GhostSpawner();
            ghostSpawnerQuery.TryGetSingleton<GhostSpawner>(out ghostSpawner);
            Entity ghostSpawnerEntity;
            ghostSpawnerQuery.TryGetSingletonEntity<GhostSpawner>(out ghostSpawnerEntity);

            newGhostSpawner.bonesNames = ghostSpawner.bonesNames;
            newGhostSpawner.dotsPositions = ghostSpawner.dotsPositions;
            newGhostSpawner.dotsLocalPositions = ghostSpawner.dotsLocalPositions;
            newGhostSpawner.unit = ghostSpawner.unit;
            newGhostSpawner.ghostScale = ghostSpawner.ghostScale;
            newGhostSpawner.ghost = ghostSpawner.ghost;
            newGhostSpawner.dot = ghostSpawner.dot;
            newGhostSpawner.isExplode = isExplode;
            newGhostSpawner.teleportTime = nowTeleportTime;

            World.DefaultGameObjectInjectionWorld.EntityManager.SetComponentData(ghostSpawnerEntity, newGhostSpawner);
        }
        void SetRandomAttack(float deltaTime)
        {
            if (!(isInTeleport || isInAttack))
            {
                if (nowAttackWaitTime > nowRandomAttackWaitTime)
                {
                    if (attackCount < attackCountPriorToExplode)
                    {
                        SetNowAttack(Random.Range(1, 3));

                        switch (attackIndex)
                        {
                            case 1:
                                ghostAttack0Feedbacks.PlayFeedbacks(player.position);
                                break;
                            case 2:
                                ghostAttack1Feedbacks.PlayFeedbacks(player.position);
                                break;
                        }
                    }
                    else
                    {
                        SetNowAttack(4);
                        attackCount = 0;
                        ghostPreExplodeFeedbacks.PlayFeedbacks();
                    }
                }
                else
                {
                    nowAttackWaitTime += deltaTime;
                }
            }
        }
        void SetNowAttackWaitTime()
        {
            nowRandomAttackWaitTime = Random.Range(minRandomAttackWaitTime, maxRandomAttackWaitTime);
        }
        void SetNowAttack(int nowAttackIndex)
        {
            nowAttackWaitTime = 0f;
            attackIndex = nowAttackIndex;
            animator.SetInteger("AttackIndex", attackIndex);
            isInAttack = attackIndex > 0;
            if (isInAttack)
                attackCount++;
        }
        public void GhostFootStepEffect()
        {
            ghostFootStopmFeedbacks.PlayFeedbacks();
        }
        public void GhostSmashEffect()
        {
            ghostSmashFeedbacks.PlayFeedbacks();
        }
        public void GhostExplodeEffect()
        {
            SetDotsTeleport(true);
            ghostExplodeFeedbacks.PlayFeedbacks();

            GhostFinishedAttack();
            Invoke("StartTeleport", explodeTeleportDelayTime);
            isInAttack = true;
            animator.SetFloat("WalkingAnimSpeed", 0f);
        }
        public void GhostFinishedAttack()
        {
            SetNowAttackWaitTime();
            SetNowAttack(0);
        }
    }
}
