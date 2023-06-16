using System;
using BepInEx;
using UnityEngine;

namespace Gupdate
{
    [BepInPlugin("com.groovesalad.Gupdate", "Gupdate", "1.0.0")]
    public class Gupdate : BaseUnityPlugin
    {
        public void Awake()
        {
            GameObject gupdate = new GameObject(nameof(Gupdate));
            DontDestroyOnLoad(gupdate);
        }
    }
}
