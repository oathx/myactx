using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// add ALL post effect used in game
// this script will control the OFF&ON use rendr level info.
// now add Bloom, Distort, Blur.
[ExecuteInEditMode]
public class XPostEffectManager : MonoBehaviour
{
    //  property
    private static XPostEffectManager _instance;
    public static XPostEffectManager Instance
    {
        get
        {
            if (!_instance)
            {
                GameObject go = new GameObject("PostEffectManager");
                _instance = go.AddComponent<XPostEffectManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    public bool IsSupportPostEffect
    {
        get
        {
            return SystemInfo.supportsImageEffects;
        }
    }

    private XRenderLevelInfoObject.XPOSTFX_QUALITY _postQuality;
    private List<XRenderLevelInfoObject.XPOSTFX> _postEffects;

    RenderTexture _bloomOut;
    private RenderTexture _colorBufferTexture;
    private RenderTexture _depthBufferTexture;

    private Camera _currentCamera;

    private bool _isReleased = false;

    private TextureSizeStruct DEFAULT_TEXTURE_SIZE = new TextureSizeStruct(1024, 1024);
    private struct TextureSizeStruct
    {
        public int x;
        public int y;
        public TextureSizeStruct(int pX, int pY)
        {
            x = pX;
            y = pY;
        }
    }

    private bool _isSupportHDR
    {
        get
        {
            return SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf);
        }
    }
    //Bloom Shader names
    private string _blendForBloomName = "PostEffect/ImageEffect/DownSampleAndBlurOnce";  // Hidden/BlendForBloom

    private Shader _blendBloomShadr;
    private Material _blendBloomMat;

    private string _postEffectShaderName = "";
    private Material _postEffectsMat;
    private Shader _postEffectShader;

    public float BloomIntensity = 0.5f;
    public float BloomThreshold = 0.5f;
    public Color BloomThresholdColor = Color.white;

    //private int _parameterID;
    private int _threshholdID;
    private int _screenStepID;
    private int _bloomIntensityID;
    private int _bloomID;
    private int _bumpTexID;
    private int _distortIntensityID;
    private int _bumpAmtID;
    private int _sampleDistID;
    private int _sampleStrength;
    private int _scrPosID;

    private Camera _distortCam;
    private int[] _distortWith = new int[] { 512, 1024 };
    private int[] _distortHeight = new int[] { 512, 1024 };
    private RenderTexture _distortTex;
    [Range(0.0f, 120.0f)]
    public float _distortBumpAmt = 40f;
    private Vector2 _distortIntensity = new Vector2(1.0f, 1.0f);

    [Range(0.0f, 1.0f)]
    public float SampleDist = 0.17f;
    [Range(0.0f, 10.0f)]
    public float SampleStrength = 2.09f;
    public Vector3 wp = new Vector3(5.374f, 3.7f, 0);  //人物触碰点

    private float _startSampleStrength;
    private float _destSampleStrength;

    private int _fixedFrameCount;
    private int _blurFrame;
    private bool _isRadialEnable = false;

    private RenderTexture _depthTex;
    Camera _depthTextureCam;
    private Shader _shaderSetDepthMap;
    private int _depthId;

    private Shader _motionBlurShader;
    private Material _motionBlurShaderMat;
    private RenderTexture _motionBlurTex;
    private RenderTexture _accumTexture;
    private Camera _motionBlurCam;

    private float _accumIntensity = 0.8f;
    private bool _isStartMotionBlur = true;
    private bool _initializeAccumTexture = false; // insure the accumTexture is newly when starting motionblur 
    private int _layerMask = 20;
    private int _accumOrigID;
    private int _accumTextureID;
    private int _motionBluredTexID;

    private Shader _depthFieldShader;
    private string _depthFieldShaderString = "PostEffect/ImageEffect/DepthOfFiled";
    private Material _depthFieldMat;
    private Shader _bloomShader;
    private Material _bloomMat;
    private RenderTexture _bloomTexture;
    [Range(0, 1)]
    private float _focalPlane;
    private RenderTexture _depthField;
    [Range(0, 6f)]
    private float _aperture = 2.0f;
    [Range(0.1f, 5.0f)]
    private float _gradualChange = 1.0f; // the range of sharpness

    private int _gradualChangeID;
    private int _apertureID;
    private int _focalPlaneID;
    private int _depthFieldBlurTexID;
    private bool _isStartDepthField = false;

    //  INIT
#if UNITY_EDITOR
    void Awake()
    {

    }
    Camera _camerEditor;
    void OnEnable()
    {
        if (Application.isPlaying)
            return;
        _instance = this;
        _camerEditor = this.gameObject.GetComponent<Camera>();
        if (_camerEditor)
        {
            _instance.Initial(XRenderLevelInfoObject.XPOSTFX_QUALITY.High, new XRenderLevelInfoObject.XPOSTFX[] { XRenderLevelInfoObject.XPOSTFX.Bloom, XRenderLevelInfoObject.XPOSTFX.Warp, XRenderLevelInfoObject.XPOSTFX.RadialBlur, XRenderLevelInfoObject.XPOSTFX.Depth });
        }
        else
        {
            Debug.LogError(" PostEffectManager NOT on a correct CAMERA!");
        }
    }

    void OnDisable()
    {
        if (Application.isPlaying)
            return;
        _instance = null;
        if (_bloomOut)
        {
            _bloomOut.Release();
            _bloomOut = null;
        }
        if (_distortTex)
        {
            _distortCam.targetTexture = null;
            _distortTex.Release();
            _distortTex = null;
        }
        if (_depthTex)
        {
            _depthTex.Release();
            _depthTex = null;
        }

        if (_blendBloomMat)
            Destroy(_blendBloomMat);

        if (_distortCam)
            Destroy(_distortCam);

        if (_postEffects != null)
            _postEffects.Clear();

        if (_postEffectsMat)
            Destroy(_postEffectsMat);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (Application.isPlaying)
            return;
        if (_camerEditor)
        {
            this.PostRender(_camerEditor, src, dst);
        }
    }

#endif

    public void ReleaseAll()
    {
        if (_bloomOut)
        {
            //_bloomOut.Release ();
            //_bloomOut = null;
            Destroy(_bloomOut);
        }
        if (_distortTex)
        {
            if (_distortCam != null)
                _distortCam.targetTexture = null;
            _distortTex.Release();
            _distortTex = null;
        }
        if (_depthTex)
        {
            if (_depthTextureCam != null)
                _depthTextureCam.targetTexture = null;
            Destroy(_depthTex);
        }

        if (_blendBloomMat)
            Destroy(_blendBloomMat);

        if (_postEffects != null)
            _postEffects.Clear();

        if (_motionBlurTex) //motionBlur
        {
            if (_motionBlurCam != null)
                _motionBlurCam.targetTexture = null;
            Destroy(_motionBlurTex);
        }
        if (_accumTexture)
        {
            Destroy(_accumTexture);
        }
        if (_motionBlurShaderMat)
        {
            Destroy(_motionBlurShaderMat);
        }
        if (_depthFieldMat) //depthfield
            Destroy(_depthFieldMat);
        if (_bloomTexture)
            Destroy(_bloomTexture);

        //ReleaseBuffers();

        _isReleased = true;
    }

    private void ReleaseBuffers()
    {
        if (_colorBufferTexture != null)
        {
            _colorBufferTexture.Release();
            _colorBufferTexture = null;
        }

        if (_depthBufferTexture != null)
        {
            _depthBufferTexture.Release();
            _depthBufferTexture = null;
        }
    }

    public void Initial(XRenderLevelInfoObject.XPOSTFX_QUALITY qulity, XRenderLevelInfoObject.XPOSTFX[] effects)
    {
        if (!IsSupportPostEffect)
            return;

        if (qulity == XRenderLevelInfoObject.XPOSTFX_QUALITY.Off || effects == null || effects.Length < 1)
        {
            ReleaseBuffers();
            return;
        }

#if !UNITY_EDITOR && !UNITY_STANDALONE
        if (_colorBufferTexture == null || _depthBufferTexture == null)
        {
            ReleaseBuffers();
            
            _colorBufferTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
            _depthBufferTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Depth);

            if (!_colorBufferTexture.Create() || !_depthBufferTexture.Create())
            {
                Debug.LogError("Color Buffer create ERROR!!");
                return;
            }
            _colorBufferTexture.name = "ColorBufferTexture";
            _depthBufferTexture.name = "DepthBufferTexture";
        }
#endif

        _postQuality = qulity;
        _postEffects = new List<XRenderLevelInfoObject.XPOSTFX>();
        for (int i = 0; i < effects.Length; i++)
        {
            _postEffects.Add(effects[i]);
        }
        _postEffectShaderName = "PostEffect/ImageEffect/";

        if (IsEffectActive(XRenderLevelInfoObject.XPOSTFX.Bloom))
        {
            _postEffectShaderName += "bloom";
        }
        if (IsEffectActive(XRenderLevelInfoObject.XPOSTFX.Warp))
        {
            _postEffectShaderName += "distort";
        }
        if (IsEffectActive(XRenderLevelInfoObject.XPOSTFX.RadialBlur))
        {
            _postEffectShaderName += "blur";
        }

        if (IsEffectActive(XRenderLevelInfoObject.XPOSTFX.Depth))
        {
            //InitDepthRender ();
            //InitDepthField ();
        }

        if (IsEffectActive(XRenderLevelInfoObject.XPOSTFX.MotionBlur))
        {
            InitMotionBlur();
        }

        InitShadersAndMaterials();
        InitOutTexs();
        InitPostEffectPropert();
        _isReleased = false;
    }

    // init out textures depend on the effects' state.
    private void InitOutTexs()
    {
        if (IsEffectActive(XRenderLevelInfoObject.XPOSTFX.Bloom))
        {
            int sizeX = _postQuality == XRenderLevelInfoObject.XPOSTFX_QUALITY.Low ? DEFAULT_TEXTURE_SIZE.x / 4 : DEFAULT_TEXTURE_SIZE.x;
            int sizeY = _postQuality == XRenderLevelInfoObject.XPOSTFX_QUALITY.Low ? DEFAULT_TEXTURE_SIZE.y / 4 : DEFAULT_TEXTURE_SIZE.y;
            _bloomOut = new RenderTexture(sizeX, sizeY, 0);
            _bloomOut.hideFlags = HideFlags.HideAndDontSave;
            _bloomOut.name = "BloomTexture";
            _bloomOut.hideFlags = HideFlags.HideAndDontSave;
            _bloomOut.Create();
        }

    }

    private void InitShadersAndMaterials()
    {
        if (_postEffects.Count > 0)
        {
            _postEffectShader = Shader.Find(_postEffectShaderName);
            if (_postEffectShader == null)
                Debug.LogError(_postEffectShaderName);
            _postEffectsMat = new Material(_postEffectShader);
            _postEffectsMat.hideFlags = HideFlags.HideAndDontSave;
        }
        //bloom
        _blendBloomShadr = Shader.Find(_blendForBloomName);
        if (_blendBloomShadr == null)
            Debug.LogError(_blendForBloomName);
        _blendBloomMat = new Material(_blendBloomShadr);
        _blendBloomMat.hideFlags = HideFlags.HideAndDontSave;

        //_parameterID = Shader.PropertyToID("_Parameter");
        _threshholdID = Shader.PropertyToID("_BloomThreshhold");
        _screenStepID = Shader.PropertyToID("_ScreenStep");

        _bloomIntensityID = Shader.PropertyToID("_BloomIntensity");
        _bloomID = Shader.PropertyToID("_Bloom");

        _bumpTexID = Shader.PropertyToID("_BumpTex");
        _distortIntensityID = Shader.PropertyToID("_DistortIntensity");
        _bumpAmtID = Shader.PropertyToID("_BumpAmt");

        _sampleDistID = Shader.PropertyToID("_SampleDist");
        _sampleStrength = Shader.PropertyToID("_SampleStrength");
        _scrPosID = Shader.PropertyToID("_ScrPos");
    }


    private void InitPostEffectPropert()
    {
        if (IsEffectActive(XRenderLevelInfoObject.XPOSTFX.Warp))
        {
            if (_distortCam == null)
            {
                GameObject distortGO = new GameObject("DistortCamera");
                _distortCam = distortGO.AddComponent<Camera>();
            }
            _distortTex = new RenderTexture(_distortWith[(int)_postQuality], _distortHeight[(int)_postQuality], 0);
            _distortTex.name = "DistortTexture";
            _distortTex.Create();

            _distortCam.targetTexture = _distortTex;
            _distortCam.renderingPath = RenderingPath.Forward;
            _distortCam.clearFlags = CameraClearFlags.SolidColor;
            _distortCam.cullingMask = 1 << LayerMask.NameToLayer("Distort");
            _distortCam.backgroundColor = Color.black;
        }
        if (IsEffectActive(XRenderLevelInfoObject.XPOSTFX.RadialBlur))
        {
            _fixedFrameCount = 0;
            _blurFrame = 0;
            _destSampleStrength = 0;
            SampleDist = 0;
        }
    }

    private void InitDepthRender()
    {
        _shaderSetDepthMap = Shader.Find("PostEffect/Effect/CreatDepthTexture");
        _depthId = Shader.PropertyToID("_DepthTextureCreated");

        GameObject o = new GameObject("depthCamera");
        _depthTextureCam = o.AddComponent<Camera>();

        int layerMask = LayerMask.NameToLayer("Effect");
        int layerMaskDist = LayerMask.NameToLayer("Distort");
        _depthTextureCam.cullingMask = ~((1 << layerMask) | (1 << layerMaskDist));
        _depthTextureCam.backgroundColor = Color.white;
        _depthTextureCam.gameObject.SetActive(false);

        int _renderLeviInfo = 1;
        if (_postQuality == XRenderLevelInfoObject.XPOSTFX_QUALITY.Low)
            _renderLeviInfo = 2;
        _depthTex = new RenderTexture(DEFAULT_TEXTURE_SIZE.x / _renderLeviInfo, DEFAULT_TEXTURE_SIZE.y / _renderLeviInfo, 24, RenderTextureFormat.Default);
        _depthTex.hideFlags = HideFlags.HideAndDontSave;
        _depthTex.name = "RenderedDepthTex";
        _depthTextureCam.targetTexture = _depthTex;
        _depthTextureCam.SetReplacementShader(_shaderSetDepthMap, "Queue");
        _depthTex.Create();
    }
    private void InitMotionBlur()  //Motion Blur Init
    {
        if (_motionBlurCam == null)
        {
            GameObject motionBlurGo = new GameObject("MotionBlurCam");
            _motionBlurCam = motionBlurGo.AddComponent<Camera>();
            _motionBlurCam.cullingMask = 1 << _layerMask;
            _motionBlurCam.backgroundColor = new Color(0, 0, 0, 0);
            _motionBlurCam.clearFlags = CameraClearFlags.SolidColor;
            _motionBlurCam.gameObject.SetActive(false);

            int _renderLeviInfo = 1;
            if (_postQuality == XRenderLevelInfoObject.XPOSTFX_QUALITY.Low)
                _renderLeviInfo = 2;
            _motionBlurTex = new RenderTexture(DEFAULT_TEXTURE_SIZE.x / _renderLeviInfo, DEFAULT_TEXTURE_SIZE.y / _renderLeviInfo, 8, RenderTextureFormat.ARGB32);
            _motionBlurTex.hideFlags = HideFlags.HideAndDontSave;
            _motionBlurTex.name = "MotionBlurTex";
            _motionBlurTex.Create();
            _accumTexture = new RenderTexture(DEFAULT_TEXTURE_SIZE.x / _renderLeviInfo, DEFAULT_TEXTURE_SIZE.y / _renderLeviInfo, 8, RenderTextureFormat.ARGB32);
            _accumTexture.hideFlags = HideFlags.HideAndDontSave;
            _accumTexture.name = "AccumTex";
            _accumTexture.Create();
            _motionBlurCam.targetTexture = _motionBlurTex;
        }
        _motionBlurShader = Shader.Find("PostEffect/ImageEffect/MotionBlur");
        _motionBlurShaderMat = new Material(_motionBlurShader);
        _motionBlurShaderMat.hideFlags = HideFlags.HideAndDontSave;
        _accumOrigID = Shader.PropertyToID("_AccumOrig");
        _accumTextureID = Shader.PropertyToID("accumTexture");
        _motionBluredTexID = Shader.PropertyToID("_MotionBluredTex");
    }

    private void InitDepthField()
    {
        _depthFieldShader = Shader.Find(_depthFieldShaderString);
        _depthFieldMat = new Material(_depthFieldShader);
        _depthFieldBlurTexID = Shader.PropertyToID("_BlurTex");
        _focalPlaneID = Shader.PropertyToID("_Dist");
        _apertureID = Shader.PropertyToID("_Parameter");
        _gradualChangeID = Shader.PropertyToID("_GragualIntensity");
        _bloomShader = Shader.Find(_blendForBloomName);
        _bloomMat = new Material(_bloomShader);
        _bloomTexture = new RenderTexture(DEFAULT_TEXTURE_SIZE.x, DEFAULT_TEXTURE_SIZE.y, 0, RenderTextureFormat.ARGB32);
        _bloomTexture.hideFlags = HideFlags.HideAndDontSave;
        _bloomTexture.name = "BloomTexForDepthField";
    }
    //RENDER
    public void PostRender(Camera cam, RenderTexture src = null, RenderTexture dst = null)
    {
        if (!IsSupportPostEffect || _isReleased)
            return;
        if (_postEffects.Count > 0)
        {
            if (src != null)
            {
                _colorBufferTexture = src;
            }
            else
            {
                if (cam != _currentCamera)
                {
                    _currentCamera = cam;
                    if (_colorBufferTexture != null)
                        _currentCamera.SetTargetBuffers(_colorBufferTexture.colorBuffer, _depthBufferTexture.depthBuffer);
                }
            }

            if (_colorBufferTexture == null)
            {
                //Debug.LogError("_colorBufferTexture is NULL!");
                return;
            }
            if (IsEffectActive(XRenderLevelInfoObject.XPOSTFX.MotionBlur))
            {
                if (_isStartMotionBlur)
                {
                    RenderMotionBlur(cam);
                }
                else
                {
                    _initializeAccumTexture = true;
                }
            }
        }

        if (_postEffects.Count > 0)
        {
            PostEffectsRender(cam, _colorBufferTexture, dst);
        }
    }

    private void BloomTex(RenderTexture src, RenderTexture dst)
    {
        int divider = _postQuality == XRenderLevelInfoObject.XPOSTFX_QUALITY.High ? 1 : 2;
        _blendBloomMat.SetColor(_threshholdID, BloomThreshold * BloomThresholdColor);
        src.filterMode = FilterMode.Bilinear;
        var rtW = DEFAULT_TEXTURE_SIZE.x / divider;
        var rtH = DEFAULT_TEXTURE_SIZE.y / divider;

        _blendBloomMat.SetVector(_screenStepID, new Vector4(0f, 1.0f));
        RenderTexture rt = RenderTexture.GetTemporary(rtW, rtH, 0, RenderTextureFormat.Default);
        rt.filterMode = FilterMode.Bilinear;
        Graphics.Blit(src, rt, _blendBloomMat, _postQuality == XRenderLevelInfoObject.XPOSTFX_QUALITY.High ? 1 : 0);
        //dst.DiscardContents();

        _blendBloomMat.SetVector(_screenStepID, new Vector4(1.0f, 0f));
        Graphics.Blit(rt, dst, _blendBloomMat, _postQuality == XRenderLevelInfoObject.XPOSTFX_QUALITY.High ? 1 : 0);
        RenderTexture.ReleaseTemporary(rt);
    }

    private void PostEffectsRender(Camera Cam, RenderTexture src, RenderTexture dst)
    {
        if (IsEffectActive(XRenderLevelInfoObject.XPOSTFX.Bloom))
        {
            if (BloomThreshold > 0.99f && BloomThresholdColor == Color.white)
            {
                _postEffectsMat.SetTexture(_bloomID, Texture2D.blackTexture);
            }
            else
            {
                _postEffectsMat.SetFloat(_bloomIntensityID, BloomIntensity);
                BloomTex(src, _bloomOut);
                _postEffectsMat.SetTexture(_bloomID, _bloomOut);
            }
        }
        if (IsEffectActive(XRenderLevelInfoObject.XPOSTFX.Warp))
        {
            _distortCam.transform.position = Cam.transform.position;
            _distortCam.transform.rotation = Cam.transform.rotation;
            _distortCam.aspect = Cam.aspect;
            _distortCam.fieldOfView = Cam.fieldOfView;
            _distortCam.farClipPlane = Cam.farClipPlane;
            _distortCam.nearClipPlane = Cam.nearClipPlane;
            _distortCam.orthographic = Cam.orthographic;
            _distortCam.orthographicSize = Cam.orthographicSize;
            if (XHeatDistortEffect.ret < 1) //没有开启distort，
                _distortBumpAmt = 0.0f;
            _postEffectsMat.SetTexture(_bumpTexID, _distortCam.targetTexture);
            _postEffectsMat.SetVector(_distortIntensityID, _distortIntensity);
            _postEffectsMat.SetFloat(_bumpAmtID, _distortBumpAmt);
        }
        if (IsEffectActive(XRenderLevelInfoObject.XPOSTFX.RadialBlur))
        {
            Vector2 ScrPos = Cam.WorldToViewportPoint(wp);//转换到viewpoit坐标
            _postEffectsMat.SetFloat(_sampleDistID, SampleDist);
            _postEffectsMat.SetFloat(_sampleStrength, SampleStrength);
            _postEffectsMat.SetVector(_scrPosID, ScrPos);
        }

        //motionblur
        if (IsEffectActive(XRenderLevelInfoObject.XPOSTFX.MotionBlur) && _isStartMotionBlur)
        {
            RenderTexture rezColor = RenderTexture.GetTemporary(DEFAULT_TEXTURE_SIZE.x, DEFAULT_TEXTURE_SIZE.y, 0, RenderTextureFormat.ARGBHalf);
            rezColor.filterMode = FilterMode.Bilinear;
            Graphics.Blit(src, rezColor, _postEffectsMat);
            Graphics.Blit(rezColor, dst, _motionBlurShaderMat, 1);
            RenderTexture.ReleaseTemporary(rezColor);
        }
        else if (IsEffectActive(XRenderLevelInfoObject.XPOSTFX.Depth) && _isStartDepthField)
        {
            RenderTexture tempTex = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.ARGBHalf);
            tempTex.hideFlags = HideFlags.DontSave;
            BlurTexture(src, _bloomTexture, _aperture * 1.5f);
            BlurTexture(_bloomTexture, _bloomTexture, _aperture);
            _depthFieldMat.SetTexture(_depthFieldBlurTexID, _bloomTexture);
            _depthFieldMat.SetFloat(_focalPlaneID, _focalPlane);
            _depthFieldMat.SetFloat(_gradualChangeID, _gradualChange);
            tempTex.DiscardContents();
            Graphics.Blit(src, tempTex, _depthFieldMat);
            Graphics.Blit(tempTex, dst, _postEffectsMat);
            RenderTexture.ReleaseTemporary(tempTex);//
        }
        else
        {
            Graphics.Blit(src, dst, _postEffectsMat);
        }
    }

