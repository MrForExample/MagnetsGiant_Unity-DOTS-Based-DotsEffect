using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using VoxelSystem;

namespace DotsEffect
{
    public class GhostSpawnerAuthoring : MonoBehaviour
    {
        public Mesh targetMesh;
        public int resolution = 30;
        public float ghostScale = 1f;
        public float explodeSpeed = 60f;
        public float maxTeleportTime = 2f;
        public float teleportGateTimeRandomRadius = 0.3f;
        public GameObject ghost;
        public GameObject dot;
    }

    public class GhostSpawnerBaker : Baker<GhostSpawnerAuthoring>
    {
        public override void Bake(GhostSpawnerAuthoring authoring)
        {
            // Record bones position and offsets
            Transform rootBone = authoring.ghost.transform;
            foreach (Transform childTransform in rootBone)
            {
                if (!childTransform.GetComponent<SkinnedMeshRenderer>())
                {
                    rootBone = childTransform;
                    break;
                }
            }

            var allAuthoringBones = rootBone.GetComponentsInChildren<GhostBoneDataAuthoring>();
            List<Transform> allBones = new List<Transform>();
            foreach (var authoringBone in allAuthoringBones)
                allBones.Add(authoringBone.transform);

            allBones.Remove(rootBone);
            Transform[] allChildBones = allBones.ToArray();
            float3[] allChildBonesPositions = new float3[allChildBones.Length];
            float3[] allChildBonesOffsets = new float3[allChildBones.Length];

            for (int i = 0; i < allChildBones.Length; i++)
            {
                Transform bone = allChildBones[i];
                allChildBonesPositions[i] = bone.position;
                allChildBonesOffsets[i] = bone.parent.position - bone.position;
            }

            // Calculate dots positions
            List<Voxel_t> voxels;
            float unit;

            CPUVoxelizer.Voxelize(
                authoring.targetMesh,   // a target mesh
                authoring.resolution,  // # of voxels for largest AABB bounds
                out voxels,
                out unit
            );
            Debug.Log("Dots Count: " + voxels.Count + "; Dot Scale: " + unit);

            unit *= authoring.ghostScale;

            NativeArray<FixedString32Bytes> bonesNames = new NativeArray<FixedString32Bytes>(voxels.Count, Allocator.Persistent);
            NativeArray<float3> dotsPositions = new NativeArray<float3>(voxels.Count, Allocator.Persistent);
            NativeArray<float3> dotsLocalPositions = new NativeArray<float3>(voxels.Count, Allocator.Persistent);
            NativeArray<float3> dotsExplodeVelocities = new NativeArray<float3>(voxels.Count, Allocator.Persistent);

            float maxDotHeight = Mathf.NegativeInfinity, minDotHeight = Mathf.Infinity;
            for (int i = 0; i < voxels.Count; i++)
            {
                float3 dotPosition = voxels[i].position;
                dotsPositions[i] = dotPosition;
    
                FixedString32Bytes targetBoneName;
                float3 dotLocalPosition;
                float3 dotsExplodeVelocity;
                CalculateDotParentAndLocalPosition(dotPosition, allChildBones, allChildBonesPositions, allChildBonesOffsets, authoring.explodeSpeed, out targetBoneName, out dotLocalPosition, out dotsExplodeVelocity);
                bonesNames[i] = targetBoneName;
                dotsLocalPositions[i] = dotLocalPosition;
                dotsExplodeVelocities[i] = dotsExplodeVelocity;

                if (dotPosition.y > maxDotHeight)
                    maxDotHeight = dotPosition.y;
                if (dotPosition.y < minDotHeight)
                    minDotHeight = dotPosition.y;
            }

            NativeArray<float> dotsTeleportGateTimes = new NativeArray<float>(voxels.Count, Allocator.Persistent);
            for (int i = 0; i < dotsPositions.Length; i++)
                dotsTeleportGateTimes[i] = Mathf.Lerp(authoring.maxTeleportTime, 0f, Mathf.InverseLerp(minDotHeight, maxDotHeight, dotsPositions[i].y))
                                         + UnityEngine.Random.Range(-authoring.teleportGateTimeRandomRadius, authoring.teleportGateTimeRandomRadius);

            AddComponent(new GhostSpawner{
                bonesNames = bonesNames,
                dotsPositions = dotsPositions,
                dotsLocalPositions = dotsLocalPositions,
                dotsExplodeVelocities = dotsExplodeVelocities,
                dotsTeleportGateTimes = dotsTeleportGateTimes,
                unit = unit,
                ghostScale = authoring.ghostScale,
                ghost = GetEntity(authoring.ghost),
                dot = GetEntity(authoring.dot),
                teleportTime = authoring.maxTeleportTime
            });
        }

        void CalculateDotParentAndLocalPosition(float3 dotPosition, Transform[] allChildBones, float3[] allChildBonesPositions, float3[] allChildBonesOffsets, float explodeSpeed, out FixedString32Bytes targetBoneName, out float3 dotLocalPosition, out float3 dotsExplodeVelocity)
        {
            float nowMinBoneProjectDistanceSq = math.INFINITY;
            int parentBoneIndex = 0;
            float3 minProjectPoint = float3.zero;
            
            for (int i = 0; i < allChildBones.Length; i++)
            {
                float3 lineStart = allChildBonesPositions[i];
                float3 lineDirection = allChildBonesOffsets[i];
                float lineLength = math.length(lineDirection);
                lineDirection = math.normalize(lineDirection);

                float projectLength = math.clamp(math.dot(dotPosition - lineStart, lineDirection), 0f, lineLength);
                float3 projectPoint = lineStart + lineDirection * projectLength;
                float nowBoneProjectDistanceSq = math.distancesq(projectPoint, dotPosition);
                if (nowBoneProjectDistanceSq < nowMinBoneProjectDistanceSq)
                {
                    parentBoneIndex = i;
                    nowMinBoneProjectDistanceSq = nowBoneProjectDistanceSq;
                    minProjectPoint = projectPoint;
                }
            }

            targetBoneName = allChildBones[parentBoneIndex].parent.name;
            dotLocalPosition = allChildBones[parentBoneIndex].parent.InverseTransformPoint(dotPosition);
            dotsExplodeVelocity = math.normalize(dotPosition - minProjectPoint) * explodeSpeed;
        }
    }
}