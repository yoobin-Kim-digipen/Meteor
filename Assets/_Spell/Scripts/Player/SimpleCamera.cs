using UnityEngine;

public class SimpleCamera : MonoBehaviour
{
    public float moveSpeed = 5f;  // 이동 속도

    void Update()
    {
        float moveX = Input.GetAxis("Horizontal"); // 좌우 (A,D 또는 ←,→)
        float moveZ = Input.GetAxis("Vertical");   // 앞뒤 (W,S 또는 ↑,↓)

        float moveY = 0f;
        if (Input.GetKey(KeyCode.Q)) // Q키 누르면 위로 이동
        {
            moveY = 1f;
        }
        else if (Input.GetKey(KeyCode.E)) // E키 누르면 아래로 이동
        {
            moveY = -1f;
        }

        Vector3 move = transform.right * moveX + transform.forward * moveZ + Vector3.up * moveY;
        transform.position += move * moveSpeed * Time.deltaTime;
    }
}