    private void AddTo(float intensity_, RenderTexture from, RenderTexture to)
    {
        _blendBloomMat.SetFloat("_Intensity", intensity_);
        to.MarkRestoreExpected(); // additive blending, RT restore expected
        Graphics.Blit(from, to, _blendBloomMat, 9);
    }

    //RadialBlur
    public void StartBlur(Vector3 pos, float dist, float strength, int time)
    {
        _isRadialEnable = true;
        SampleDist = dist;
        SampleStrength = .1f;
        wp = pos;

        _startSampleStrength = SampleStrength;
        _destSampleStrength = strength;
        _blurFrame = time;
        _fixedFrameCount = 0;
    }

    private bool IsEffectActive(XRenderLevelInfoObject.XPOSTFX effect)
    {
        for (int i = 0; i < _postEffects.Count; i++)
        {
            if (_postEffects[i] == effect)
                return true;
        }
        return false;
    }
    //adjust post effect param
    public void SetBloomIntensity(float intensity)
    {
        BloomIntensity = intensity;
    }
    public void SetBloomThreshold(float threshold)
    {
        BloomThreshold = threshold;
    }
    public void SetBloomThresholdColor(Color color)
    {
        BloomThresholdColor = color;
    }
    public void SetDistortIntensity(Vector2 intensity)
    {
        _distortIntensity = intensity;
    }
    public void SetDistortBumpAmt(float bumpAmt)
    {
        _distortBumpAmt = bumpAmt;
    }
    //MotionBlur Setting
    public void SetMotionBlurActive(bool isActive)
    {
        _isStartMotionBlur = isActive;
    }
    public void SetAccumOrig(float accumOrig)
    {
        _accumIntensity = accumOrig;
    }
    public void SetLayerMask(int layer)
    {
        _layerMask = layer;
    }
    //depth of field setting
    public void SetDepthFieldActive(bool active)
    {
        _isStartDepthField = active;
    }
    public void SetAperture(float aper)
    {
        _aperture = aper;
    }
    public void SetFocalPlane(float dist)
    {
        _focalPlane = dist;
    }
    public void SetGradualChange(float change)
    {
        _gradualChange = change;
    }

