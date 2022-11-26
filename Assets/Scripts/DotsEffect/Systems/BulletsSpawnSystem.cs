using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace DotsEffect
{
    public partial class BulletsSpawnSystem : SystemBase
    {
        protected override void OnStartRunning()
        {
            EntityQuery bulletEntityQuery = EntityManager.CreateEntityQuery(typeof(BulletData));

            BulletSpawner bulletSpawner = SystemAPI.GetSingleton<BulletSpawner>();

            while (bulletEntityQuery.CalculateEntityCount() < bulletSpawner.maxBulletsCount)
            {
                Entity bullet = EntityManager.Instantiate(bulletSpawner.bullet);
            }
        }
        protected override void OnUpdate()
        {
            
        }
    }
}
