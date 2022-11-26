using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using UnityEngine;
using MoreMountains.Feedbacks;

namespace DotsEffect
{
    public class BulletShooter : MonoBehaviour
    {
        public Transform shootStartPoint;
        public float shootSpeed = 10f;
        public int shootOnceBulletsCount = 1;
        public MMF_Player shootFeedbacks; 

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Shoot();
            }
        }
        void Shoot()
        {
            EntityQuery bulletEntityQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(typeof(BulletData));
            NativeArray<Entity> bulletsEntities = bulletEntityQuery.ToEntityArray(Allocator.Temp);

            int nowShootOnceBulletsCount = 0;
            foreach (Entity bullet in bulletsEntities)
            {
                BulletMovementAspect bulletMovementAspect = World.DefaultGameObjectInjectionWorld.EntityManager.GetAspect<BulletMovementAspect>(bullet);
                if (!bulletMovementAspect.bulletData.ValueRO.isActive)
                {
                    bulletMovementAspect.transformAspect.Position = shootStartPoint.position;
                    bulletMovementAspect.bulletData.ValueRW.lastPosition = shootStartPoint.position;
                    bulletMovementAspect.bulletData.ValueRW.nowPosition = shootStartPoint.position;
                    bulletMovementAspect.bulletData.ValueRW.targetRotation = shootStartPoint.rotation;
                    bulletMovementAspect.bulletData.ValueRW.velocity = CalculateShootVelocity();
                    bulletMovementAspect.ActivateBulletData(true);
                    nowShootOnceBulletsCount++;

                    if (nowShootOnceBulletsCount >= shootOnceBulletsCount)
                    {
                        shootEffects();
                        break;
                    }
                }
            }

            bulletsEntities.Dispose();
        }
        Vector3 CalculateShootVelocity()
        {
            return transform.forward * shootSpeed;
        }
        void shootEffects()
        {
            shootFeedbacks.PlayFeedbacks();
        }
    }
}