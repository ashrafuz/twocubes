using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace AUZ_UTIL {

    public abstract class SlowMono : MonoBehaviour {

        private float m_ElapsedTime = 0.0f;
        private float m_StepTime = 1f;
        private bool m_IsRunning = true;

        public virtual void SetUpdateRateInSeconds (float _sec) {
            m_StepTime = _sec;
        }

        protected abstract void SlowUpdate ();

        protected virtual void Start () {
            SetUpdateRateInSeconds (1);
        }

        protected virtual void Update () {
            if (m_IsRunning) {
                m_ElapsedTime += Time.deltaTime;
                if (m_ElapsedTime > m_StepTime) {
                    SlowUpdate ();
                    m_ElapsedTime = 0;
                }
            }
        }

        //helpers
        protected void PauseSlowUpdate () {
            m_IsRunning = false;
        }

        protected void HaltSlowUpdate () {
            m_IsRunning = false;
            m_ElapsedTime = 0;
        }

        protected void RestartUpdate () {
            m_IsRunning = true;
            m_ElapsedTime = 0;
        }
    }

}