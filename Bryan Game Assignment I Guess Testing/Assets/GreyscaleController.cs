using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GreyscaleController : MonoBehaviour
{
    public Material greyscaleMaterial;
    [Range(0f, 1f)] public float greyscaleStage = 0f;
    public float greyscaleScale = 1f;
    public float contrast = 1f;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!greyscaleMaterial || !ApplyShaderProperties()) {
            Graphics.Blit(source, destination);
            return;
        }
        Graphics.Blit(source, destination, greyscaleMaterial);
    }

    private bool ApplyShaderProperties() {
        if (!greyscaleMaterial) return false;

        if (greyscaleMaterial.HasProperty("_Scale") && greyscaleMaterial.HasProperty("_Stage")) {
            greyscaleMaterial.SetFloat("_Scale", greyscaleScale);
            greyscaleMaterial.SetFloat("_Contrast", contrast);
            greyscaleMaterial.SetFloat("_Stage", greyscaleStage);
            return true;
        }


        return false;
    }
}
