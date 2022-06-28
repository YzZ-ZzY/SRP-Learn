using UnityEngine;
using UnityEngine.Rendering;
[CreateAssetMenu(menuName = "Rendering/Test Render Pipeline")]
public class TestRenderPipelineAsset : RenderPipelineAsset
{

  public Cubemap diffuse;

  public Cubemap specular;

  public Texture brdfLut;
  protected override RenderPipeline CreatePipeline()
  {

    var rp = new TestRenderPipeline();
    rp.diffuseIBL = diffuse;
    rp.specularIBL = specular;
    rp.brdfLut = brdfLut;
    return rp;
  }
}