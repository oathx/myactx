using System.Collections.Generic;
using UnityEngine;
using SLua;

public class XBoxOverlap
{
    public XBoxComponent Hero;
    public int Overlap;
}

public class XBoxBloody
{
    public XBoxComponent AttackHero
    {
        get
        {
            return _attackHero;
        }
    }

    public List<int> BloodyHeroes
    {
        get
        {
            return _bloodyHeroes;
        }
    }

    public List<XBoxRect> OverlapBoxes
    {
        get
        {
            return _boxes;
        }
    }

    private XBoxComponent _attackHero;

    private List<int> _bloodyHeroes;
    private List<XBoxRect> _boxes;

    public XBoxBloody(XBoxComponent attack)
    {
        _attackHero = attack;
    }

    public void AddBloodyHero(XBoxComponent receive, XBoxRect box)
    {
        if (_bloodyHeroes == null)
            _bloodyHeroes = new List<int>();

        if (_boxes == null)
            _boxes = new List<XBoxRect>();

        _bloodyHeroes.Add(receive.cid);
        _boxes.Add(box);
    }

    public void AddBloodyHero(int receiveId, XBoxRect box)
    {
        if (_bloodyHeroes == null)
            _bloodyHeroes = new List<int>();

        if (_boxes == null)
            _boxes = new List<XBoxRect>();

        _bloodyHeroes.Add(receiveId);
        _boxes.Add(box);
    }

    public void Clear()
    {
        if (_bloodyHeroes != null)
            _bloodyHeroes.Clear();

        if (_boxes != null)
            _boxes.Clear();
    }
}

public class XBulletHero
{
    public XBulletComponent Bullet;
    public List<Transform> TransList;

    public XBulletHero()
    {
        TransList = new List<Transform>();
    }

    public void Clear()
    {
        TransList.Clear();
    }
}

public class XBodyMeetCache
{
    private List<List<XBoxComponent>> _cache = new List<List<XBoxComponent>>();

    public List<List<XBoxComponent>> CacheList
    {
        get { return _cache; }
    }

    public void AddCache(XBoxComponent src, XBoxComponent hc)
    {
        List<XBoxComponent> cacheExis = GetCacheInCache(src);

        if (cacheExis != null)
        {
            if (!cacheExis.Contains(hc))
                cacheExis.Add(hc);
            return;
        }

        List<XBoxComponent> cache = new List<XBoxComponent>();
        cache.Add(src);
        cache.Add(hc);
        _cache.Add(cache);
    }

    public List<XBoxComponent> GetCacheInCache(XBoxComponent hc)
    {
        if (_cache.Count > 0)
        {
            for (int i = 0; i < _cache.Count; i++)
            {
                if (_cache[i] != null)
                {
                    for (int index = 0; index < _cache[i].Count; index++)
                    {
                        if (hc == _cache[i][index])
                            return _cache[i];
                    }
                }
            }
        }
        return null;
    }

    public void Clear()
    {
        for (int i = 0; i < _cache.Count; i++)
        {
            if (_cache[i] != null)
            {
                _cache[i].Clear();
            }
            _cache.Clear();
        }
    }

}

public class XBoxIndex
{
    public XBoxComponent hc;
    public int index;
}

[CustomLuaClass]
public class XBoxSystem {
    private static readonly XBoxSystem instance = new XBoxSystem();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static XBoxSystem GetSingleton()
    {
        return instance;
    }

    protected List<XBoxComponent> _heroes;
    protected List<XBoxComponent> _receiveWarningHeroes;
    protected List<XBoxComponent> _lastReceiveWarningHeroes;
    protected List<XBoxComponent> _bodyAwayHeroes;
    protected List<XBoxComponent> _bodySortedHeroes;
    protected List<XBoxComponent> _hugEachList;
    protected List<XBoxComponent> _hugBeEatenList;
    protected List<XBulletComponent> _bullets;
    protected List<XBulletHero> _bulletHeroes;
    protected List<XBoxFlexibleHurt> _flexibleHurtBoxes;

    protected List<XBoxBloody> _bloodyHeroes;

    protected Dictionary<XBoxExtraProperty, List<XBoxExtraRect>> _extraBoxesDic;
    protected Dictionary<XBoxComponent, XBoxOverlap> _hugHeroesDic;

    protected bool _isTrial = false;
    protected const int _bodyMoveRuler = 10;
    protected const int _twoBodyMoveRuler = 100;
    protected XBodyMeetCache _meetCache = new XBodyMeetCache();

    /// <summary>
    /// 
    /// </summary>
    protected XBoxSystem()
    {
        Init();
    }

