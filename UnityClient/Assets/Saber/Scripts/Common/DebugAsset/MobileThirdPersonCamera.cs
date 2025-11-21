using UnityEngine;

public class MobileThirdPersonCamera : MonoBehaviour
{
    public Transform target;
    public float distance = 5.0f;
    public float height = 2.0f;
    public float rotationSpeed = 2.0f;
    public float zoomSpeed = 0.5f;
    public float minDistance = 2.0f;
    public float maxDistance = 10.0f;

    private float currentX;
    private float currentY;

    void Update()
    {
        HandleTouchInput();
        UpdateCameraPosition();
    }

    void HandleTouchInput()
    {
        // 单指触摸 - 旋转摄像机
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                currentX += touch.deltaPosition.x * rotationSpeed * 0.02f;
                currentY -= touch.deltaPosition.y * rotationSpeed * 0.02f;
                currentY = Mathf.Clamp(currentY, -80, 80);
            }
        }

        // 双指触摸 - 缩放
        if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
            {
                Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
                Vector2 touch2PrevPos = touch2.position - touch2.deltaPosition;

                float prevTouchDeltaMag = (touch1PrevPos - touch2PrevPos).magnitude;
                float touchDeltaMag = (touch1.position - touch2.position).magnitude;

                float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

                distance += deltaMagnitudeDiff * zoomSpeed * 0.01f;
                distance = Mathf.Clamp(distance, minDistance, maxDistance);
            }
        }
    }

    void UpdateCameraPosition()
    {
        if (target == null)
            return;

        Vector3 direction = new Vector3(0, 0, -distance);
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 position = target.position + rotation * direction;
        position.y += height;

        transform.position = position;
        transform.LookAt(target.position + Vector3.up * height * 0.5f);
    }
}