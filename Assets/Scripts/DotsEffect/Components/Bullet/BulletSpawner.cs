using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace DotsEffect
{
    public struct BulletSpawner : IComponentData
    {
        public float bulletRadius;
        public int maxBulletsCount;
        public Entity bullet;
    }
}