    /// <summary>
    /// 
    /// </summary>
    public void Init()
    {
        _heroes = new List<XBoxComponent>();
        _bloodyHeroes = new List<XBoxBloody>();
        _receiveWarningHeroes = new List<XBoxComponent>();
        _lastReceiveWarningHeroes = new List<XBoxComponent>();
        _bodyAwayHeroes = new List<XBoxComponent>();
        _extraBoxesDic = new Dictionary<XBoxExtraProperty, List<XBoxExtraRect>>();
        _bodySortedHeroes = new List<XBoxComponent>();

        _hugEachList = new List<XBoxComponent>();
        _hugBeEatenList = new List<XBoxComponent>();
        _hugHeroesDic = new Dictionary<XBoxComponent, XBoxOverlap>();

        _bulletHeroes = new List<XBulletHero>();
        _flexibleHurtBoxes = new List<XBoxFlexibleHurt>();
        _bullets = new List<XBulletComponent>();
    }

    /// <summary>
    /// 
    /// </summary>
    public void Clear()
    {
        _heroes.Clear();
        _bloodyHeroes.Clear();
        _receiveWarningHeroes.Clear();
        _lastReceiveWarningHeroes.Clear();
        _bodyAwayHeroes.Clear();
        _extraBoxesDic.Clear();
        _bodySortedHeroes.Clear();
        _hugEachList.Clear();
        _hugBeEatenList.Clear();
        _hugHeroesDic.Clear();
    }

    private void SortHeroesByPos()
    {
        for (int i = 0; i < _heroes.Count; i++)
        {
            if (_heroes[i] != null && _heroes[i].GetBoxesFlag(XBoxComponent.XBoxFlag.BodyBox) && !_bodySortedHeroes.Contains(_heroes[i]))
            {
                _bodySortedHeroes.Add(_heroes[i]);
            }
        }

        if (_bodySortedHeroes.Count >= 2)
        {
            _bodySortedHeroes.Sort(delegate (XBoxComponent x, XBoxComponent y)
            {
                return x.GetBodyMoveBox().MinX - y.GetBodyMoveBox().MinX;
            });
        }
    }

    private XBoxExtraRect ExtraBoxMeet(XBoxExtraProperty property, XBoxRect box)
    {
        int meet = -1;
        if (_extraBoxesDic.ContainsKey(property))
        {
            for (int extraIndex = 0; extraIndex < _extraBoxesDic[property].Count; extraIndex++)
            {
                meet = XBoxRect.Overlap(box, _extraBoxesDic[property][extraIndex]).Width;
                if (meet >= 0)
                {
                    return _extraBoxesDic[property][extraIndex];
                }
            }
        }
        return null;
    }

    private void CalDirectionalForce(List<XBoxIndex> forceBodies, int start, int linkBegin, List<XBoxIndex> hcStarts)
    {
        if (start >= forceBodies.Count)
            return;

        if (forceBodies[start].hc.GetBodyMoveBox().MoveX < 0)
        {
            if (start == forceBodies.Count - 1)
            {
                hcStarts.Add(forceBodies[start]);
                return;
            }

            if (forceBodies[start + 1].hc.GetBodyMoveBox().MoveX > 0)
                hcStarts.Add(forceBodies[start]);
            CalDirectionalForce(forceBodies, start + 1, start + 1, hcStarts);
        }
        else                          
        {
            if (start == forceBodies.Count - 1)
            {
                hcStarts.Add(forceBodies[linkBegin]);
                return;
            }

            if (forceBodies[start + 1].hc.GetBodyMoveBox().MoveX > 0)
            {
                CalDirectionalForce(forceBodies, start + 1, linkBegin, hcStarts);
            }
            else
            {
                CalDirectionalForce(forceBodies, start + 2, start + 2, hcStarts);
            }
        }
    }

    private void CalMove(List<List<XBoxComponent>> cacheList)
    {
        for (int i = 0; i < cacheList.Count; i++)
        {
            List<XBoxComponent> cache = cacheList[i];
            if (cache != null && cache.Count > 1)
            {
                List<XBoxIndex> forceBodyList = new List<XBoxIndex>();
                for (int index = 0; index < cache.Count; index++)
                {
                    if (cache[index].GetBodyMoveBox().MoveX != 0)
                    {
                        XBoxIndex hi = new XBoxIndex();
                        hi.hc = cache[index];
                        hi.index = index;
                        forceBodyList.Add(hi);
                    }

                    if (cache[index].GetBodyMoveBox().MoveY != 0)
                    {
                        int y = cache[index].GetBodyMoveBox().MoveY > 0 ? _bodyMoveRuler : -_bodyMoveRuler;
                        y = GetRoundZero(y, cache[index].GetBodyMoveBox().MoveY);
                        cache[index].SetLastDeltaPos(0, y);
                    }
                }

                List<XBoxIndex> bodyStarts = new List<XBoxIndex>();

                CalDirectionalForce(forceBodyList, 0, 0, bodyStarts);

                for (int index = 0; index < bodyStarts.Count; index++)
                {

                    if (bodyStarts[index].hc.GetBodyMoveBox().MoveX > 0)
                    {
                        XBoxRect extra = ExtraBoxMeet(XBoxExtraProperty.Wall, cache[cache.Count - 1].GetBodyMoveBox());
                        if (extra != null && (bodyStarts[index].hc.GetBodyMoveBox().MinX - extra.MinX) * bodyStarts[index].hc.GetBodyMoveBox().MoveX <= 0)
                            continue;
                        for (int moveIndex = bodyStarts[index].index; moveIndex < cache.Count; moveIndex++)
                        {
                            int moveStep = GetRoundZero(bodyStarts[index].hc.GetBodyMoveBox().MoveX, _bodyMoveRuler);
                            cache[moveIndex].SetLastDeltaPos(moveStep, 0);
                            cache[moveIndex].AdjustBodyMoveBox();
                        }
                    }
                    else
                    {
                        XBoxRect extra = ExtraBoxMeet(XBoxExtraProperty.Wall, cache[0].GetBodyMoveBox());
                        if (extra != null && (bodyStarts[index].hc.GetBodyMoveBox().MinX - extra.MinX) * bodyStarts[index].hc.GetBodyMoveBox().MoveX <= 0)
                            continue;

                        for (int moveIndex = bodyStarts[index].index; moveIndex >= 0; moveIndex--)
                        {
                            int moveStep = GetRoundZero(bodyStarts[index].hc.GetBodyMoveBox().MoveX, -_bodyMoveRuler);
                            cache[moveIndex].SetLastDeltaPos(moveStep, 0);
                            cache[moveIndex].AdjustBodyMoveBox();
                        }
                    }
                }
                forceBodyList.Clear();
                bodyStarts.Clear();
            }
        }
    }

