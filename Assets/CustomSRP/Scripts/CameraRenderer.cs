using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer
{

  private ScriptableRenderContext context;
  private Camera camera;
  private Lighting lighting = new Lighting();
  private const string bufferName = "Render Camera";
  private CommandBuffer buffer = new CommandBuffer { name = bufferName };
  private CullingResults cullingResults;

  static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit"),
                     litShaderTagId = new ShaderTagId("CustomLit");
  partial void DrawUnsupportedShaders();
  partial void DrawGizmos();
  partial void PrepareForSceneWindow();
  partial void PrepareBuffer();
  public void Render(ScriptableRenderContext context, Camera camera, bool useDynamicBatching, bool useGPUInstancing)
  {
    this.context = context;
    this.camera = camera;
    PrepareBuffer();
    PrepareForSceneWindow();
    if (!Cull())
    {
      return;
    }


    Setup();
    lighting.Setup(context, cullingResults);
    DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);
    DrawUnsupportedShaders();

    DrawGizmos();
    Submit();
  }

  void Setup()
  {
    context.SetupCameraProperties(camera);

    CameraClearFlags flags = camera.clearFlags;
    buffer.ClearRenderTarget(
        flags <= CameraClearFlags.Depth,
        flags == CameraClearFlags.Color,
        flags == CameraClearFlags.Color ?
            camera.backgroundColor.linear : Color.clear
    );

    // buffer.ClearRenderTarget(true, true, Color.clear);
    buffer.BeginSample(SampleName);
    ExcuteBuffer();

  }

  void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing)
  {
    var sortingSettings = new SortingSettings(camera)
    {
      criteria = SortingCriteria.CommonOpaque
    };
    var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings)
    {
      enableDynamicBatching = useDynamicBatching,
      enableInstancing = useGPUInstancing
    };
    drawingSettings.SetShaderPassName(1, litShaderTagId);

    // 非透明物体
    var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
    context.DrawRenderers(
        cullingResults, ref drawingSettings, ref filteringSettings
    );
    // 天空盒
    context.DrawSkybox(camera);
    // 透明物体
    filteringSettings = new FilteringSettings(RenderQueueRange.transparent);
    context.DrawRenderers(
        cullingResults, ref drawingSettings, ref filteringSettings
    );
  }

  void ExcuteBuffer()
  {
    context.ExecuteCommandBuffer(buffer);
    buffer.Clear();
  }

  void Submit()
  {
    buffer.EndSample(SampleName);
    ExcuteBuffer();
    context.Submit();
  }

  void DrawSkybox()
  {

  }
  bool Cull()
  {
    if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
    {
      cullingResults = context.Cull(ref p);
      return true;
    }

    return false;
  }
}