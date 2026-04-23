using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public enum PlayerButtons
{
    Ready = 0,
    Fire = 1,
}
public struct PlayerInput : INetworkInput
{
    public Vector3 MoveDirection;
    public float AimYaw;
    public NetworkButtons Buttons;
}
public class InputManager : Singleton<InputManager>
{
    private PlayerInput playerInput;

    public PlayerInput PlayerInput => playerInput;

    private Camera cachedMainCamera;
    private Transform localPlayerTransform;

    private Camera MainCamera
    {
        get
        {
            if (cachedMainCamera == null)
                cachedMainCamera = Camera.main;

            return cachedMainCamera;
        }
    }

    public void SetLocalPlayerTransform(Transform playerTransform)
    {
        localPlayerTransform = playerTransform;
    }

    public void ClearLocalPlayerTransform(Transform playerTransform)
    {
        if (localPlayerTransform == playerTransform)
            localPlayerTransform = null;
    }

    void Update()
    {
        playerInput.MoveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;

        // Tính yaw từ vị trí chuột và vị trí tank local, không dùng Physics.Raycast.
        Camera cameraRef = MainCamera;
        if (cameraRef != null && localPlayerTransform != null)
        {
            Vector3 mousePos = Input.mousePosition;

            if (mousePos.x >= 0 && mousePos.x <= Screen.width &&
                mousePos.y >= 0 && mousePos.y <= Screen.height)
            {
                Ray ray = cameraRef.ScreenPointToRay(mousePos);
                float directionY = ray.direction.y;

                if (Mathf.Abs(directionY) > 0.0001f)
                {
                    float distance = (localPlayerTransform.position.y - ray.origin.y) / directionY;
                    if (distance > 0f)
                    {
                        Vector3 mouseWorld = ray.origin + ray.direction * distance;
                        Vector3 direction = mouseWorld - localPlayerTransform.position;
                        direction.y = 0f;

                        if (direction.sqrMagnitude > 0.001f)
                        {
                            playerInput.AimYaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                        }
                    }
                }
            }
        }

        //Nhấn Ready
        playerInput.Buttons.Set(PlayerButtons.Ready, Input.GetKey(KeyCode.R));

        //Nhấn Fire
        playerInput.Buttons.Set(PlayerButtons.Fire, Input.GetMouseButton(0));
    }
}