    void FixedUpdate()
    {
        if (_blurFrame > 0 && _isRadialEnable)
        {
            if (_fixedFrameCount <= _blurFrame)
            {
                SampleStrength += (_destSampleStrength - _startSampleStrength) / _blurFrame;
                if (SampleStrength <= 0)
                {
                    _isRadialEnable = false;
                }
            }
            else
            {
                _startSampleStrength = SampleStrength;
                _destSampleStrength = .0f;
                _fixedFrameCount = 0;
            }
            _fixedFrameCount++;
        }
    }
    public void PreCullRender(Camera curCam)
    {
        if (_postEffects.Count > 0)
        {
            if (IsEffectActive(XRenderLevelInfoObject.XPOSTFX.Depth) && _isStartDepthField)
            {
                //DeleteShadow ();
                //RenderDepthMap (curCam);
            }
        }
    }

    private void DeleteShadow()
    {
        GameObject enemyShadow = GameObject.Find("UltimateBattleCamEnemy/shadow");//
        if (enemyShadow)
        {
            bool activeShadow = enemyShadow.activeInHierarchy;
            if (activeShadow)
                enemyShadow.SetActive(false);
        }

        GameObject playerShadow = GameObject.Find("UltimateBattleCamPlayer/shadow");
        if (playerShadow)
        {
            bool activeShadow = playerShadow.activeInHierarchy;
            if (activeShadow)
                playerShadow.SetActive(false);
        }
    }

