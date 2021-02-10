using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace World
{
    public class LightGroup
    {
        public List<WorldLight> Lights;
    }

    public class LightManager : MonoBehaviour
    {
        public LightManager Instance;

        public List<LightGroup> LightGroups;

        void Awake()
        {
            if (Instance) // Do not allow multiple instances!
            {
                Debug.LogError($"LightManager [init]: Only one instance of a LightSystem can exist! [current: {Instance.gameObject.name}]");
                return;
            }
            Instance = this;
        }


    }
} // endof namespace