using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class XMirror : MonoBehaviour
{
    public bool _disablePixelLights = true;
    public int _textureSize = 256;
    public float _clipPlaneOffset = 0.07f;
    public bool _isFlatMirror = true;

    public LayerMask _reflectLayers = -1;
    public Shader _replaceShader;

    private Hashtable _reflectionCameras = new Hashtable();

    private RenderTexture _reflectionTexture = null;
    private int _oldReflectionTextureSize = 0;

    private static bool _insideRendering = false;

    public void OnWillRenderObject()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (!enabled || !renderer || !renderer.sharedMaterial || !renderer.enabled)
            return;

        Camera cam = Camera.current;
        if (!cam)
            return;

        if (_insideRendering)
            return;
        _insideRendering = true;

        Camera reflectionCamera;
        CreateMirrorObjects(cam, out reflectionCamera);

        Vector3 pos = transform.position;
        Vector3 normal;
        if (_isFlatMirror)
        {
            normal = transform.InverseTransformVector(transform.up).normalized;
        }
        else
        {
            normal = transform.position - cam.transform.position;
            normal.Normalize();
        }
        int oldPixelLightCount = QualitySettings.pixelLightCount;
        if (_disablePixelLights)
            QualitySettings.pixelLightCount = 0;

        UpdateCameraModes(cam, reflectionCamera);

        float d = -Vector3.Dot(normal, pos) - _clipPlaneOffset;
        Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

        Matrix4x4 reflection = Matrix4x4.zero;
        CalculateReflectionMatrix(ref reflection, reflectionPlane);
        Vector3 oldpos = cam.transform.position;
        Vector3 newpos = reflection.MultiplyPoint(oldpos);
        reflectionCamera.worldToCameraMatrix = cam.worldToCameraMatrix * reflection;


        Vector4 clipPlane = CameraSpacePlane(reflectionCamera, pos, normal, 1.0f);
        Matrix4x4 projection = cam.projectionMatrix;
        CalculateObliqueMatrix(ref projection, clipPlane);
        reflectionCamera.projectionMatrix = projection;

        reflectionCamera.cullingMask = ~(1 << 4) & _reflectLayers.value;
        reflectionCamera.targetTexture = _reflectionTexture;
        GL.SetRevertBackfacing(true);
        reflectionCamera.transform.position = newpos;
        Vector3 euler = cam.transform.eulerAngles;
        reflectionCamera.transform.eulerAngles = new Vector3(0, euler.y, euler.z);
        if (_replaceShader != null)
            reflectionCamera.SetReplacementShader(_replaceShader, "");
        reflectionCamera.Render();
        reflectionCamera.transform.position = oldpos;
        GL.SetRevertBackfacing(false);
        Material[] materials = renderer.sharedMaterials;
        foreach (Material mat in materials)
        {
            if (mat.HasProperty("_Ref"))
                mat.SetTexture("_Ref", _reflectionTexture);
        }
        if (_disablePixelLights)
            QualitySettings.pixelLightCount = oldPixelLightCount;

        _insideRendering = false;
    }

    void OnDisable()
    {
        if (_reflectionTexture)
        {
            DestroyImmediate(_reflectionTexture);
            _reflectionTexture = null;
        }
        foreach (DictionaryEntry kvp in _reflectionCameras)
            DestroyImmediate(((Camera)kvp.Value).gameObject);
        _reflectionCameras.Clear();
    }


    private void UpdateCameraModes(Camera src, Camera dest)
    {
        if (dest == null)
            return;

        dest.clearFlags = src.clearFlags;
        dest.backgroundColor = src.backgroundColor;
        if (src.clearFlags == CameraClearFlags.Skybox)
        {
            Skybox sky = src.GetComponent(typeof(Skybox)) as Skybox;
            Skybox mysky = dest.GetComponent(typeof(Skybox)) as Skybox;
            if (!sky || !sky.material)
            {
                mysky.enabled = false;
            }
            else
            {
                mysky.enabled = true;
                mysky.material = sky.material;
            }
        }

        dest.farClipPlane = src.farClipPlane;
        dest.nearClipPlane = src.nearClipPlane;
        dest.orthographic = src.orthographic;
        dest.fieldOfView = src.fieldOfView;
        dest.aspect = src.aspect;
        dest.orthographicSize = src.orthographicSize;
        dest.renderingPath = src.renderingPath;
    }


    private void CreateMirrorObjects(Camera currentCamera, out Camera reflectionCamera)
    {
        reflectionCamera = null;


        if (!_reflectionTexture || _oldReflectionTextureSize != _textureSize)
        {
            if (_reflectionTexture)
                DestroyImmediate(_reflectionTexture);
            _reflectionTexture = new RenderTexture(_textureSize, _textureSize, 16);
            _reflectionTexture.name = "__MirrorReflection" + GetInstanceID();
            _reflectionTexture.isPowerOfTwo = true;
            _reflectionTexture.hideFlags = HideFlags.DontSave;
            _oldReflectionTextureSize = _textureSize;
        }


        reflectionCamera = _reflectionCameras[currentCamera] as Camera;
        if (!reflectionCamera)
        {
            GameObject go = new GameObject("Mirror Refl Camera id" + GetInstanceID() + " for " + currentCamera.GetInstanceID(), typeof(Camera), typeof(Skybox));
            reflectionCamera = go.GetComponent<Camera>();
            reflectionCamera.enabled = false;
            reflectionCamera.transform.position = transform.position;
            reflectionCamera.transform.rotation = transform.rotation;
            reflectionCamera.gameObject.AddComponent<FlareLayer>();
            go.hideFlags = HideFlags.HideAndDontSave;
            _reflectionCameras[currentCamera] = reflectionCamera;
        }
    }

    private static float sgn(float a)
    {
        if (a > 0.0f) return 1.0f;
        if (a < 0.0f) return -1.0f;
        return 0.0f;
    }

    private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
    {
        Vector3 offsetPos = pos + normal * _clipPlaneOffset;
        Matrix4x4 m = cam.worldToCameraMatrix;
        Vector3 cpos = m.MultiplyPoint(offsetPos);
        Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
        return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
    }

    private static void CalculateObliqueMatrix(ref Matrix4x4 projection, Vector4 clipPlane)
    {
        Vector4 q = projection.inverse * new Vector4(
            sgn(clipPlane.x),
            sgn(clipPlane.y),
            1.0f,
            1.0f
        );
        Vector4 c = clipPlane * (2.0F / (Vector4.Dot(clipPlane, q)));

        projection[2] = c.x - projection[3];
        projection[6] = c.y - projection[7];
        projection[10] = c.z - projection[11];
        projection[14] = c.w - projection[15];
    }

    private static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
    {
        reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
        reflectionMat.m01 = (-2F * plane[0] * plane[1]);
        reflectionMat.m02 = (-2F * plane[0] * plane[2]);
        reflectionMat.m03 = (-2F * plane[3] * plane[0]);

        reflectionMat.m10 = (-2F * plane[1] * plane[0]);
        reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
        reflectionMat.m12 = (-2F * plane[1] * plane[2]);
        reflectionMat.m13 = (-2F * plane[3] * plane[1]);

        reflectionMat.m20 = (-2F * plane[2] * plane[0]);
        reflectionMat.m21 = (-2F * plane[2] * plane[1]);
        reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
        reflectionMat.m23 = (-2F * plane[3] * plane[2]);

        reflectionMat.m30 = 0F;
        reflectionMat.m31 = 0F;
        reflectionMat.m32 = 0F;
        reflectionMat.m33 = 1F;
    }
}
