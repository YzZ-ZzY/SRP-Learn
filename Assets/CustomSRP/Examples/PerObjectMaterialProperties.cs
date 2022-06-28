using UnityEngine;
// 对象必须是Mesh或者Skinned Mesh，不能是粒子特效
// Shader必须与SRP Batcher兼容，所有的在URP或者HDRP中的Lit和Unlit Shader都满足这个需求（除了这些shader的粒子特效版本）
// 渲染的对象不使用MaterialPropertyBlocks

// 要使自定义着色器与 SRP Batcher 兼容，它必须满足以下要求：
// 着色器必须在名为 的单个常量缓冲区中声明所有内置引擎属性UnityPerDraw。例如unity_ObjectToWorld， 或unity_SHAr。
// 着色器必须在名为 的单个常量缓冲区中声明所有材质属性UnityPerMateria


[DisallowMultipleComponent]
public class PerObjectMaterialProperties : MonoBehaviour
{

  static int
    baseColorId = Shader.PropertyToID("_BaseColor"),
    cutoffId = Shader.PropertyToID("_Cutoff"),
    metallicId = Shader.PropertyToID("_Metallic"),
    smoothnessId = Shader.PropertyToID("_Smoothness");

  static MaterialPropertyBlock block;

  [SerializeField]
  Color baseColor = Color.white;

  [SerializeField, Range(0f, 1f)]
  float alphaCutoff = 0.5f, metallic = 0f, smoothness = 0.5f;

  void Awake()
  {
    OnValidate();
  }

  void OnValidate()
  {
    if (block == null)
    {
      block = new MaterialPropertyBlock();
    }
    block.SetColor(baseColorId, baseColor);
    block.SetFloat(cutoffId, alphaCutoff);
    block.SetFloat(metallicId, metallic);
    block.SetFloat(smoothnessId, smoothness);
    GetComponent<Renderer>().SetPropertyBlock(block);
  }
}