    private int GetRoundZero(int x, int y)
    {
        if (x > 0)
            return Mathf.Min(x, y);
        else
            return Mathf.Max(x, y);
    }

    private void DealBodyMove()
    {
        int threshold = 300;
        while (threshold > 0)
        {
            for (int i = 0; i < _bodySortedHeroes.Count; i++)
            {
                for (int index = 0; index < _bodySortedHeroes.Count; index++)
                {
                    if (i < index)
                    {
                        int meet = XBoxRect.Overlap(_bodySortedHeroes[i].GetBodyMoveBox(), _bodySortedHeroes[index].GetBodyMoveBox()).Width;
                        if (meet >= 0)
                        {
                            XBoxRect extraI = ExtraBoxMeet(XBoxExtraProperty.Wall, _bodySortedHeroes[i].GetBodyMoveBox());
                            XBoxRect extrIndex = ExtraBoxMeet(XBoxExtraProperty.Wall, _bodySortedHeroes[index].GetBodyMoveBox());
                            if (extraI != null && extrIndex != null && extrIndex == extraI)
                            {
                                meet = _bodySortedHeroes[i].GetBodyMoveBox().MinX < 0 ? _bodyMoveRuler : -_bodyMoveRuler;
                                if (_bodySortedHeroes[i].GetBodyMoveBox().MinY > _bodySortedHeroes[index].GetBodyMoveBox().MinY)
                                {
                                    _bodySortedHeroes[index].SetLastDeltaPos(meet, 0);
                                    _bodySortedHeroes[index].AdjustBodyMoveBox();
                                }
                                else
                                {
                                    _bodySortedHeroes[i].SetLastDeltaPos(meet, 0);
                                    _bodySortedHeroes[i].AdjustBodyMoveBox();
                                }
                            }
                            else
                            {
                                if (_bodySortedHeroes[i].GetBodyMoveBox().MinX < _bodySortedHeroes[index].GetBodyMoveBox().MinX)
                                {
                                    int move = Mathf.Min((int)(meet * 0.5), _bodyMoveRuler);
                                    _bodySortedHeroes[i].GetBodyMoveBox().MoveX -= move;
                                    _bodySortedHeroes[index].GetBodyMoveBox().MoveX += move;
                                }
                                else
                                {
                                    int move = Mathf.Min((int)(meet * 0.5), _bodyMoveRuler);
                                    _bodySortedHeroes[index].GetBodyMoveBox().MoveX -= move;
                                    _bodySortedHeroes[i].GetBodyMoveBox().MoveX += move;
                                }
                            }

                            _meetCache.AddCache(_bodySortedHeroes[i], _bodySortedHeroes[index]);
                        }
                    }
                }
            }

            CalMove(_meetCache.CacheList);

            bool stillMove = false;
            for (int i = 0; i < _bodySortedHeroes.Count; i++)
            {
                if (_bodySortedHeroes[i].GetBodyMoveBox().MoveX != 0 || _bodySortedHeroes[i].GetBodyMoveBox().MoveY != 0)
                {
                    int x = 0;
                    if (_bodySortedHeroes[i].GetBodyMoveBox().MoveX != 0)
                    {
                        x = _bodySortedHeroes[i].GetBodyMoveBox().MoveX > 0 ? _bodyMoveRuler : -_bodyMoveRuler;
                        x = GetRoundZero(x, _bodySortedHeroes[i].GetBodyMoveBox().MoveX);
                    }

                    int y = 0;
                    if (_bodySortedHeroes[i].GetBodyMoveBox().MoveY != 0)
                    {
                        y = _bodySortedHeroes[i].GetBodyMoveBox().MoveY > 0 ? _bodyMoveRuler : -_bodyMoveRuler;
                        y = GetRoundZero(y, _bodySortedHeroes[i].GetBodyMoveBox().MoveY);
                    }

                    if (_meetCache.GetCacheInCache(_bodySortedHeroes[i]) == null)
                    {
                        XBoxRect extra = ExtraBoxMeet(XBoxExtraProperty.Wall, _bodySortedHeroes[i].GetBodyMoveBox());
                        if (extra == null || (extra != null && (_bodySortedHeroes[i].GetBodyMoveBox().MinX - extra.MinX) * _bodySortedHeroes[i].GetBodyMoveBox().MoveX > 0))
                        {
                            _bodySortedHeroes[i].SetLastDeltaPos(x, y);
                            _bodySortedHeroes[i].AdjustBodyMoveBox();
                        }
                    }
                    BodyMoveStep(_bodySortedHeroes[i], x, y);
                    stillMove = true;
                }
            }
            _meetCache.Clear();
            if (stillMove)
                threshold -= _bodyMoveRuler;
            else
                threshold = 0;
        }
    }

