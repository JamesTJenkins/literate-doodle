using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;

[Serializable, VolumeComponentMenu("Post-processing/Custom/Sobel")]
public sealed class Sobel : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Tooltip("Controls the intensity of the effect.")]
	public ClampedFloatParameter outlineIntensity = new ClampedFloatParameter(0f, 0f, 1f);

    [Tooltip("Controls the outline color.")]
	public ColorParameter outlineColor = new ColorParameter(Color.black);

    [Tooltip("Controls the outline thickness in pixels roughly.")]
    public FloatParameter outlineThickness = new FloatParameter(1f);

	[Tooltip("Linearly scales the depth calculation.")]
    public FloatParameter depthMultiplier = new FloatParameter(1f);
	
	[Tooltip("Bias (i.e. power) applied to the scaled depth value.")]
    public FloatParameter depthBias = new FloatParameter(1f);

	[Tooltip("Linearly scales the normal calculation.")]
	public FloatParameter normalMultiplier = new FloatParameter(1f);

	[Tooltip("Bias (i.e. power) applied to the scaled normal value.")]
	public FloatParameter normalBias = new FloatParameter(1f);

	[Tooltip("Linearly scales the luminance calculation.")]
	public FloatParameter luminanceMultiplier = new FloatParameter(1f);

	[Tooltip("Bias (i.e. power) applied to the scaled luminance value.")]
	public FloatParameter luminanceBias = new FloatParameter(1f);

	[Tooltip("Max Range for the edge detection to be applied.")]
	public FloatParameter maxRange = new FloatParameter(100f);

	[Tooltip("Distance falloff power to make the falloff towards Max Range sharper or more subtle.")]
	public FloatParameter distanceFalloffPower = new FloatParameter(3f);

	Material m_Material;

    public bool IsActive() => m_Material != null && outlineIntensity.value > 0f;

    // Do not forget to add this post process in the Custom Post Process Orders list (Project Settings > Graphics > HDRP Global Settings).
    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

    const string kShaderName = "Hidden/Shader/Sobel";

    public override void Setup()
    {
        if (Shader.Find(kShaderName) != null)
            m_Material = new Material(Shader.Find(kShaderName));
        else
            Debug.LogError($"Unable to find shader '{kShaderName}'. Post Process Volume Sobel is unable to load. To fix this, please edit the 'kShaderName' constant in Sobel.cs or change the name of your custom post process shader.");
    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        if (m_Material == null)
            return;

		m_Material.SetTexture("_MainTex", source);
        m_Material.SetFloat("_OutlineIntensity", outlineIntensity.value);
        m_Material.SetColor("_OutlineColor", outlineColor.value);
        m_Material.SetFloat("_OutlineThickness", outlineThickness.value);
        m_Material.SetFloat("_DepthMultiplier", depthMultiplier.value);
        m_Material.SetFloat("_DepthBias", depthBias.value);
		m_Material.SetFloat("_NormalMultiplier", normalMultiplier.value);
		m_Material.SetFloat("_NormalBias", normalBias.value);
		m_Material.SetFloat("_LuminanceMultiplier", normalMultiplier.value);
		m_Material.SetFloat("_LuminanceBias", normalBias.value);
		m_Material.SetFloat("_MaxRange", maxRange.value);
		m_Material.SetFloat("_DistanceFalloffPower", distanceFalloffPower.value);
		HDUtils.DrawFullScreen(cmd, m_Material, destination, shaderPassId: 0);
    }

    public override void Cleanup()
    {
        CoreUtils.Destroy(m_Material);
    }
}
