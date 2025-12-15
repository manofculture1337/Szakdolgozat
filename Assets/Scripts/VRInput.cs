using Oculus.Interaction;
using UnityEngine;


public class VRInput : IInput
{
    private RayInteractor _rightInteractor;

    private Vector3 _prevPosition;

    private LayerMask _uiBackstopLayerMask;


    public VRInput(RayInteractor rightInteractor)
    {
        _rightInteractor = rightInteractor;
        _uiBackstopLayerMask = LayerMask.GetMask("UIBackstop");
    }

    public Vector3 GetPos()
    {
        return _rightInteractor.Origin;
    }

    public bool GetClickDown()
    {
        return OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger);
    }

    public bool GetClick()
    {
        return OVRInput.Get(OVRInput.RawButton.RIndexTrigger);
    }

    public bool GetSecondaryClick()
    {
        return false;
    }

    public bool GetClickUp()
    {
        return OVRInput.GetUp(OVRInput.RawButton.RIndexTrigger);
    }

    public Ray GetRay(Camera camera)
    {
        return _rightInteractor.Ray;
    }

    public Vector3 GetDelta()
    {
        return _rightInteractor.Origin - _prevPosition;
    }

    public void SetPrevPosition()
    {
        _prevPosition = _rightInteractor.Origin;
    }

    public Vector3 GetWorldPosition(Camera camera)
    {
        return _rightInteractor.Origin;
    }

    public bool IsOverUI()
    {
        return Physics.Raycast(_rightInteractor.Ray, out var hit, Mathf.Infinity, _uiBackstopLayerMask);
    }

    public void CheckKeyForModeSwitch(ControlObject controller)
    {
        if (OVRInput.GetDown(OVRInput.RawButton.B))
        {
            controller.CurrentMode = controller.CurrentMode == TransformMode.None ? TransformMode.Move : TransformMode.None;
        }
        else if (OVRInput.GetDown(OVRInput.RawButton.A))
        {
            controller.CurrentMode = controller.CurrentMode == TransformMode.Rotate ? TransformMode.Scale : TransformMode.Rotate;
        }
    }
}