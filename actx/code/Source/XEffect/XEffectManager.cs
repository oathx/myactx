using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SLua;

[CustomLuaClass]
public sealed class XEffectManager
{
    public Transform objectContainer = null;
    int _effectGlobalId = 1;

    private int _maxEffectCacheCount = 20; 
    private int _maxEffectQueueCount = 4;
    private float _effectQueueTimeThreshold = 10f;

    private int _maxBulletCacheCount = 10;
    private int _maxBullectQueueCount = 4;
    private float _bulletQueueTimeThreshold = 10f;

    private List<XEffectCache> _cacheList = new List<XEffectCache>();
    private Dictionary<int, XEffectComponent> _map = new Dictionary<int, XEffectComponent>();

    private List<BulletCache> _bulletCacheList = new List<BulletCache>();
    private Dictionary<int, XBulletComponent> _bulletMap = new Dictionary<int, XBulletComponent>();
    private List<XBulletComponent> _destroyMap = new List<XBulletComponent>();

    private Dictionary<string, int> _maxEffectCountDic = new Dictionary<string, int>();
    private Dictionary<string, int> _currentEffectCountDic = new Dictionary<string, int>();

    private class XEffectCache : CacheBase
    {
        public Queue<XEffectComponent> queue;
        public override void Release()
        {
            if (queue != null)
            {
                while (queue.Count > 0)
                {
                    XEffectComponent efc = queue.Dequeue();
                    if (efc != null && efc.gameObject != null)
                    {
                        GameObject.Destroy(efc.gameObject);
                    }
                }
                queue.Clear();
                queue = null;
            }
            base.Release();
        }
    }

    private class BulletCache : CacheBase
    {
        public Queue<XBulletComponent> queue;
        public override void Release()
        {
            if (queue != null)
            {
                while (queue.Count > 0)
                {
                    XBulletComponent efc = queue.Dequeue();
                    if (efc != null && efc.gameObject != null)
                    {
                        GameObject.Destroy(efc.gameObject);
                    }
                }
                queue.Clear();
                queue = null;
            }
            base.Release();
        }
    }

    private class CacheBase
    {
        public string effectPath;
        public GameObject prefab;
        public bool loading = false;
        public float createTime = 0f;

        public virtual void Release()
        {
            prefab = null;
            loading = false;
            createTime = 0f;
        }
    }

    public static XEffectManager Instance { get; internal set; }

    static XEffectManager()
    {
        Instance = new XEffectManager();
        Instance.InitialCacheStrategy();
    }

    public void InitialCacheStrategy()
    {
        if (XUtility.IsLowMemoryDevice())
        {
            _maxEffectCacheCount = 15;
            _maxEffectQueueCount = 4;
            _effectQueueTimeThreshold = 8f;

            _maxBulletCacheCount = 6;
            _maxBullectQueueCount = 2;
            _bulletQueueTimeThreshold = 8f;
        }
        else
        {
            _maxEffectCacheCount = 64;
            _maxEffectQueueCount = 8;
            _effectQueueTimeThreshold = 20f;

            _maxBulletCacheCount = 15;
            _maxBullectQueueCount = 4;
            _bulletQueueTimeThreshold = 20f;
        }
    }

    private void AddEffectMap(XEffectComponent effect)
    {
        if (effect == null) return;
        effect.gid = _effectGlobalId++;
        _map.Add(effect.gid, effect);
        AddEffectCount(effect.resPath);
    }

    //add cache
    private void AddEffectCache(string path, XEffectCache cache)
    {
        AddCache<XEffectCache>(_cacheList, _maxEffectCacheCount, path, cache);
        ReleaseAllEffectQueueInCacheByTimeThreshold();
    }

    private void AddBulletCache(string path, BulletCache cache)
    {
        AddCache<BulletCache>(_bulletCacheList, _maxBulletCacheCount, path, cache);
        ReleaseAllBulletQueueInCacheByTimeThreshold();
    }

