using System;
using System.Collections;

namespace RedLoader
{
    public class Coroutines
    {
        /// <summary>
        /// Start a new coroutine.<br />
        /// Coroutines are called at the end of the game Update loops.
        /// </summary>
        /// <param name="routine">The target routine</param>
        /// <returns>An object that can be passed to Stop to stop this coroutine</returns>
        public static object Start(IEnumerator routine)
        {
            if (SupportModule.Interface == null)
                throw new NotSupportedException("Support module must be initialized before starting coroutines");
            return SupportModule.Interface.StartCoroutine(routine);
        }

        /// <summary>
        /// Stop a currently running coroutine
        /// </summary>
        /// <param name="coroutineToken">The coroutine to stop</param>
        public static void Stop(object coroutineToken)
        {
            if (SupportModule.Interface == null)
                throw new NotSupportedException("Support module must be initialized before starting coroutines");
            SupportModule.Interface.StopCoroutine(coroutineToken);
        }
        
        public class CoroutineToken
        {
            private object _token;
            
            public bool IsValid => _token != null;

            public CoroutineToken(object token)
            {
                _token = token;
            }

            public void Stop()
            {
                if (!IsValid)
                    return;

                Coroutines.Stop(_token);
                _token = null;
            }

            public static implicit operator bool(CoroutineToken token) => token?.IsValid ?? false;
        }
    }

    public static class CoroutineExtensions
    {
        public static Coroutines.CoroutineToken RunCoro(this IEnumerator coro)
        {
            return new Coroutines.CoroutineToken(Coroutines.Start(coro));
        }
    }
}