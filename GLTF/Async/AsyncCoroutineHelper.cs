using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;
using RedLoader;
using UnityEngine;

namespace UnityGLTF
{
    public class AsyncCoroutineHelper : MonoBehaviour
    {
        public float BudgetPerFrameInSeconds = 0.01f;

        private WaitForEndOfFrame _waitForEndOfFrame = new WaitForEndOfFrame();
        private float _timeout;

        public async Task YieldOnTimeout()
        {
            if (Time.realtimeSinceStartup > _timeout)
            {
                await Task.Yield();
                _timeout = Time.realtimeSinceStartup + BudgetPerFrameInSeconds;
            }
        }

        private void Start()
        {
            _timeout = Time.realtimeSinceStartup + BudgetPerFrameInSeconds;

            ResetFrameTimeout().RunCoro();
        }

        private IEnumerator ResetFrameTimeout()
        {
            while (true)
            {
                yield return _waitForEndOfFrame;
                _timeout = Time.realtimeSinceStartup + BudgetPerFrameInSeconds;
            }
        }
    }
}