    private void AddCache<T>(List<T> cacheList, int maxCacheCount, string path, T cache) where T : CacheBase
    {
        cache.createTime = Time.time;
        cache.effectPath = path;
        if (cacheList.Count < maxCacheCount)
        {
            cacheList.Add(cache);
        }
        else
        {
            cacheList.Sort(delegate (T x, T y)
            {
                return (int)(x.createTime - y.createTime);
            });
            cacheList[0].Release();
            cacheList[0] = cache;
        }
    }

    private void RemoveCache<T>(List<T> cacheList, T cache) where T : CacheBase
    {
        if (cache != null && cacheList.Contains(cache))
        {
            cacheList.Remove(cache);
        }
    }

    private void RemoveCache<T>(List<T> cacheList, string path) where T : CacheBase
    {
        T cache = cacheList.Find(delegate (T obj)
        {
            return obj.effectPath == path;
        });

        if (cache != null)
        {
            cacheList.Remove(cache);
        }
    }

    // get cache
    private bool TryGetEffectCache(string path, out XEffectCache effectCache)
    {
        return TryGetCache<XEffectCache>(_cacheList, path, out effectCache);
    }

    private bool TryGetBulletCache(string path, out BulletCache bulletCache)
    {
        return TryGetCache<BulletCache>(_bulletCacheList, path, out bulletCache);
    }

    private bool TryGetCache<T>(List<T> cacheList, string path, out T cache) where T : CacheBase
    {
        T effectInCache = cacheList.Find(delegate (T obj) {
            return obj.effectPath == path;
        });
        cache = effectInCache;
        if (cache != null)
        {
            cache.createTime = Time.time;
        }
        return cache != null;
    }

    // release queue
    private void ReleaseAllEffectQueueInCacheByTimeThreshold()
    {
        for (int i = 0; i < _cacheList.Count; i++)
        {
            if ((Time.time - _cacheList[i].createTime) > _effectQueueTimeThreshold && _cacheList[i].queue != null)
            {
                while (_cacheList[i].queue.Count > _maxEffectQueueCount)
                {
                    XEffectComponent efc = _cacheList[i].queue.Dequeue();
                    GameObject.Destroy(efc.gameObject);
                }
            }
        }
    }

    private void ReleaseAllBulletQueueInCacheByTimeThreshold()
    {
        for (int i = 0; i < _bulletCacheList.Count; i++)
        {
            if ((Time.time - _bulletCacheList[i].createTime) > _bulletQueueTimeThreshold && _bulletCacheList[i].queue != null)
            {
                while (_bulletCacheList[i].queue.Count > _maxBullectQueueCount)
                {
                    XBulletComponent efc = _bulletCacheList[i].queue.Dequeue();
                    GameObject.Destroy(efc.gameObject);
                }
            }
        }
    }

    private bool CheckReleaseQueueInCacheByTimeThreshold(XEffectCache cache, float timeThreshold)
    {
        bool canEnqueue = true;
        if ((Time.time - cache.createTime) > timeThreshold && cache.queue != null)
        {
            while (cache.queue.Count > _maxEffectQueueCount)
            {
                XEffectComponent efc = cache.queue.Dequeue();
                GameObject.Destroy(efc.gameObject);
                canEnqueue = false;
            }
        }
        return canEnqueue;
    }

    private bool CheckReleaseQueueInCacheByTimeThreshold(XEffectCache cache)
    {
        return CheckReleaseQueueInCacheByTimeThreshold(cache, _effectQueueTimeThreshold);
    }

    private XEffectComponent GenerateEffect(string effectPath, XEffectCache effectCache)
    {
        XEffectComponent effect = null;
        if (effectCache.queue.Count > 0)
        {
            effect = effectCache.queue.Dequeue();
            if (effect == null || effect.gameObject == null)
            {
                effect = null;
            }
            else
            {
                effect.gameObject.SetActive(true);
            }
        }
        else
        {
            if (effectCache.prefab == null)
            {
                RemoveCache<XEffectCache>(_cacheList, effectCache);
                return null;
            }

            if (!CanEffectInstantial(effectPath))
                return null;

            GameObject obj = GameObject.Instantiate(effectCache.prefab) as GameObject;
            effect = obj.GetComponent<XEffectComponent>();
            if (effect != null)
                effect.resPath = effectPath;
        }

        AddEffectMap(effect);
        return effect;
    }

