using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class WorldSpacePostEffect : MonoBehaviour
{
    //the main post effect shader
    public Shader m_world_space_post_effect_shader = null;

    //stores whether this is a single pass stereo build (not required but useful to be able to access)
    public bool m_single_pass_stereo = false;

    //material field + accessor that creates it when needed (done this way to avoid editor confusion when OnEnable doesn't get called in time)
    Material m_material = null;
    Material Material
    {
        get
        {
            if(m_material == null)
            {
                m_material = new Material(m_world_space_post_effect_shader);
                m_material.hideFlags = HideFlags.DontSave;
            }
            return m_material;
        }
    }

    //camera field + accessor
    Camera m_camera;
    Camera Camera
    {
        get
        {
            if(m_camera == null)
                m_camera = GetComponent<Camera>();
            return m_camera;
        }
    }

    //on validate just reads stereo mode
    private void OnValidate()
    {
        //update the stereo flag in editor
        #if UNITY_EDITOR
        m_single_pass_stereo = UnityEditor.PlayerSettings.stereoRenderingPath == UnityEditor.StereoRenderingPath.SinglePass;
        #endif
    }

    //on disable destroys the material
    protected void OnDisable()
    {
        if (m_material)
            DestroyImmediate(m_material);
    }

    //pre render captures all the necessary matrix info, which is normally mangled by the time OnRenderImage is called
    private void OnPreRender()
    {
        //update this every frame in editor 
        #if UNITY_EDITOR
        m_single_pass_stereo = UnityEditor.PlayerSettings.stereoRenderingPath == UnityEditor.StereoRenderingPath.SinglePass;
        #endif

        //camera must at least be in depth mode
        Camera.depthTextureMode = DepthTextureMode.DepthNormals;

        //cache matrices so they can be used in render image step
        if (Camera.stereoEnabled)
        {
            // Both stereo eye inverse view matrices
            Matrix4x4 left_world_from_view = Camera.GetStereoViewMatrix(Camera.StereoscopicEye.Left).inverse;
            Matrix4x4 right_world_from_view = Camera.GetStereoViewMatrix(Camera.StereoscopicEye.Right).inverse;

            // Both stereo eye inverse projection matrices, plumbed through GetGPUProjectionMatrix to compensate for render texture
            Matrix4x4 left_screen_from_view = Camera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
            Matrix4x4 right_screen_from_view = Camera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
            Matrix4x4 left_view_from_screen = GL.GetGPUProjectionMatrix(left_screen_from_view, true).inverse;
            Matrix4x4 right_view_from_screen = GL.GetGPUProjectionMatrix(right_screen_from_view, true).inverse;

            // Negate [1,1] to reflect Unity's CBuffer state
            left_view_from_screen[1, 1] *= -1;
            right_view_from_screen[1, 1] *= -1;

            // Store matrices
            Material.SetMatrix("_LeftWorldFromView", left_world_from_view);
            Material.SetMatrix("_RightWorldFromView", right_world_from_view);
            Material.SetMatrix("_LeftViewFromScreen", left_view_from_screen);
            Material.SetMatrix("_RightViewFromScreen", right_view_from_screen);
        }
        else
        {
            // Main eye inverse view matrix
            Matrix4x4 left_world_from_view = Camera.cameraToWorldMatrix;

            // Inverse projection matrices, plumbed through GetGPUProjectionMatrix to compensate for render texture
            Matrix4x4 screen_from_view = Camera.projectionMatrix;
            Matrix4x4 left_view_from_screen = GL.GetGPUProjectionMatrix(screen_from_view, true).inverse;

            // Negate [1,1] to reflect Unity's CBuffer state
            left_view_from_screen[1, 1] *= -1;

            // Store matrices
            Material.SetMatrix("_LeftWorldFromView", left_world_from_view);
            Material.SetMatrix("_LeftViewFromScreen", left_view_from_screen);
        }
    }

    //simplest possible on render image just blits from source to dest using our shader
    [ImageEffectOpaque]
    void OnRenderImage(RenderTexture source, RenderTexture dest)
    {
        Graphics.Blit(source, dest, Material);
    }
}