    private void BodyMoveStep(XBoxComponent hc, int x, int y)
    {
        if (Mathf.Abs(hc.GetBodyMoveBox().MoveX) >= Mathf.Abs(x))
            hc.GetBodyMoveBox().MoveX -= x;
        else
            hc.GetBodyMoveBox().MoveX = 0;

        if (Mathf.Abs(hc.GetBodyMoveBox().MoveY) >= Mathf.Abs(y))
            hc.GetBodyMoveBox().MoveY -= y;
        else
            hc.GetBodyMoveBox().MoveY = 0;
    }

    public void Update()
    {
        _bodySortedHeroes.Clear();

        //protect code, some exception will cause this UNCLREAR!
        _bulletHeroes.Clear();

        SortHeroesByPos();
        bool isMoreThanTwo = false;
        if (_bodySortedHeroes.Count > 2)
        {
            DealBodyMove();

            isMoreThanTwo = true;
            for (int i = 0; i < _heroes.Count; i++)
            {
                _heroes[i].ForceBodyMoveCallback(-1, null);
            }
        }


        for (int i = 0; i < _heroes.Count; i++)
        {
            for (int index = 0; index < _heroes.Count; index++)
            {
                if (i == index)
                    continue;

                if (!isMoreThanTwo)
                {
                    if (index > i)
                    {
                        if (!_heroes[i].GetBoxesFlag(XBoxComponent.XBoxFlag.BodyBox) || !_heroes[index].GetBoxesFlag(XBoxComponent.XBoxFlag.BodyBox))
                        {
                            if (!_heroes[i].GetBoxesFlag(XBoxComponent.XBoxFlag.BodyBox) && !_bodyAwayHeroes.Contains(_heroes[i]))
                                _bodyAwayHeroes.Add(_heroes[i]);
                            if (!_heroes[index].GetBoxesFlag(XBoxComponent.XBoxFlag.BodyBox) && !_bodyAwayHeroes.Contains(_heroes[index]))
                                _bodyAwayHeroes.Add(_heroes[index]);
                        }
                        else
                        {
                            _heroes[i].SetLastDeltaPos(0, _heroes[i].GetBodyMoveBox().MoveY);
                            _heroes[index].SetLastDeltaPos(0, _heroes[index].GetBodyMoveBox().MoveY);
                            int maxMove = 500;
                            int meetLen = -1;
                            while (maxMove > 0)
                            {
                                if (_heroes[i].GetBodyMoveBox().MoveX != 0)
                                {
                                    int move = 0;
                                    move = GetRoundZero(_heroes[i].GetBodyMoveBox().MoveX, _heroes[i].GetBodyMoveBox().MoveX > 0 ? _twoBodyMoveRuler : -_twoBodyMoveRuler);
                                    _heroes[i].GetBodyMoveBox().MoveX -= move;
                                    _heroes[i].SetLastDeltaPos(move, 0);

                                    _heroes[i].AdjustBodyMoveBox();
                                }

                                if (_heroes[index].GetBodyMoveBox().MoveX != 0)
                                {
                                    int move = 0;
                                    move = GetRoundZero(_heroes[index].GetBodyMoveBox().MoveX, _heroes[index].GetBodyMoveBox().MoveX > 0 ? _twoBodyMoveRuler : -_twoBodyMoveRuler);
                                    _heroes[index].GetBodyMoveBox().MoveX -= move;
                                    _heroes[index].SetLastDeltaPos(move, 0);

                                    _heroes[index].AdjustBodyMoveBox();
                                }

                                int meetL = LastBodyMeet(_heroes[i], _heroes[index]);
                                meetLen = Mathf.Max(meetL, meetLen);
                                _heroes[i].LastBodyMove(meetL, _heroes[index]);
                                _heroes[index].LastBodyMove(meetL, _heroes[i]);

                                _heroes[i].AdjustBodyMoveBox();
                                _heroes[index].AdjustBodyMoveBox();

                                maxMove -= _twoBodyMoveRuler;
                            }
       
                            _heroes[i].ForceBodyMoveCallback(meetLen, _heroes[index]);
                            _heroes[index].ForceBodyMoveCallback(meetLen, _heroes[i]);
                        }
                    }
                }
            }
        }

        for (int i = 0; i < _bodyAwayHeroes.Count; i++)
        {
            _bodyAwayHeroes[i].ForceBodyMoveCallback(-1, _bodyAwayHeroes[i]);
        }
        _bodyAwayHeroes.Clear();
        UpdateByAnimator();
    }