    private IEnumerator GetEffect(string effectPath, System.Action<XEffectCache, XEffectComponent> callback)
    {
        XEffectCache effectCache = null;
        XEffectComponent effect = null;

        TryGetEffectCache(effectPath, out effectCache);
        if (effectCache == null)
        {
            effectCache = new XEffectCache();
            effectCache.loading = true;
            AddEffectCache(effectPath, effectCache);

            yield return XRes.LoadAsync<GameObject>(effectPath, delegate (Object obj) {
                effectCache.queue = new Queue<XEffectComponent>();
                effectCache.prefab = obj as GameObject;
                effectCache.loading = false;
            });
        }

        while (effectCache.loading)
            yield return null;

        if (effectCache.prefab != null)
        {
            effect = GenerateEffect(effectPath, effectCache);
        }

        callback(effectCache, effect);
    }

    private IEnumerator GetEffect(GameObject effectPrefab, System.Action<XEffectCache, XEffectComponent> callback)
    {
        XEffectCache effectCache = null;
        XEffectComponent effect = null;

        string effectPath = effectPrefab.name;
        TryGetEffectCache(effectPath, out effectCache);
        if (effectCache == null)
        {
            effectCache = new XEffectCache();
            effectCache.loading = true;
            AddEffectCache(effectPath, effectCache);

            effectCache.queue = new Queue<XEffectComponent>();
            effectCache.prefab = effectPrefab;
            effectCache.loading = false;
        }

        while (effectCache.loading)
            yield return null;

        if (effectCache.prefab != null)
        {
            effect = GenerateEffect(effectPath, effectCache);
        }

        callback(effectCache, effect);
    }

    public void PreloadEffectPrefabs(string[] resList, System.Action<bool> callback)
    {
        XCoroutine.Run(DoPreloadEffectPrefabs(resList, callback));
    }

    private IEnumerator DoPreloadEffectPrefabs(string[] resList, System.Action<bool> callback)
    {
        List<string> resLoad = new List<string>();
        for (int i = 0; i < resList.Length; i++)
        {
            string effectPath = resList[i];
            XEffectCache effectCache = null;
            TryGetEffectCache(effectPath, out effectCache);
            if (effectCache == null)
            {
                effectCache = new XEffectCache();
                AddEffectCache(effectPath, effectCache);
            }

            if (!effectCache.loading)
            {
                effectCache.loading = true;
                resLoad.Add(effectPath);
            }
        }

        yield return XRes.LoadMultiAsync(resLoad.ToArray(), delegate (Object[] obj) {
            for (int i = 0; i < obj.Length; i++)
            {
                XEffectCache effectCache = null;
                if (TryGetEffectCache(resLoad[i], out effectCache))
                {
                    effectCache.prefab = obj[i] as GameObject;
                    effectCache.queue = new Queue<XEffectComponent>();
                    effectCache.loading = false;
                }
            }

            if (callback != null)
                callback(true);
        });
    }

    public void PreloadBulletPrefabs(string[] resList, System.Action<bool> callback)
    {
        XCoroutine.Run(DoPreloadBulletPrefabs(resList, callback));
    }

