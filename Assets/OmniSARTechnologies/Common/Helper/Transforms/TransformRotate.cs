//
// Transform Rotate
//
// Author     : Alex Tuduran
// Copyright  : OmniSAR Technologies
//

// uncomment this to execute rotation while the game is not running
//#define ___EXECUTE_IN_EDITOR___

using UnityEngine;

namespace OmniSARTechnologies.Helper {
#if ___EXECUTE_IN_EDITOR___
    [ExecuteInEditMode]
#endif // ___EXECUTE_IN_EDITOR___
    public class TransformRotate : MonoBehaviour {
        public Vector3 eulerAnglesSpeed;

        private void Update() {
            transform.RotateAround(transform.position, Vector3.right,   eulerAnglesSpeed.x * Time.deltaTime);
            transform.RotateAround(transform.position, Vector3.up,      eulerAnglesSpeed.y * Time.deltaTime);
            transform.RotateAround(transform.position, Vector3.forward, eulerAnglesSpeed.z * Time.deltaTime);
        }
    }
}