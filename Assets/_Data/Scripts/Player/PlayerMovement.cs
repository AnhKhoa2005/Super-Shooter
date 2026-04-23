using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerMovement : LoadComponents
{
    [SerializeField] private NetworkCharacterController networkCharacterController;

    protected override void LoadComponent()
    {
        if (networkCharacterController == null)
            networkCharacterController = GetComponent<NetworkCharacterController>();
    }

    protected override void LoadComponentRuntime()
    {

    }

    public void Move(Vector3 moveDirection)
    {
        Vector3 movement = moveDirection;
        networkCharacterController.Move(new Vector3(movement.x, 0, movement.z));
    }

    public void Teleport(Vector3 position, Quaternion rotation)
    {
        networkCharacterController.Teleport(position, rotation);
    }

    public void SetGravity(float gravity)
    {
        networkCharacterController.gravity = gravity;
    }

    public void SetCollision(bool enableCollision)
    {
        networkCharacterController.enabled = enableCollision;
    }
}
