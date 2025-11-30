using UnityEngine;
using Mirror;
using Dummiesman;

public class MyOBJLoader : NetworkBehaviour
{
    private void Start()
    {
        var objPath = "C:/Users/szeke/Downloads/meta_room_mesh.obj";

        var obj = new OBJLoader().Load(objPath);
        obj.transform.localScale = Vector3.one;

        foreach (var renderer in obj.GetComponentsInChildren<Renderer>())
        {
            foreach (var mat in renderer.materials)
            {
                var texture = mat.mainTexture;

                mat.shader = Shader.Find("Universal Render Pipeline/Lit");

                if (texture != null)
                {
                    mat.SetTexture("_BaseMap", texture);
                }
            }
        }
    }
}
