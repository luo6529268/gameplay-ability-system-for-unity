using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NTSD.App
{
    public class InputModule
    {
        private NTSDInputConfig _NTSDInputConfig;
        public InputModule()
        {
            _NTSDInputConfig ??= new NTSDInputConfig();
        }

        public InputActionMap GetActionMapByPlayerID(int playerID) 
        {
            return _NTSDInputConfig.asset.FindActionMap($"Player_{playerID}");
        }
    }
}
