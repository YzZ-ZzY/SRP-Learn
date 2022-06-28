using UnityEngine;
using UnityEngine.Rendering;
public class CustomRenderPipeline : RenderPipeline {


    private CameraRenderer cameraRenderer = new CameraRenderer();

    private bool useDynamicBatching;
    private bool useGPUInstancing;

    public CustomRenderPipeline(bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher) {
        this.useDynamicBatching = useDynamicBatching;
        this.useGPUInstancing = useGPUInstancing;

      GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;

    }

  protected override void Render(ScriptableRenderContext context, Camera[] cameras)
  {

    foreach (var camera in cameras)
    {
      cameraRenderer.Render(context, camera,useDynamicBatching, useDynamicBatching);
    }

	}
}