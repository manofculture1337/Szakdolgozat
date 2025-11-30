using Mirror.BouncyCastle.Asn1.Mozilla;
using UnityEngine;


public interface IInput
{
    public abstract Vector3 GetPos();

    public abstract bool GetClickDown();

    public abstract bool GetClick();

    public abstract bool GetSecondaryClick();

    public abstract bool GetClickUp();

    public abstract Ray GetRay(Camera camera);

    public abstract Vector3 GetDelta();

    public abstract void SetPrevPosition();

    public abstract Vector3 GetWorldPosition(Camera camera);

    public abstract bool IsOverUI();

    public abstract void CheckKeyForModeSwitch(ControlObject controller);
}