    private void RenderDepthMap(Camera mainCam)
    {
        _depthTextureCam.depth = mainCam.depth - 2;
        _depthTextureCam.transform.position = mainCam.transform.position;
        _depthTextureCam.transform.rotation = mainCam.transform.rotation;
        _depthTextureCam.fieldOfView = mainCam.fieldOfView;
        _depthTextureCam.orthographic = mainCam.orthographic;
        _depthTextureCam.orthographicSize = mainCam.orthographicSize;
        _depthTextureCam.clearFlags = CameraClearFlags.SolidColor;
        _depthTextureCam.aspect = mainCam.aspect;
        _depthTextureCam.farClipPlane = 20;
        _depthTextureCam.nearClipPlane = mainCam.nearClipPlane;
        _depthTextureCam.RenderWithShader(_shaderSetDepthMap, "Queue");
        BlurTexture(_depthTextureCam.targetTexture, _depthTextureCam.targetTexture, _aperture * 0.5f);
        Shader.SetGlobalTexture(_depthId, _depthTextureCam.targetTexture);
    }
    private void RenderMotionBlur(Camera mainCam)
    {
        _motionBlurCam.depth = mainCam.depth - 2;
        _motionBlurCam.transform.position = mainCam.transform.position;
        _motionBlurCam.transform.rotation = mainCam.transform.rotation;
        _motionBlurCam.fieldOfView = mainCam.fieldOfView;
        _motionBlurCam.orthographic = mainCam.orthographic;
        _motionBlurCam.orthographicSize = mainCam.orthographicSize;
        _motionBlurCam.aspect = mainCam.aspect;
        _motionBlurCam.farClipPlane = mainCam.farClipPlane;
        _motionBlurCam.nearClipPlane = mainCam.nearClipPlane;

        _motionBlurCam.Render();
        if (_initializeAccumTexture) // when open motionblur ,the first frame
        {
            Graphics.Blit(_motionBlurTex, _accumTexture);
            _initializeAccumTexture = false;
        }
        int _renderLeviInfo = 1;
        if (_postQuality == XRenderLevelInfoObject.XPOSTFX_QUALITY.Low)
            _renderLeviInfo = 2;
        RenderTexture blurbuffer = RenderTexture.GetTemporary(DEFAULT_TEXTURE_SIZE.x / _renderLeviInfo, DEFAULT_TEXTURE_SIZE.y / _renderLeviInfo, 8, RenderTextureFormat.ARGB32);
        Graphics.Blit(_accumTexture, blurbuffer);
        Graphics.Blit(blurbuffer, _accumTexture); // to insure _accumTexture is correct,(Unity bug,_accumTexture is all Color(1,1,1,1)) 
        RenderTexture.ReleaseTemporary(blurbuffer);
        _motionBlurShaderMat.SetFloat(_accumOrigID, _accumIntensity);
        _motionBlurShaderMat.SetTexture(_accumTextureID, _accumTexture);
        Graphics.Blit(_motionBlurTex, _accumTexture, _motionBlurShaderMat, 0);
        //Graphics.Blit (_motionBlurTex,_preFrameTex);
        Shader.SetGlobalTexture(_motionBluredTexID, _accumTexture);
    }
    private void BlurTexture(RenderTexture src, RenderTexture dst, float aperture)
    {
        _bloomMat.SetVector(_apertureID, new Vector4(aperture, 0, 0, 0));
        RenderTexture tempTex = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.ARGBHalf);
        tempTex.hideFlags = HideFlags.DontSave;
        //vertival
        Graphics.Blit(src, tempTex, _bloomMat, 1);
        //Horizontal
        Graphics.Blit(tempTex, dst, _bloomMat, 2);
        RenderTexture.ReleaseTemporary(tempTex);
    }
}