    private bool IsWarningAttack(XBoxComponent cruel, XBoxComponent hard)
    {
        XBoxRect attackBox = cruel.GetAttackWarningBoxRect();
        return _InternalWarningAttack(attackBox, hard);
    }

    private bool IsWarningAttack(XBulletComponent bc, XBoxComponent hard)
    {
        XBoxRect box = bc.GetBulletWarningBox();
        if (box == null)
            return false;

        return _InternalWarningAttack(box, hard);
    }

    private bool _InternalWarningAttack(XBoxRect rectBox, XBoxComponent hard)
    {
        List<XBoxRect> receiveBoxes = hard.GetReceiveDamageBoxesRect();

        if (rectBox == null || receiveBoxes == null)
            return false;

        for (int i = 0; i < receiveBoxes.Count; i++)
        {
            XBoxRect box = XBoxRect.Overlap(rectBox, receiveBoxes[i]);
            if (box.Width >= 0)
            {
                if (!_receiveWarningHeroes.Contains(hard))
                    _receiveWarningHeroes.Add(hard);

                return true;
            }
        }

        return false;
    }

    private bool IsBloodyAttack(XBoxComponent cruel, XBoxComponent hard)
    {
        XBoxRect attackBox = cruel.GetAttackBoxRect();
        List<XBoxRect> receiveBoxes = hard.GetReceiveDamageBoxesRect();

        if (attackBox == null || receiveBoxes == null)
            return false;

        XBoxRect box = new XBoxRect();
        for (int i = 0; i < receiveBoxes.Count; i++)
        {
            XBoxRect lapBox = XBoxRect.Overlap(attackBox, receiveBoxes[i]);
            if (lapBox.Width >= 0)
            {
                if (box.Height < lapBox.Height)
                {
                    box.Copy(lapBox);
                }
            }
        }
        if (box.Width >= 0)
        {
            AddBloodyHeroes(cruel, hard, box);
            return true;
        }

        return false;
    }

    private void AddBloodyHeroes(XBoxComponent attack, XBoxComponent receive, XBoxRect overlap)
    {
        AddBloodyHeroes(attack, receive.cid, overlap);
    }

    private void AddBloodyHeroes(XBoxComponent attack, int receiveId, XBoxRect overlap)
    {
        XBoxBloody hero = _bloodyHeroes.Find(delegate (XBoxBloody obj)
        {
            return obj.AttackHero == attack;
        });
        if (hero == null)
        {
            hero = new XBoxBloody(attack);
            _bloodyHeroes.Add(hero);
        }
        hero.AddBloodyHero(receiveId, overlap);
    }

    private void AddBloodyFlexiableBoxes(XBoxComponent attack)
    {
        XBoxRect attackBox = attack.GetAttackBoxRect();
        if (attackBox == null || _flexibleHurtBoxes.Count < 1)
            return;

        for (int i = 0; i < _flexibleHurtBoxes.Count; i++)
        {
            XBoxRect box = XBoxRect.Overlap(attackBox, _flexibleHurtBoxes[i].GetFlexibleHurtBox());
            if (box.Width >= 0)
            {
                AddBloodyHeroes(attack, _flexibleHurtBoxes[i].GetActiveId(), box);
            }
        }
    }

    private void HugEach(XBoxComponent human, XBoxComponent orc)
    {
        XBoxHugRect humanBox = human.GetHugBoxRect();
        XBoxHugRect orcBox = orc.GetHugBoxRect();
        if (humanBox == null || orcBox == null)
            return;

        XBoxRect overlap = XBoxRect.Overlap(humanBox, orcBox);

        if (overlap != null && overlap.Width >= 0)
        {
            if (humanBox.HugType == orcBox.HugType || (humanBox.HugType != XHugType.Normal && orcBox.HugType != XHugType.Normal))
            {
                if (!_hugEachList.Contains(human))
                    _hugEachList.Add(human);
                if (!_hugEachList.Contains(orc))
                    _hugEachList.Add(orc);
            }
            else
            {
                XBoxComponent hc = humanBox.HugType > orcBox.HugType ? orc : human;
                if (!_hugBeEatenList.Contains(hc))
                    _hugBeEatenList.Add(hc);
            }
        }
    }

