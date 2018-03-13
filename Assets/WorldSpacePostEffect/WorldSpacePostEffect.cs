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

    //matrices updated each frame
    Matrix4x4 leftToWorld;
    Matrix4x4 rightToWorld;
    Matrix4x4 leftEye;
    Matrix4x4 rightEye;

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
            // Left and Right Eye inverse View Matrices
            leftToWorld = Camera.GetStereoViewMatrix(Camera.StereoscopicEye.Left).inverse;
            rightToWorld = Camera.GetStereoViewMatrix(Camera.StereoscopicEye.Right).inverse;
            Material.SetMatrix("_LeftEyeToWorld", leftToWorld);
            Material.SetMatrix("_RightEyeToWorld", rightToWorld);

            leftEye = Camera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
            rightEye = Camera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);

            // Compensate for RenderTexture...
            leftEye = GL.GetGPUProjectionMatrix(leftEye, true).inverse;
            rightEye = GL.GetGPUProjectionMatrix(rightEye, true).inverse;
            // Negate [1,1] to reflect Unity's CBuffer state
            leftEye[1, 1] *= -1;
            rightEye[1, 1] *= -1;

            Material.SetMatrix("_LeftEyeProjection", leftEye);
            Material.SetMatrix("_RightEyeProjection", rightEye);
        }
        else
        {
            leftToWorld = Camera.cameraToWorldMatrix;
            Material.SetMatrix("_LeftEyeToWorld", leftToWorld);

            leftEye = Camera.projectionMatrix;
            leftEye = GL.GetGPUProjectionMatrix(leftEye, true).inverse;
            leftEye[1, 1] *= -1;

            Material.SetMatrix("_LeftEyeProjection", leftEye);
        }
    }

    //simplest possible on render image just blits from source to dest using our shader
    [ImageEffectOpaque]
    void OnRenderImage(RenderTexture source, RenderTexture dest)
    {
        Graphics.Blit(source, dest, Material);
    }
}
