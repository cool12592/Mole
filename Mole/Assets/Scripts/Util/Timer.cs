using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Timer
{
    public struct TimerStruct
    {
        private readonly float coolTime;
        public float coolTimer { get; private set; }

        public TimerStruct(float cooltime_)
        {
            this.coolTime = cooltime_;
            this.coolTimer = 0f;
        }

        public void ResetCoolTime()
        {
            coolTimer = coolTime;
        }

        public bool isCoolTime()
        {
            return coolTimer > 0f ? true : false;
        }

        public void RunTimer()
        {
            coolTimer -= Time.deltaTime;
        }
    }
}
