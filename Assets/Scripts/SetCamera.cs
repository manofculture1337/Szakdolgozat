using UnityEngine;
using UnityEngine.Animations;

public class SetCamera : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        var camera = GameObject.Find("CenterEyeAnchor");
        var posCon = this.GetComponent<PositionConstraint>();
        var SourceCamera = new ConstraintSource { sourceTransform = camera.transform, weight = 1.0f };
        posCon.AddSource(SourceCamera);
        var rotCon = this.GetComponent<RotationConstraint>();
        rotCon.AddSource(SourceCamera);


    }
}
