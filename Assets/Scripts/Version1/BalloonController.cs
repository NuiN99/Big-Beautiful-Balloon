using NuiN.NExtensions;
using UnityEngine;

public class BalloonController : MonoBehaviour
{
    [SerializeField] BalloonMovement movement;
    
    PlayerControls _controls;

    void Awake()
    {
        PlayerCamera.Instance.AssignTrackingTarget(transform);
    }

    void OnEnable()
    {
        _controls = new PlayerControls();
        _controls.Enable();
    }

    void OnDisable()
    {
        _controls.Disable();
        _controls = null;
    }

    void Update()
    {
        if (_controls.Balloon.Move.IsPressed())
        {
            Vector2 input = _controls.Balloon.Move.ReadValue<Vector2>();
            Vector3 moveDirection = ((PlayerCamera.Instance.Forward * input.y) + (PlayerCamera.Instance.Right * input.x)).With(y:0);
            
            movement.Move(moveDirection);
        }

        if (_controls.Balloon.Inflate.IsPressed())
        {
            movement.Inflate();
        }

        if (_controls.Balloon.Deflate.IsPressed())
        {
            transform.position = new Vector3(0, 3, 0);
            GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        }
    }
}