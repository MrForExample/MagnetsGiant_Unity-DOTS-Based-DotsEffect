using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct BulletData : IComponentData
{
    public float maxActiveTime;
    public bool isActive;
    public float activeTime;
    public float3 velocity;
    public float3 nowPosition;
    public float3 lastPosition;
    public quaternion targetRotation;
}