    private void HugYou(XBoxComponent me, XBoxComponent you)
    {
        if (_hugBeEatenList.Contains(me))
            return;

        XBoxRect hugBox = me.GetHugBoxRect();
        if (hugBox == null)
            return;
        List<XBoxRect> hurtBoxes = you.GetReceiveDamageBoxesRect();
        if (hurtBoxes == null)
            return;

        for (int i = 0; i < hurtBoxes.Count; i++)
        {
            XBoxRect overlap = XBoxRect.Overlap(hugBox, hurtBoxes[i]);
            if (overlap != null && overlap.Width >= 0)
            {
                if ((_hugHeroesDic.ContainsKey(me) && overlap.Width < _hugHeroesDic[me].Overlap) || !_hugHeroesDic.ContainsKey(me))
                {
                    XBoxOverlap lap = new XBoxOverlap();
                    lap.Hero = you;
                    lap.Overlap = overlap.Width;
                    _hugHeroesDic[me] = lap;
                }
                return;
            }
        }
    }

    private void DealHugEvent()
    {
        for (int i = 0; i < _hugEachList.Count; i++)
        {
            if (_hugHeroesDic.ContainsKey(_hugEachList[i]))
            {
                _hugHeroesDic.Remove(_hugEachList[i]);
            }
            _hugEachList[i].HugCallback(1, _hugEachList[i], null);
        }

        for (int i = 0; i < _hugBeEatenList.Count; i++)
        {
            _hugBeEatenList[i].HugCallback(0, null, null);
        }

        foreach (KeyValuePair<XBoxComponent, XBoxOverlap> pair in _hugHeroesDic)
        {
            pair.Key.HugCallback(2, pair.Key, pair.Value.Hero);
        }

        _hugEachList.Clear();
        _hugBeEatenList.Clear();
        _hugHeroesDic.Clear();
    }

    private bool AttackOnBullet(XBoxComponent hero)
    {
        XBoxRect attackBox = hero.GetAttackBoxRect();

        if (attackBox == null)
            return false;

        if (_bullets.Count > 0)
        {
            for (int i = 0; i < _bullets.Count; i++)
            {
                XBoxRect lapBox = XBoxRect.Overlap(attackBox, _bullets[i].GetBulletHitBox());
                if (lapBox.Width >= 0)
                {
                    AddBloodyHeroes(hero, _bullets[i].gid, lapBox);
                    return true;
                }
            }
        }
        return false;
    }

    private void AddBulletHero(XBulletComponent bullet, Transform trans)
    {
        XBulletHero bh = _bulletHeroes.Find(delegate (XBulletHero obj)
        {
            return obj.Bullet == bullet;
        });

        if (bh == null)
        {
            bh = new XBulletHero();
            bh.Bullet = bullet;
            _bulletHeroes.Add(bh);
        }
        bh.TransList.Add(trans); ;
    }

    private void CalBulletHit(XBoxComponent hero)
    {
        if (_bullets.Count > 0)
        {
            bool isHeroWarning = false;
            for (int i = 0; i < _bullets.Count; i++)
            {
                bool addI = false;
                List<XBoxRect> boxes = hero.GetReceiveDamageBoxesRect();
                for (int hurtBoxIndex = 0; hurtBoxIndex < boxes.Count; hurtBoxIndex++)
                {
                    if (XBoxRect.Overlap(boxes[hurtBoxIndex], _bullets[i].GetBulletHitBox()).Width >= 0)
                    {
                        AddBulletHero(_bullets[i], hero.Trans);
                        addI = true;
                        break;
                    }
                }

                if (!isHeroWarning)
                    isHeroWarning = IsWarningAttack(_bullets[i], hero);
                if (addI)
                    continue;

                for (int flexiHurtIndex = 0; flexiHurtIndex < _flexibleHurtBoxes.Count; flexiHurtIndex++)
                {
                    if (XBoxRect.Overlap(_flexibleHurtBoxes[flexiHurtIndex].GetFlexibleHurtBox(), _bullets[i].GetBulletHitBox()).Width >= 0)
                    {
                        AddBulletHero(_bullets[i], _flexibleHurtBoxes[flexiHurtIndex].Trans);
                        addI = true;
                        break;
                    }
                }

                if (addI)
                    continue;

                for (int index = 0; index < _bullets.Count; index++)
                {
                    if (index != i)
                    {
                        if (XBoxRect.Overlap(_bullets[i].GetBulletHitBox(), _bullets[index].GetBulletHitBox()).Width >= 0)
                        {
                            AddBulletHero(_bullets[i], _bullets[index].GetFlyEffectTrans());
                        }
                    }
                }
            }
        }
    }

    public void CheckAttackWhenBoxesCome(XBoxComponent hc)
    {
        for (int i = 0; i < _heroes.Count; i++)
        {
            if (_heroes[i] == hc)
                continue;

            IsBloodyAttack(hc, _heroes[i]);
        }
        AddBloodyFlexiableBoxes(hc);

        DealAttackEvent();
    }

    private void DealAttackEvent()
    {
        for (int i = _bloodyHeroes.Count - 1; i >= 0; i--)
        {
            if (_bloodyHeroes[i].BloodyHeroes != null && _bloodyHeroes[i].BloodyHeroes.Count > 0)
            {
                _bloodyHeroes[i].AttackHero.AttackEventCallback(_bloodyHeroes[i].BloodyHeroes.ToArray(), _bloodyHeroes[i].OverlapBoxes.ToArray());
            }
            _bloodyHeroes[i].Clear();
        }
    }

