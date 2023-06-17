using System;
using BepInEx;
using UnityEngine;
using BepInEx.Logging;

namespace Gupdate
{
    public class ModBehaviour : MonoBehaviour
    {
        private byte ilindex;
        private bool _ilfound;

        protected bool ilfound
        {
            get => _ilfound;
            set
            {
                _ilfound = value;
                if (!_ilfound)
                {
                    LogWarning($"{GetType().Name}: IL failed to find match at index {ilindex}");
                }
                ilindex++;
            }
        }

        protected void Log(LogLevel level, object data) => Gupdate.Loggup.Log(level, data);

        protected void LogDebug(object data) => Gupdate.Loggup.LogDebug(data);

        protected void LogError(object data) => Gupdate.Loggup.LogError(data);

        protected void LogFatal(object data) => Gupdate.Loggup.LogFatal(data);

        protected void LogInfo(object data) => Gupdate.Loggup.LogInfo(data);

        protected void LogMessage(object data) => Gupdate.Loggup.LogMessage(data);

        protected void LogWarning(object data) => Gupdate.Loggup.LogWarning(data);

        public virtual (string, string)[] GetLang() => Array.Empty<(string, string)>();
    }
}