    private IEnumerator DoPreloadBulletPrefabs(string[] resList, System.Action<bool> callback)
    {
        List<string> resLoad = new List<string>();
        for (int i = 0; i < resList.Length; i++)
        {
            string effectPath = resList[i];
            BulletCache bulletCache = null;
            TryGetBulletCache(effectPath, out bulletCache);
            if (bulletCache == null)
            {
                bulletCache = new BulletCache();
                AddBulletCache(effectPath, bulletCache);
            }

            if (!bulletCache.loading)
            {
                bulletCache.loading = true;
                resLoad.Add(effectPath);
            }
        }

        yield return XRes.LoadMultiAsync(resLoad.ToArray(), delegate (Object[] obj) {
            for (int i = 0; i < obj.Length; i++)
            {
                BulletCache bulletCache = null;
                if (TryGetBulletCache(resLoad[i], out bulletCache))
                {
                    bulletCache.prefab = obj[i] as GameObject;
                    bulletCache.queue = new Queue<XBulletComponent>();
                    bulletCache.loading = false;
                }
            }

            if (callback != null)
                callback(true);
        });
    }

    [DoNotToLua]
    public void DestroyEffect(XEffectComponent effect, bool destroyed = false)
    {
        if (effect == null) return;

        if (!destroyed)
        {
            XEffectCache effectCache = null;
            TryGetEffectCache(effect.resPath, out effectCache);
            if (effectCache != null)
            {
                if (effectCache.queue == null)
                    effectCache.queue = new Queue<XEffectComponent>();
                effectCache.queue.Enqueue(effect);
                effect.transform.parent = objectContainer;
                effect.gameObject.SetActive(false);
            }
            else if (effect.gameObject != null)
            {
                GameObject.Destroy(effect.gameObject);
            }
        }

        if (effect.gid > 0)
        {
            _map.Remove(effect.gid);
            effect.gid = 0;
            RemoveEffectCount(effect.resPath);
        }
    }

    [DoNotToLua]
    public void DestroyEffect(int effectId)
    {
        XEffectComponent effect = null;
        _map.TryGetValue(effectId, out effect);
        if (effect != null)
            DestroyEffect(effect, false);
    }

    [DoNotToLua]
    public void AddDestroyBullet(XBulletComponent bullet)
    {
        _destroyMap.Add(bullet);
    }

    public static void Destroy(int effectId)
    {
        Instance.DestroyEffect(effectId);
    }

    private void DoGenerateAsync(string effectPath, System.Action<XEffectCache, XEffectComponent> callback)
    {
        XEffectCache effectCache = null;

        TryGetEffectCache(effectPath, out effectCache);

        if (effectCache != null && !effectCache.loading)
        {
            callback(effectCache, GenerateEffect(effectPath, effectCache));
        }
        else
        {
            XCoroutine.Run(GetEffect(effectPath, callback));
        }
    }

    public static void GenerateAsync(GameObject owner, Vector3 position, Quaternion rotation, string effectPath, float time,
        System.Action<XEffectComponent> callback)
    {
        Instance.DoGenerateAsync(effectPath, delegate (XEffectCache effectCache, XEffectComponent effect) {
            if (effect != null)
                effect.Initialize(owner, position, rotation, effectCache.prefab, time);

            if (callback != null)
                callback(effect);
        });
    }


    public static void GenerateAsync(GameObject parent, string effectPath, float time, System.Action<XEffectComponent> callback)
    {
        Instance.DoGenerateAsync(effectPath, delegate (XEffectCache effectCache, XEffectComponent effect) {
            if (effect != null)
            {
                effect.Initialize(parent, effectCache.prefab, time);
            }

            if (callback != null)
                callback(effect);
        });
    }

    public static void GenerateAsync(GameObject effectPrefab, float time, System.Action<XEffectComponent> callback)
    {
        XCoroutine.Run(Instance.GetEffect(effectPrefab, delegate (XEffectCache effectCache, XEffectComponent effect) {
            if (effect != null)
                effect.Initialize(effect.gameObject, effectCache.prefab, time);

            if (callback != null)
                callback(effect);
        }));
    }


    private bool CanEffectInstantial(string effectPath)
    {
        if (_currentEffectCountDic.ContainsKey(effectPath) && _currentEffectCountDic[effectPath] >= _maxEffectCountDic[effectPath])
        {
            return false;
        }
        return true;
    }

