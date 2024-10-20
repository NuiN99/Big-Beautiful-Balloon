﻿using System.Collections;
using System.Linq;
using NuiN.NExtensions;
using UnityEngine;

public class BalloonMovement_A : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] SoftBody softBody;
    [SerializeField] Rigidbody rb;
    [SerializeField] Collider col;
    
    [Header("Physics")]
    [SerializeField] FloatRange gravityRange;
    [SerializeField] FloatRange dragRange;
    [SerializeField] FloatRange angularDragRange;
    [SerializeField] FloatRange bounceRange;
    [SerializeField] FloatRange frictionRange;
    [SerializeField] float bounceForce;
    
    [Header("Other")]
    [SerializeField] FloatRange speedRange;
    [SerializeField] FloatRange scaleRange;
    [SerializeField] FloatRange camZoomRange;
    [SerializeField] float inflateSpeed;
    [SerializeField] float aimYMult = 4f;
    
    [Header("Deflating")] 
    [SerializeField] float deflateSpeed;
    [SerializeField] FloatRange deflateForceRange;
    [SerializeField] SerializedWaitForSeconds inflateCooldown;

    WaitForFixedUpdate _waitForFixedUpdate;
    bool _isDeflating;
    bool _isAiming;
    float _curSize;
    float _gravity;

    float SizeLerp => _curSize;
    float InverseSizeLerp => 1 - SizeLerp;

    void Start()
    {
        inflateCooldown.Init();
        _waitForFixedUpdate = new WaitForFixedUpdate();
        UpdateValues();
    }

    void FixedUpdate()
    {
        rb.AddForce(Vector3.down * (_gravity * Time.fixedDeltaTime), ForceMode.Acceleration);

        if (Physics.Raycast(transform.position, Vector3.down, 1f))
        {
            rb.AddForce(Vector3.up * (_gravity * SizeLerp * Time.fixedDeltaTime * 1.5f), ForceMode.Acceleration);
        }
        
        Rotate(_isDeflating || _isAiming);
    }

    public void Rotate(bool faceCameraFwd)
    {
        float baseSpringStrength = faceCameraFwd ? 17.5f : 15f;
        float damperStrength = faceCameraFwd ? 2f : 0.75f;

        Vector3 up = transform.up;
        Vector3 fwd = faceCameraFwd ? PlayerCamera.Instance.Forward.With(y: PlayerCamera.Instance.Forward.y * aimYMult).normalized : Vector3.up;
        
        float angleFromUpright = Vector3.Angle(up, fwd);
    
        float springStrength = baseSpringStrength * (angleFromUpright / 90f);
        springStrength = Mathf.Clamp(springStrength, 0, baseSpringStrength * 2);

        var springTorque = springStrength * Vector3.Cross(up, fwd);
        var dampTorque = damperStrength * -rb.angularVelocity;
        rb.AddTorque(springTorque + dampTorque, ForceMode.Acceleration);
    }
    
    void OnCollisionEnter(Collision other)
    {
        Bounce(other);
    }

    public void Inflate(float increase)
    {
        if (_isDeflating) 
            return;
        
        float speed = inflateSpeed * Time.deltaTime * increase * 50.0f;
        _curSize = Mathf.Clamp01(_curSize + speed);
        
        UpdateValues();
    }


    public void Deflate(float rate)
    {
        _isAiming = false;
        if (rate > 0f && _curSize > 0) {

            _curSize -= deflateSpeed * Time.fixedDeltaTime * rate;
            UpdateValues();

            float force = deflateForceRange.Lerp(SizeLerp) * Time.fixedDeltaTime * rate;
            rb.AddForce(transform.up * force, ForceMode.VelocityChange);
            _isAiming = true;

            //yeild return _waitForFixedUpdate;
        }
    }

    //UNUSED
    IEnumerator DeflateRoutine()
    {
        _isDeflating = true;

        while (_curSize > 0)
        {
            _curSize -= deflateSpeed * Time.fixedDeltaTime;
            UpdateValues();

            float force = deflateForceRange.Lerp(SizeLerp) * Time.fixedDeltaTime;
            rb.AddForce(transform.up * force, ForceMode.VelocityChange);

            yield return _waitForFixedUpdate;
        }

        yield return inflateCooldown.Wait;
        
        _isDeflating = false;
    }

    void UpdateValues()
    {
        transform.localScale = Vector3.Lerp(Vector3.one * scaleRange.Min, Vector3.one * scaleRange.Max, SizeLerp);
        rb.linearDamping = dragRange.Lerp(SizeLerp);
        rb.angularDamping = angularDragRange.Lerp(SizeLerp);
        col.material.bounciness = bounceRange.Lerp(SizeLerp);
        col.material.dynamicFriction = frictionRange.Lerp(InverseSizeLerp);
        col.material.staticFriction = frictionRange.Lerp(InverseSizeLerp);
        _gravity = gravityRange.Lerp(SizeLerp);
        PlayerCamera.Instance.SetZoom(camZoomRange.Lerp(SizeLerp * SizeLerp * SizeLerp));
    }

    public void Move(Vector3 direction)
    {
        if (SizeLerp <= 0) return;
        
        float speed = speedRange.Lerp(SizeLerp) * Time.fixedDeltaTime;
        rb.AddForce(direction * speed, ForceMode.Acceleration);
        //softBody.DistributeForce(direction * speed, ForceMode.Acceleration);
    }

    void Bounce(Collision collision)
    {
        ContactPoint contact = collision.GetContact(0);
        Vector3 reflectDirection = Vector3.Reflect(rb.linearVelocity, contact.normal);
        rb.AddForceAtPosition(contact.point, reflectDirection * SizeLerp * bounceForce);
    }

    public void StartAiming()
    {
        _isAiming = true;
    }
}