    private void DealAttackWarningEvent()
    {
        _receiveWarningHeroes.Sort(delegate (XBoxComponent x, XBoxComponent y) {
            return x.cid - y.cid;
        });
        for (int i = 0; i < _receiveWarningHeroes.Count; i++)
        {
            XBoxComponent hero = _receiveWarningHeroes[i];
            if (hero != null && !_lastReceiveWarningHeroes.Contains(hero))
                hero.AttackWarningEventCallback(true);
        }

        for (int i = 0; i < _lastReceiveWarningHeroes.Count; i++)
        {
            XBoxComponent hero = _lastReceiveWarningHeroes[i];
            if (hero != null && !_receiveWarningHeroes.Contains(hero))
                hero.AttackWarningEventCallback(false);
        }

        if (_receiveWarningHeroes.Count > 0)
        {
            _lastReceiveWarningHeroes = _receiveWarningHeroes;
            _receiveWarningHeroes = new List<XBoxComponent>();
        }
        else
        {
            _lastReceiveWarningHeroes.Clear();
        }
    }

    private void DealBulletHit()
    {
        for (int i = 0; i < _bullets.Count; i++)
        {
            XBulletHero bh = _bulletHeroes.Find(delegate (XBulletHero obj) {
                return obj.Bullet == _bullets[i];
            });
            if (bh == null)
                _bullets[i].HitRunOut();
        }

        for (int i = 0; i < _bulletHeroes.Count; i++)
        {
            if (!_bulletHeroes[i].Bullet.CallHitDetection(_bulletHeroes[i].TransList))
            {
                _bulletHeroes[i].Bullet.HitRunOut();
            }

            _bulletHeroes[i].Clear();
            _bulletHeroes[i] = null;
        }
        _bulletHeroes.Clear();
    }

    public void UpdateByAnimator()
    {
        for (int i = 0; i < _heroes.Count; i++)
        {
            for (int index = 0; index < _heroes.Count; index++)
            {
                if (i == index)
                    continue;
                IsWarningAttack(_heroes[i], _heroes[index]);
                IsBloodyAttack(_heroes[i], _heroes[index]);

                HugEach(_heroes[i], _heroes[index]);
                HugYou(_heroes[i], _heroes[index]);
            }

            if (_isTrial)
            {
                if (!AttackOnBullet(_heroes[i]))
                    CalBulletHit(_heroes[i]);
            }
            else
            {
                CalBulletHit(_heroes[i]);
            }

            //gather attacks on flexiable box
            AddBloodyFlexiableBoxes(_heroes[i]);
        }
        // gather complete, deal attack warning event in on frame
        DealAttackWarningEvent();

        // gather complete, deal attack event in one frame
        DealAttackEvent();

        DealHugEvent();
        DealBulletHit();
    }

