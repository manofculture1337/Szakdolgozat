using UnityEngine;
using Mirror;
using Dummiesman;
using System.IO;

public static class MyOBJLoader
{
    static public void Load()
    {
        var fileName = "received_file.obj";
        
        var obj = new OBJLoader().Load(Path.Combine(Application.persistentDataPath, $"{fileName}"));
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