    private void AddEffectCount(string effectPath)
    {
        if (_maxEffectCountDic.ContainsKey(effectPath))
        {
            if (!_currentEffectCountDic.ContainsKey(effectPath))
                _currentEffectCountDic[effectPath] = 0;
            _currentEffectCountDic[effectPath] += 1;
        }
    }
    private void RemoveEffectCount(string effectPath)
    {
        if (_currentEffectCountDic.ContainsKey(effectPath) && _currentEffectCountDic[effectPath] > 0)
        {
            _currentEffectCountDic[effectPath] -= 1;
        }
    }

    public void SetMaxEffectCount(string effectPath, int maxCount)
    {
        _maxEffectCountDic[effectPath] = maxCount;
    }

    private void AddBulletMap(XBulletComponent bullet)
    {
        if (bullet == null) return;
        bullet.gid = _effectGlobalId++;
        _bulletMap.Add(bullet.gid, bullet);
    }


    [DoNotToLua]
    public void DestroyBullet(XBulletComponent bullet)
    {
        BulletCache bulletCache = null;
        TryGetBulletCache(bullet.resPath, out bulletCache);
        if (bulletCache != null)
        {
            bulletCache.queue.Enqueue(bullet);
            bullet.transform.parent = objectContainer;
            bullet.gameObject.SetActive(false);
        }
        else if (bullet.gameObject != null)
        {
            GameObject.Destroy(bullet.gameObject);
        }

        if (bullet.gid > 0)
        {
            _bulletMap.Remove(bullet.gid);
            bullet.gid = 0;
        }
    }

    public void BulletFixedUpdate(float fFixedDeltaTime)
    {
        foreach (KeyValuePair<int, XBulletComponent> it in _bulletMap)
        {
            it.Value.ForceFixedUpdate(fFixedDeltaTime);
        }

        for (int i = 0; i < _destroyMap.Count; i++)
        {
            XBulletComponent bullet = _destroyMap[i];
            if (bullet != null)
                DestroyBullet(bullet);
        }
        _destroyMap.Clear();
    }

    public static void FixedUpdate(float fFixedDeltaTime)
    {
        Instance.BulletFixedUpdate(fFixedDeltaTime);
    }

    [DoNotToLua]
    public void DestroyBulletById(int id)
    {
        XBulletComponent bullet = null;
        _bulletMap.TryGetValue(id, out bullet);
        if (bullet != null)
            DestroyBullet(bullet);
    }

    public static void DestroyBullet(int id)
    {
        Instance.DestroyBulletById(id);
    }

    private XBulletComponent GenerateBullet(string bulletPath, BulletCache bulletCache)
    {
        XBulletComponent bullet = null;
        if (bulletCache.queue.Count > 0)
        {
            bullet = bulletCache.queue.Dequeue();
            bullet.gameObject.SetActive(true);
        }
        else
        {
            if (bulletCache.prefab == null)
            {
                RemoveCache<BulletCache>(_bulletCacheList, bulletCache);
                return null;
            }
            GameObject obj = GameObject.Instantiate(bulletCache.prefab) as GameObject;
            bullet = obj.GetComponent<XBulletComponent>();
            if (bullet != null)
                bullet.resPath = bulletPath;
        }

        AddBulletMap(bullet);
        return bullet;
    }

    private IEnumerator GetBullet(string bulletPath, System.Action<XBulletComponent> callback)
    {
        BulletCache bulletCache = null;
        XBulletComponent bullet = null;

        TryGetBulletCache(bulletPath, out bulletCache);
        if (bulletCache == null)
        {
            bulletCache = new BulletCache();
            bulletCache.loading = true;
            AddBulletCache(bulletPath, bulletCache);

            yield return XRes.LoadAsync<GameObject>(bulletPath, delegate (Object obj) {
                bulletCache.queue = new Queue<XBulletComponent>();
                bulletCache.prefab = obj as GameObject;
                bulletCache.loading = false;
            });
        }

        while (bulletCache.loading)
            yield return null;

        if (bulletCache.prefab != null)
        {
            bullet = GenerateBullet(bulletPath, bulletCache);
        }

        callback(bullet);
    }

