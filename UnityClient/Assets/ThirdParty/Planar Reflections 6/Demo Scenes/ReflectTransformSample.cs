namespace PlanarReflections6.Demos {

    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [ExecuteAlways]
    public class ReflectTransformSample : MonoBehaviour {

        [SerializeField] protected Transform _originalTransform;
        [SerializeField] protected PlanarReflectionRenderer _reflectionRenderer;


        private void LateUpdate() {
            if (_reflectionRenderer && _originalTransform ) {
                _reflectionRenderer.ReflectTransform( _originalTransform, transform );
            }
        }

    }

}