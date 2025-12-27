namespace PlanarReflections6.Demos {

    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [ExecuteAlways]
    public class IsVisibleSample : MonoBehaviour {

        [SerializeField] protected PlanarReflectionCaster _onCaster;
        [SerializeField] protected Camera _fromCamera;

        [SerializeField] protected Material _visibleMat, _notVisibleMat;

        private Renderer _myRenderer;

        private void OnValidate() {

            _myRenderer = GetComponent<Renderer>();

        }



        private void LateUpdate() {

            if ( _myRenderer && _onCaster && _fromCamera && _visibleMat && _notVisibleMat ) {

                _myRenderer.sharedMaterial = _onCaster.IsVisible( _myRenderer, _fromCamera ) ? _visibleMat : _notVisibleMat;

            }

        }


    }

}