    private int LastBodyMeet(XBoxComponent hc1, XBoxComponent hc2)
    {
        XBodyMoveRectBox rb1 = hc1.GetBodyMoveBox();
        XBodyMoveRectBox rb2 = hc2.GetBodyMoveBox();

        if (rb1 == null || rb2 == null)
            return -1;

        XBoxRect box = XBoxRect.Overlap(rb1, rb2);
        return box.Width;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="isTrial"></param>
    public void SetTrialMode(bool isTrial)
    {
        _isTrial = isTrial;
    }

    public void RegisterBullet(XBulletComponent bullet)
    {
        if (_bullets.Contains(bullet))
        {
            return;
        }

        _bullets.Add(bullet);
    }


    public void UnRegisterBullet(XBulletComponent bullet)
    {
        if (!_bullets.Contains(bullet))
        {
            return;
        }

        _bullets.Remove(bullet);
    }

    public void RegisterFlexibleHurtBox(XBoxFlexibleHurt hurtBox)
    {
        if (_flexibleHurtBoxes.Contains(hurtBox))
        {
            return;
        }

        _flexibleHurtBoxes.Add(hurtBox);
    }

    public void UnRegisterFlexibleHurtBox(XBoxFlexibleHurt hurtBox)
    {
        if (!_flexibleHurtBoxes.Contains(hurtBox))
        {
            return;
        }

        _flexibleHurtBoxes.Remove(hurtBox);
    }

    public void RegisterExtraBox(XBoxExtraRect box)
    {
        if (!_extraBoxesDic.ContainsKey(box.Property))
        {
            _extraBoxesDic[box.Property] = new List<XBoxExtraRect>();
        }
        _extraBoxesDic[box.Property].Add(box);
    }

    public void UnRegisterExtraBox(XBoxExtraRect box)
    {
        if (_extraBoxesDic.ContainsKey(box.Property))
        {
            _extraBoxesDic[box.Property].Remove(box);
            if (_extraBoxesDic[box.Property].Count == 0)
                _extraBoxesDic.Remove(box.Property);
        }
    }

    public void Register(XBoxComponent hero)
    {
        if (hero == null)
        {
            GLog.LogError("REGISTER a null hero??");
            return;
        }

        if (_heroes.Contains(hero))
        {
            GLog.LogError("REGISTER a duplicate hero?? {0}", hero.gameObject.name);
            return;
        }
        _heroes.Add(hero);
    }

    public void UnRegister(XBoxComponent hero)
    {
        if (hero == null)
        {
            GLog.Log("UNREGISTER a null hero??");
            return;
        }

        if (_heroes.Contains(hero))
            _heroes.Remove(hero);
        else
            GLog.Log("UNREGISTER a ghost hero?? {0}", hero.gameObject.name);

        if (_bloodyHeroes != null)
        {
            XBoxBloody bloodyHero = _bloodyHeroes.Find(delegate (XBoxBloody obj) {
                return obj.AttackHero == hero;
            });
            GLog.Log("Unregister bloodyHeroes {0}", _bloodyHeroes.Count);
            if (bloodyHero != null)
                _bloodyHeroes.Remove(bloodyHero);
        }
    }

    public void UpdateOnlyTransPosition()
    {
        if (_heroes != null)
        {
            int heroCount = 0;
            for (int i = 0; i < _heroes.Count; i++)
            {
                if (_heroes[i].GetBoxesFlag(XBoxComponent.XBoxFlag.BodyBox))
                    heroCount++;
            }

            if (heroCount <= 2)
            {
                List<XBoxComponent> transAwayHeroes = new List<XBoxComponent>();
                for (int i = 0; i < _heroes.Count; i++)
                {
                    for (int index = 0; index < _heroes.Count; index++)
                    {
                        if (i == index)
                            continue;
                        if (index > i)
                        {
                            if (!_heroes[i].GetBoxesFlag(XBoxComponent.XBoxFlag.BodyBox) || !_heroes[index].GetBoxesFlag(XBoxComponent.XBoxFlag.BodyBox))
                            {
                                if (!_heroes[i].GetBoxesFlag(XBoxComponent.XBoxFlag.BodyBox) && !transAwayHeroes.Contains(_heroes[i]))
                                    transAwayHeroes.Add(_heroes[i]);
                                if (!_heroes[index].GetBoxesFlag(XBoxComponent.XBoxFlag.BodyBox) && !transAwayHeroes.Contains(_heroes[index]))
                                    transAwayHeroes.Add(_heroes[index]);
                            }
                            else
                            {
                                _heroes[i].SetLastTransDeltaPos(0, _heroes[i].GetTransMoveBox().MoveY);
                                _heroes[index].SetLastTransDeltaPos(0, _heroes[index].GetTransMoveBox().MoveY);
                                int maxMove = 500;
                                int meetLen = -1;
                                while (maxMove > 0)
                                {
                                    if (_heroes[i].GetTransMoveBox().MoveX != 0)
                                    {
                                        int move = 0;
                                        move = GetRoundZero(_heroes[i].GetTransMoveBox().MoveX, _heroes[i].GetTransMoveBox().MoveX > 0 ? _twoBodyMoveRuler : -_twoBodyMoveRuler);
                                        _heroes[i].GetTransMoveBox().MoveX -= move;
                                        _heroes[i].SetLastTransDeltaPos(move, 0);

                                        _heroes[i].AdjustTransMoveBox();
                                    }

                                    if (_heroes[index].GetTransMoveBox().MoveX != 0)
                                    {
                                        int move = 0;
                                        move = GetRoundZero(_heroes[index].GetTransMoveBox().MoveX, _heroes[index].GetTransMoveBox().MoveX > 0 ? _twoBodyMoveRuler : -_twoBodyMoveRuler);
                                        _heroes[index].GetTransMoveBox().MoveX -= move;
                                        _heroes[index].SetLastTransDeltaPos(move, 0);

                                        _heroes[index].AdjustTransMoveBox();
                                    }

                                    int meetL = LastTransMeet(_heroes[i], _heroes[index]);
                                    meetLen = Mathf.Max(meetL, meetLen);
                                    _heroes[i].LastTransMove(meetL, _heroes[index]);
                                    _heroes[index].LastTransMove(meetL, _heroes[i]);

                                    _heroes[i].AdjustTransMoveBox();
                                    _heroes[index].AdjustTransMoveBox();

                                    maxMove -= _twoBodyMoveRuler;
                                }
 
                                _heroes[i].ForceTransMoveCallback(meetLen, _heroes[index]);
                                _heroes[index].ForceTransMoveCallback(meetLen, _heroes[i]);
                            }
                        }
                    }
                }

                for (int i = 0; i < transAwayHeroes.Count; i++)
                {
                    transAwayHeroes[i].ForceTransMoveCallback(-1, transAwayHeroes[i]);
                }
                transAwayHeroes.Clear();
            }
        }
    }

    private int LastTransMeet(XBoxComponent hc1, XBoxComponent hc2)
    {
        XBodyMoveRectBox rb1 = hc1.GetTransMoveBox();
        XBodyMoveRectBox rb2 = hc2.GetTransMoveBox();

        if (rb1 == null || rb2 == null)
            return -1;

        XBoxRect box = XBoxRect.Overlap(rb1, rb2);

        return box.Width;
    }
}
