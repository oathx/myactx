using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SLua;

/// <summary>
/// 
/// </summary>
public class XCameraShake : MonoBehaviour
{
    public List<Camera> cameras = new List<Camera>();
    public int          numberOfShakes = 2;
    public Vector3      shakeAmount = Vector3.one;
    public Vector3      rotationAmount = Vector3.one;
    public float        distance = 0.1f;
    public float        speed = 50f;
    public float        decay = 0.2f;
    public float        guiShakeModifier = 1f;
    public bool         multiplyByTimeScale = true;
    private Rect        shakeRect = new Rect();
    private bool        shaking = false;
    private bool        cancelling = false;

    /// <summary>
    /// 
    /// </summary>
    internal class ShakeState
    {
        internal readonly Vector3       startPosition;
        internal readonly Quaternion    startRotation;
        internal readonly Vector2       guiStartPosition;
        internal Vector3                shakePosition;
        internal Quaternion             shakeRotation;
        internal Vector2                guiShakePosition;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="guiPosition"></param>
        internal ShakeState(Vector3 position, Quaternion rotation, Vector2 guiPosition)
        {
            startPosition = position;
            startRotation = rotation;
            guiStartPosition = guiPosition;
            shakePosition = position;
            shakeRotation = rotation;
            guiShakePosition = guiPosition;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private Dictionary<Camera, List<ShakeState>> 
        states = new Dictionary<Camera, List<ShakeState>>();

    /// <summary>
    /// 
    /// </summary>
    private Dictionary<Camera, int> shakeCount = new Dictionary<Camera, int>();

    //Minimum shake values
    private const bool  checkForMinimumValues = true;
    private const float minShakeValue = 0.001f;
    private const float minRotationValue = 0.001f;

    //The Camera Shake singleton instance
    /// <summary>
    /// 
    /// </summary>
    private static XCameraShake instance;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static XCameraShake  GetSingleton()
    {
        return instance;
    }

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        if (cameras.Count < 1)
        {
            if (GetComponent<Camera>())
                cameras.Add(GetComponent<Camera>());
        }

        if (cameras.Count < 1)
        {
            if (Camera.main)
                cameras.Add(Camera.main);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static bool  isShaking
    {
        get
        {
            return instance.IsShaking();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static bool  isCancelling
    {
        get
        {
            return instance.IsCancelling();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static void  Shake()
    {
        instance.DoShake();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="numberOfShakes"></param>
    /// <param name="shakeAmount"></param>
    /// <param name="rotationAmount"></param>
    /// <param name="distance"></param>
    /// <param name="speed"></param>
    /// <param name="decay"></param>
    /// <param name="guiShakeModifier"></param>
    /// <param name="multiplyByTimeScale"></param>
    public static void Shake(int numberOfShakes, Vector3 shakeAmount, Vector3 rotationAmount, 
        float distance, float speed, float decay, float guiShakeModifier, bool multiplyByTimeScale)
    {
        instance.DoShake(numberOfShakes, shakeAmount, rotationAmount, distance, speed, decay, guiShakeModifier, multiplyByTimeScale);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="callback"></param>
    public static void Shake(System.Action callback)
    {
        instance.DoShake(callback);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="numberOfShakes"></param>
    /// <param name="shakeAmount"></param>
    /// <param name="rotationAmount"></param>
    /// <param name="distance"></param>
    /// <param name="speed"></param>
    /// <param name="decay"></param>
    /// <param name="guiShakeModifier"></param>
    /// <param name="multiplyByTimeScale"></param>
    /// <param name="callback"></param>
    public static void Shake(int numberOfShakes, Vector3 shakeAmount, Vector3 rotationAmount, 
        float distance, float speed, float decay, float guiShakeModifier, bool multiplyByTimeScale, System.Action callback)
    {
        instance.DoShake(numberOfShakes, shakeAmount, rotationAmount, distance, speed, decay, guiShakeModifier, multiplyByTimeScale, callback);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="numberOfShakes"></param>
    /// <param name="shakeAmount"></param>
    /// <param name="rotationAmount"></param>
    /// <param name="distance"></param>
    /// <param name="speed"></param>
    /// <param name="decay"></param>
    /// <param name="guiShakeModifier"></param>
    /// <param name="multiplyByTimeScale"></param>
    /// <param name="luaCallBack"></param>
    /// <param name="luaModule"></param>
    public static void Shake(int numberOfShakes, Vector3 shakeAmount, Vector3 rotationAmount, 
        float distance, float speed, float decay, float guiShakeModifier, bool multiplyByTimeScale, LuaFunction luaCallBack, LuaTable luaModule)
    {
        instance.DoShake(numberOfShakes, shakeAmount, rotationAmount, distance, speed, decay, guiShakeModifier, multiplyByTimeScale, luaCallBack, luaModule);
    }

    /// <summary>
    /// 
    /// </summary>
    public static void  CancelShake()
    {
        instance.DoCancelShake();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="time"></param>
    public static void  CancelShake(float time)
    {
        instance.DoCancelShake(time);
    }

    /// <summary>
    /// 
    /// </summary>
    public static void  BeginShakeGUI()
    {
        instance.DoBeginShakeGUI();
    }

    /// <summary>
    /// 
    /// </summary>
    public static void  EndShakeGUI()
    {
        instance.DoEndShakeGUI();
    }

    /// <summary>
    /// 
    /// </summary>
    public static void  BeginShakeGUILayout()
    {
        instance.DoBeginShakeGUILayout();
    }

    /// <summary>
    /// 
    /// </summary>
    public static void  EndShakeGUILayout()
    {
        instance.DoEndShakeGUILayout();
    }

    //Occurs when a camera starts shaking
    public event System.Action cameraShakeStarted;
    //Occurs when a camera has completely stopped shaking and has been reset to its original position
    public event System.Action allCameraShakesCompleted;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool IsShaking()
    {
        return shaking;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool IsCancelling()
    {
        return cancelling;
    }

    /// <summary>
    /// 
    /// </summary>
    public void DoShake()
    {
        Vector3 seed = Random.insideUnitSphere;

        foreach (Camera cam in cameras)
        {
            StartCoroutine(DoShake_Internal(cam, seed, this.numberOfShakes, this.shakeAmount, this.rotationAmount, 
                this.distance, this.speed, this.decay, this.guiShakeModifier, this.multiplyByTimeScale, null, null, null));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="numberOfShakes"></param>
    /// <param name="shakeAmount"></param>
    /// <param name="rotationAmount"></param>
    /// <param name="distance"></param>
    /// <param name="speed"></param>
    /// <param name="decay"></param>
    /// <param name="guiShakeModifier"></param>
    /// <param name="multiplyByTimeScale"></param>
    public void DoShake(int numberOfShakes, Vector3 shakeAmount, Vector3 rotationAmount,
        float distance, float speed, float decay, float guiShakeModifier, bool multiplyByTimeScale)
    {
        Vector3 seed = Random.insideUnitSphere;

        foreach (Camera cam in cameras)
        {
            StartCoroutine(DoShake_Internal(cam, seed, numberOfShakes, shakeAmount, rotationAmount, 
                distance, speed, decay, guiShakeModifier, multiplyByTimeScale, null, null, null));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="callback"></param>
    public void DoShake(System.Action callback)
    {
        Vector3 seed = Random.insideUnitSphere;

        foreach (Camera cam in cameras)
        {
            StartCoroutine(DoShake_Internal(cam, seed, this.numberOfShakes, this.shakeAmount, this.rotationAmount, 
                this.distance, this.speed, this.decay, this.guiShakeModifier, this.multiplyByTimeScale, callback, null, null));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="numberOfShakes"></param>
    /// <param name="shakeAmount"></param>
    /// <param name="rotationAmount"></param>
    /// <param name="distance"></param>
    /// <param name="speed"></param>
    /// <param name="decay"></param>
    /// <param name="guiShakeModifier"></param>
    /// <param name="multiplyByTimeScale"></param>
    /// <param name="callback"></param>
    public void DoShake(int numberOfShakes, Vector3 shakeAmount, Vector3 rotationAmount, 
        float distance, float speed, float decay, float guiShakeModifier, bool multiplyByTimeScale, System.Action callback)
    {
        Vector3 seed = Random.insideUnitSphere;

        foreach (Camera cam in cameras)
        {
            StartCoroutine(DoShake_Internal(cam, seed, numberOfShakes, shakeAmount, rotationAmount,
                distance, speed, decay, guiShakeModifier, multiplyByTimeScale, callback, null, null));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="numberOfShakes"></param>
    /// <param name="shakeAmount"></param>
    /// <param name="rotationAmount"></param>
    /// <param name="distance"></param>
    /// <param name="speed"></param>
    /// <param name="decay"></param>
    /// <param name="guiShakeModifier"></param>
    /// <param name="multiplyByTimeScale"></param>
    /// <param name="luaCallBack"></param>
    /// <param name="luaModule"></param>
    public void DoShake(int numberOfShakes, Vector3 shakeAmount, Vector3 rotationAmount, float distance, float speed,
                        float decay, float guiShakeModifier, bool multiplyByTimeScale, LuaFunction luaCallBack,
                        LuaTable luaModule)
    {
        Vector3 seed = Random.insideUnitSphere;

        foreach (Camera cam in cameras)
        {
            StartCoroutine(DoShake_Internal(cam, seed, numberOfShakes, shakeAmount, rotationAmount,
                distance, speed, decay, guiShakeModifier, multiplyByTimeScale, null, luaCallBack, luaModule));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void DoCancelShake()
    {
        if (shaking && !cancelling)
        {
            shaking = false;
            this.StopAllCoroutines();
            foreach (Camera cam in cameras)
            {
                if (shakeCount.ContainsKey(cam))
                {
                    shakeCount[cam] = 0;
                }
                ResetState(cam.transform, cam);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="time"></param>
    public void DoCancelShake(float time)
    {
        if (shaking && !cancelling)
        {
            this.StopAllCoroutines();
            this.StartCoroutine(DoResetState(cameras, shakeCount, time));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void DoBeginShakeGUI()
    {
        CheckShakeRect();
        GUI.BeginGroup(shakeRect);
    }

    /// <summary>
    /// 
    /// </summary>
    public void DoEndShakeGUI()
    {
        GUI.EndGroup();
    }

    /// <summary>
    /// 
    /// </summary>
    public void DoBeginShakeGUILayout()
    {
        CheckShakeRect();
        GUILayout.BeginArea(shakeRect);
    }

    /// <summary>
    /// 
    /// </summary>
    public void DoEndShakeGUILayout()
    {
        GUILayout.EndArea();
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        foreach (Camera cam in cameras)
        {
            if (!cam)
                continue;

            if (IsShaking())
            {
                Vector3 offset = cam.worldToCameraMatrix.GetColumn(3);
                offset.z *= -1;
                offset = cam.transform.position + cam.transform.TransformPoint(offset);
                Quaternion rot = QuaternionFromMatrix(cam.worldToCameraMatrix.inverse * Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1)));
                Matrix4x4 matrix = Matrix4x4.TRS(offset, rot, cam.transform.lossyScale);
                Gizmos.matrix = matrix;
            }
            else
            {
                Matrix4x4 matrix = Matrix4x4.TRS(cam.transform.position, cam.transform.rotation, cam.transform.lossyScale);
                Gizmos.matrix = matrix;
            }

            Gizmos.DrawWireCube(Vector3.zero, shakeAmount);

            Gizmos.color = Color.cyan;

            if (cam.orthographic)
            {
                Vector3 pos = new Vector3(0, 0, (cam.nearClipPlane + cam.farClipPlane) / 2f);
                Vector3 size = new Vector3(cam.orthographicSize / cam.aspect, cam.orthographicSize * 2f, cam.farClipPlane - cam.nearClipPlane);
                Gizmos.DrawWireCube(pos, size);
            }
            else
            {
                Gizmos.DrawFrustum(Vector3.zero, cam.fieldOfView, cam.farClipPlane, cam.nearClipPlane, (.7f / cam.aspect));
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cam"></param>
    /// <param name="seed"></param>
    /// <param name="numberOfShakes"></param>
    /// <param name="shakeAmount"></param>
    /// <param name="rotationAmount"></param>
    /// <param name="distance"></param>
    /// <param name="speed"></param>
    /// <param name="decay"></param>
    /// <param name="guiShakeModifier"></param>
    /// <param name="multiplyByTimeScale"></param>
    /// <param name="callback"></param>
    /// <param name="luaCallBack"></param>
    /// <param name="luaModule"></param>
    /// <returns></returns>
    private IEnumerator DoShake_Internal(Camera cam, Vector3 seed, int numberOfShakes, Vector3 shakeAmount,
                                         Vector3 rotationAmount, float distance, float speed, float decay,
                                         float guiShakeModifier, bool multiplyByTimeScale, System.Action callback, LuaFunction luaCallBack, LuaTable luaModule)
    {
        //Wait for async cancel operations to complete
        if (cancelling)
            yield return null;

        //Set random values
        var mod1 = seed.x > .5f ? 1 : -1;
        var mod2 = seed.y > .5f ? 1 : -1;
        var mod3 = seed.z > .5f ? 1 : -1;

        //First shake
        if (!shaking)
        {
            shaking = true;

            if (cameraShakeStarted != null)
                cameraShakeStarted();
        }

        if (shakeCount.ContainsKey(cam))
            shakeCount[cam]++;
        else
            shakeCount.Add(cam, 1);

        //Pixel width is always based on the first camera
        float pixelWidth = GetPixelWidth(cameras[0].transform, cameras[0]);

        //Set other values
        Transform cachedTransform = cam.transform;
        Vector3 camOffset = Vector3.zero;
        Quaternion camRot = Quaternion.identity;

        int currentShakes = numberOfShakes;
        float shakeDistance = distance;
        float rotationStrength = 1;

        float startTime = Time.time;
        float scale = multiplyByTimeScale ? Time.timeScale : 1;
        float pixelScale = pixelWidth * guiShakeModifier * scale;
        Vector3 start1 = Vector3.zero;
        Quaternion startR = Quaternion.identity;
        Vector2 start2 = Vector2.zero;

        ShakeState state = new ShakeState(cachedTransform.position, cachedTransform.rotation, new Vector2(shakeRect.x, shakeRect.y));
        List<ShakeState> stateList;

        if (states.TryGetValue(cam, out stateList))
        {
            stateList.Add(state);
        }
        else
        {
            stateList = new List<ShakeState>();
            stateList.Add(state);
            states.Add(cam, stateList);
        }

        //Main loop
        while (currentShakes > 0)
        {
            if (checkForMinimumValues)
            {
                //Early break when rotation is less than the minimum value
                if (rotationAmount.sqrMagnitude != 0 && rotationStrength <= minRotationValue)
                    break;

                //Early break when shake amount is less than the minimum value
                if (shakeAmount.sqrMagnitude != 0 && distance != 0 && shakeDistance <= minShakeValue)
                    break;
            }

            var timer = (Time.time - startTime) * speed;

            state.shakePosition = start1 +
                                  new Vector3(mod1 * Mathf.Sin(timer) * (shakeAmount.x * shakeDistance * scale),
                                              mod2 * Mathf.Cos(timer) * (shakeAmount.y * shakeDistance * scale),
                                              mod3 * Mathf.Sin(timer) * (shakeAmount.z * shakeDistance * scale));

            state.shakeRotation = startR *
                                  Quaternion.Euler(mod1 * Mathf.Cos(timer) * (rotationAmount.x * rotationStrength * scale),
                                                   mod2 * Mathf.Sin(timer) * (rotationAmount.y * rotationStrength * scale),
                                                   mod3 * Mathf.Cos(timer) * (rotationAmount.z * rotationStrength * scale));

            state.guiShakePosition = new Vector2(start2.x - (mod1 * Mathf.Sin(timer) * (shakeAmount.x * shakeDistance * pixelScale)), 
                start2.y - (mod2 * Mathf.Cos(timer) * (shakeAmount.y * shakeDistance * pixelScale)));

            camOffset = GetGeometricAvg(stateList, true);
            camRot = GetAvgRotation(stateList);
            NormalizeQuaternion(ref camRot);

            Matrix4x4 m = Matrix4x4.TRS(camOffset, camRot, new Vector3(1, 1, -1));

            cam.worldToCameraMatrix = m * cachedTransform.worldToLocalMatrix;

            var avg = GetGeometricAvg(stateList, false);

            shakeRect.x = avg.x;
            shakeRect.y = avg.y;

            if (timer > Mathf.PI * 2)
            {
                startTime = Time.time;
                shakeDistance *= (1 - Mathf.Clamp01(decay));
                rotationStrength *= (1 - Mathf.Clamp01(decay));
                currentShakes--;
            }
            yield return null;
        }

        //End conditions

        shakeCount[cam]--;

        //Last shake
        if (shakeCount[cam] == 0)
        {
            shaking = false;
            ResetState(cam.transform, cam);

            if (allCameraShakesCompleted != null)
            {
                allCameraShakesCompleted();
            }
        }
        else
        {
            stateList.Remove(state);
        }

        if (callback != null)
            callback();

        if ((luaCallBack != null) && (luaModule != null))
        {
            AnimationEvent e = new AnimationEvent();
            e.stringParameter = "shakeFinish";
            luaCallBack.call(luaModule, e);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="states"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    private Vector3 GetGeometricAvg(List<ShakeState> states, bool position)
    {
        float x = 0, y = 0, z = 0, l = states.Count;

        foreach (ShakeState state in states)
        {
            if (position)
            {
                x -= state.shakePosition.x;
                y -= state.shakePosition.y;
                z -= state.shakePosition.z;
            }
            else
            {
                x += state.guiShakePosition.x;
                y += state.guiShakePosition.y;
            }
        }

        return new Vector3(x / l, y / l, z / l);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="states"></param>
    /// <returns></returns>
    private Quaternion GetAvgRotation(List<ShakeState> states)
    {
        Quaternion avg = new Quaternion(0, 0, 0, 0);

        foreach (ShakeState state in states)
        {
            if (Quaternion.Dot(state.shakeRotation, avg) > 0)
            {
                avg.x += state.shakeRotation.x;
                avg.y += state.shakeRotation.y;
                avg.z += state.shakeRotation.z;
                avg.w += state.shakeRotation.w;
            }
            else
            {
                avg.x += -state.shakeRotation.x;
                avg.y += -state.shakeRotation.y;
                avg.z += -state.shakeRotation.z;
                avg.w += -state.shakeRotation.w;
            }
        }

        var mag = Mathf.Sqrt(avg.x * avg.x + avg.y * avg.y + avg.z * avg.z + avg.w * avg.w);

        if (mag > 0.0001f)
        {
            avg.x /= mag;
            avg.y /= mag;
            avg.z /= mag;
            avg.w /= mag;
        }
        else
        {
            avg = states[0].shakeRotation;
        }

        return avg;
    }

    /// <summary>
    /// 
    /// </summary>
    private void CheckShakeRect()
    {
        if (Screen.width != shakeRect.width || Screen.height != shakeRect.height)
        {

            shakeRect.width = Screen.width;
            shakeRect.height = Screen.height;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cachedTransform"></param>
    /// <param name="cachedCamera"></param>
    /// <returns></returns>
    private float GetPixelWidth(Transform cachedTransform, Camera cachedCamera)
    {
        var position = cachedTransform.position;
        var screenPos = cachedCamera.WorldToScreenPoint(position - cachedTransform.forward * .01f);
        var offset = Vector3.zero;

        if (screenPos.x > 0)
            offset = screenPos - Vector3.right;
        else
            offset = screenPos + Vector3.right;

        if (screenPos.y > 0)
            offset = screenPos - Vector3.up;
        else
            offset = screenPos + Vector3.up;

        offset = cachedCamera.ScreenToWorldPoint(offset);

        return 1f / (cachedTransform.InverseTransformPoint(position) - cachedTransform.InverseTransformPoint(offset)).magnitude;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cachedTransform"></param>
    /// <param name="cam"></param>
    private void ResetState(Transform cachedTransform, Camera cam)
    {
        cam.ResetWorldToCameraMatrix();

        shakeRect.x = 0;
        shakeRect.y = 0;

        states[cam].Clear();
    }

    /// <summary>
    /// 
    /// </summary>
    private List<Vector3> offsetCache = new List<Vector3>(10);
    private List<Quaternion> rotationCache = new List<Quaternion>(10);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cameras"></param>
    /// <param name="shakeCount"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    private IEnumerator DoResetState(List<Camera> cameras, Dictionary<Camera, int> shakeCount, float time)
    {
        offsetCache.Clear();
        rotationCache.Clear();

        foreach (Camera cam in cameras)
        {
            offsetCache.Add((Vector3)((cam.worldToCameraMatrix * cam.transform.worldToLocalMatrix.inverse).GetColumn(3)));
            rotationCache.Add(QuaternionFromMatrix((cam.worldToCameraMatrix * cam.transform.worldToLocalMatrix.inverse).inverse * Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1))));

            if (shakeCount.ContainsKey(cam))
            {
                shakeCount[cam] = 0;
            }
            states[cam].Clear();
        }

        float t = 0;
        float x = shakeRect.x, y = shakeRect.y;
        cancelling = true;
        while (t < time)
        {
            int i = 0;
            foreach (Camera cam in cameras)
            {
                Transform cachedTransform = cam.transform;

                shakeRect.x = Mathf.Lerp(x, 0, t / time);
                shakeRect.y = Mathf.Lerp(y, 0, t / time);

                Vector3 pos = Vector3.Lerp(offsetCache[i], Vector3.zero, t / time);
                Quaternion rot = Quaternion.Slerp(rotationCache[i], cachedTransform.rotation, t / time);
                Matrix4x4 m = Matrix4x4.TRS(pos, rot, new Vector3(1, 1, -1));

                cam.worldToCameraMatrix = m * cachedTransform.worldToLocalMatrix;
                i++;
            }
            t += Time.deltaTime;
            yield return null;
        }

        foreach (Camera cam in cameras)
        {
            cam.ResetWorldToCameraMatrix();
            shakeRect.x = 0;
            shakeRect.y = 0;
        }
        this.shaking = false;
        this.cancelling = false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="m"></param>
    /// <returns></returns>
    private static Quaternion QuaternionFromMatrix(Matrix4x4 m)
    {
        return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="q"></param>
    private static void NormalizeQuaternion(ref Quaternion q)
    {
        float sum = 0;

        for (int i = 0; i < 4; ++i)
            sum += q[i] * q[i];

        float magnitudeInverse = 1 / Mathf.Sqrt(sum);

        for (int i = 0; i < 4; ++i)
            q[i] *= magnitudeInverse;
    }
}