    private static void FireBullet(GameObject go, Vector3 dest, XBulletProperty property,
        System.Func<XBulletComponent, Transform, bool> callback, Transform[] hitTargets)
    {
        XBulletComponent bullet = go.GetComponent<XBulletComponent>();
        if (bullet != null)
        {
            bullet.Fire(dest, property, hitTargets, callback);
        }
        else
        {
            GameObject.Destroy(go);
        }
    }

    public static void GenerateBulletAsync(bool isPvp, string path, Vector3 position, Vector3 dest, XBulletProperty property,
        System.Func<XBulletComponent, Transform, bool> callback, Transform[] hitTargets, System.Action<XBulletComponent> loadDone)
    {
        XCoroutine.Run(Instance.GetBullet(path, delegate (XBulletComponent bullet) {
            if (bullet != null)
            {
                bullet.transform.position = position;
                bullet.transform.rotation = Quaternion.identity;
                if (position.x >= dest.x)
                    bullet.transform.localEulerAngles = new Vector3(0, 180, 0);

                if (isPvp)
                    property.time = Mathf.RoundToInt(Vector3.Distance(position, dest) / property.speed * 1000) * 0.001f;

                bullet.Fire(dest, property, hitTargets, callback);
            }

            if (loadDone != null)
                loadDone(bullet);
        }));
    }

    public static void GenerateBulletAsync(bool isPvp, string path, Vector3 position, Vector3 dest, XBulletProperty property,
        System.Func<XBulletComponent, Transform, bool> callback, Transform[] hitTargets)
    {
        GenerateBulletAsync(isPvp, path, position, dest, property, callback, hitTargets, null);
    }

    public static XEffectComponent GetEffect(int id)
    {
        XEffectComponent effect = null;
        Instance._map.TryGetValue(id, out effect);
        return effect;
    }

    public static XBulletComponent GetBullet(int id)
    {
        XBulletComponent bullet = null;
        Instance._bulletMap.TryGetValue(id, out bullet);
        return bullet;
    }

    public static void DestroyAllBullet()
    {
        foreach (KeyValuePair<int, XBulletComponent> entry in Instance._bulletMap)
        {
            XBulletComponent bullet = entry.Value;

            BulletCache bulletCache = null;
            Instance.TryGetBulletCache(bullet.resPath, out bulletCache);
            if (bulletCache != null)
            {
                bulletCache.queue.Enqueue(bullet);
                bullet.transform.parent = Instance.objectContainer;
                bullet.gameObject.SetActive(false);
            }
            else
            {
                GameObject.Destroy(bullet.gameObject);
            }

            bullet.gid = 0;
        }
        Instance._bulletMap.Clear();
    }

    public void ClearCache()
    {
        foreach (Transform child in objectContainer)
        {
            GameObject.Destroy(child.gameObject);
        }

        _cacheList.Clear();
        _map.Clear();

        _bulletCacheList.Clear();
        _bulletMap.Clear();

        _currentEffectCountDic.Clear();
    }

    public void ClearQueueInCache()
    {
        for (int i = 0; i < _cacheList.Count; i++)
        {
            if (_cacheList[i].queue != null)
            {
                while (_cacheList[i].queue.Count > 0)
                {
                    XEffectComponent eff = _cacheList[i].queue.Dequeue();
                    if (eff != null && eff.gameObject != null)
                        GameObject.Destroy(eff.gameObject);
                }
            }
        }

        for (int i = 0; i < _bulletCacheList.Count; i++)
        {
            if (_bulletCacheList[i].queue != null)
            {
                while (_bulletCacheList[i].queue.Count > 0)
                {
                    XBulletComponent eff = _bulletCacheList[i].queue.Dequeue();
                    if (eff != null && eff.gameObject != null)
                        GameObject.Destroy(eff.gameObject);
                }
            }
        }
        _currentEffectCountDic.Clear();
    }
}
