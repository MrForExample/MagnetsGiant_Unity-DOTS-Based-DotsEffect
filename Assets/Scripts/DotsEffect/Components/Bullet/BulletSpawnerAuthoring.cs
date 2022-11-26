using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace DotsEffect
{
    public class BulletSpawnerAuthoring : MonoBehaviour
    {
        public float bulletRadius = 1f;
        public int maxBulletsCount;
        public GameObject bullet;
    }
    public class BulletSpawnerBaker : Baker<BulletSpawnerAuthoring>
    {
        public override void Bake(BulletSpawnerAuthoring authoring)
        {
            AddComponent(new BulletSpawner{
                bulletRadius = authoring.bulletRadius,
                maxBulletsCount = authoring.maxBulletsCount,
                bullet = GetEntity(authoring.bullet)
            });
        }
    }
}
