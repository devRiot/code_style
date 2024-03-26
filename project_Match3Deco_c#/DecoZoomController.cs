using System.Collections;
using UnityEngine;

namespace devRiot.Deco
{
    public class DecoZoomController : MonoBehaviour
    {
        private Transform _target; // 타겟
        /*
        private float zoomSpeed = 0.8f; // 줌인 속도
        private float zoomLimit = 0.7f; // 줌인 제한 거리
        private float smoothTime = 1.0f;
        private float zoomMoved = 0f;
        private Vector3 initialPosition; // 초기 위치
        */
        private Quaternion initialRotation; // 초기 회전
        private float initialOrthographicSize;

        private Camera _camera;

        private bool _isMoving = false;
        public bool IsMoving { get { return _isMoving; } }

        private void Start()
        {
            //initialPosition = transform.position;
            initialRotation = transform.rotation;

            _camera = GetComponent<Camera>();
            if (_camera.orthographic)
                initialOrthographicSize = _camera.orthographicSize;

            iTween.Init(gameObject);
        }

        void OnTweenComplete()
        {
            Debug.Log("OnTweenComplete");
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }

        public void ZoomInCamera(float zoomRatio, float duration)
        {
            _isMoving = true;

            float targetZoom = GetComponent<Camera>().orthographicSize - zoomRatio;
            ZoomIn(targetZoom, duration);
        }

        // 초기 위치와 회전으로 리셋
        public void ResetCamera(float duration)
        {
            _isMoving = true;

            ZoomOut(initialOrthographicSize, duration);
        }

        public void ZoomIn(float zoomInSize, float zoomTime)
        {
            iTween.ValueTo(gameObject, iTween.Hash(
                "from", GetComponent<Camera>().orthographicSize,
                "to", zoomInSize,
                "time", zoomTime,
                "onupdate", "OnZoomUpdate",
                "oncomplete", "OnZoomComplete"
            ));

            Vector3 direction = _target.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

            Hashtable hash = new Hashtable();
            hash.Add("rotation", targetRotation.eulerAngles);
            hash.Add("time", zoomTime);
            hash.Add("easetype", iTween.EaseType.easeInOutSine);
            iTween.RotateTo(gameObject, hash);
        }

        public void ZoomOut(float zoomOutSize, float zoomTime)
        {
            iTween.ValueTo(gameObject, iTween.Hash(
                "from", GetComponent<Camera>().orthographicSize,
                "to", zoomOutSize,
                "time", zoomTime,
                "onupdate", "OnZoomUpdate",
                "oncomplete", "OnResetComplete"
            ));


            Hashtable hash = new Hashtable();
            hash.Add("rotation", initialRotation.eulerAngles);
            hash.Add("time", zoomTime);
            hash.Add("easetype", iTween.EaseType.easeInOutSine);
            iTween.RotateTo(gameObject, hash);
        }

        void OnZoomUpdate(float value)
        {
            GetComponent<Camera>().orthographicSize = value;
        }

        void OnZoomComplete()
        {
            Debug.Log("OnZoomComplete Complete!");
            _isMoving = false;
        }

        void OnResetComplete()
        {
            Debug.Log("OnResetComplete Complete!");
            _isMoving = false;
        }
    }
}