using UnityEngine;
using UnityEngine.EventSystems;


public class PCInput : IInput
{
    public Vector3 GetPos()
    {
        return Input.mousePosition;
    }

    public bool GetClickDown()
    {
        return Input.GetMouseButtonDown(0);
    }

    public bool GetClick()
    {
        return Input.GetMouseButton(0);
    }

    public bool GetSecondaryClick()
    {
        return Input.GetMouseButton(1);
    }

    public bool GetClickUp()
    {
        return Input.GetMouseButtonUp(0);
    }

    public Ray GetRay(Camera camera)
    {
        return camera.ScreenPointToRay(Input.mousePosition);
    }

    public Vector3 GetDelta()
    {
        return Input.mousePositionDelta;
    }

    public void SetPrevPosition()
    {
        return;
    }

    public Vector3 GetWorldPosition(Camera camera)
    {
        return camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, camera.nearClipPlane + 1f));
    }

    public bool IsOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    public void CheckKeyForModeSwitch(ControlObject controller)
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            controller.CurrentMode = TransformMode.None;
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            controller.CurrentMode = TransformMode.Move;
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            controller.CurrentMode = TransformMode.Rotate;
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            controller.CurrentMode = TransformMode.Scale;
        }
    }
}