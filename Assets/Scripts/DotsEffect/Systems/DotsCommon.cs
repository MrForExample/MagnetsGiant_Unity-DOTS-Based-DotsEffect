using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


namespace DotsEffect
{
    public static class DotsCommon
    {
        public static float3 CalculatePDForceStable(float3 targetPos, float3 targetVel, float3 nowPos, float3 nowVel, float frequency, float damping, float dt)
        {
            float kp, kd, ksg, kdg;
            PreCalculatePD(frequency, damping, dt, out kp, out kd);
            FinalCalculatePD(kp, kd, dt, out ksg, out kdg);
            float3 nowPDForce = ksg * (targetPos - nowPos) + kdg * (targetVel - nowVel);
            return nowPDForce;
        }
        static void PreCalculatePD(float frequency, float damping, float dt, out float kp, out float kd)
        {
            kp = (6f*frequency)*(6f*frequency)* 0.25f;
            kd = 4.5f*frequency*damping;
        }
        static void FinalCalculatePD(float kp, float kd, float dt, out float ksg, out float kdg)
        {
            float g = 1 / (1 + kd * dt + kp * dt * dt);
            ksg = kp * g;
            kdg = (kd + kp * dt) * g;
        }
    }
}