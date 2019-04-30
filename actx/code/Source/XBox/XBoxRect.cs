using UnityEngine;

/// <summary>
/// 
/// </summary>
public class XBoxRect {
    public int MinX;
    public int MinY;
    public int Width;
    public int Height;

    /// <summary>
    /// 
    /// </summary>
    public XBoxRect()
    {
        MinX = 0;
        MinY = 0;
        Width = -1;
        Height = -1;
    }

    /// <summary>
    /// 
    /// </summary>
    private static XBoxRect _rectBoxOverlap = null;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return string.Format("[XBoxRect] {0}  {1}  {2}  {3}", (MinX / 100f).ToString(), (MinY / 100f).ToString(), (Width / 100f).ToString(), (Height / 100f).ToString());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="box"></param>
    /// <returns></returns>
    public int Overlap(XBoxRect box)
    {
        // pretend to create a box, if the box exit, overlap!
        int minX = Mathf.Max(this.MinX, box.MinX);
        int minY = Mathf.Max(this.MinY, box.MinY);
        int maxX = Mathf.Min(this.MinX + Width, box.MinX + box.Width);
        int maxY = Mathf.Min(this.MinY + Height, box.MinY + box.Height);

        if (minX > maxX || minY > maxY)
        {
            return -1;
        }
        else
        {
            if (minX >= maxX)
                return 0;

            return maxX - minX;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool IsEmpty()
    {
        return Width == 0 || Height == 0;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Empty()
    {
        Width = 0;
        Height = 0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="box"></param>
    public void Copy(XBoxRect box)
    {
        this.MinX = box.MinX;
        this.MinY = box.MinY;
        this.Width = box.Width;
        this.Height = box.Height;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="box1"></param>
    /// <param name="box2"></param>
    /// <returns></returns>
    public static XBoxRect Overlap(XBoxRect box1, XBoxRect box2)
    {
        if (_rectBoxOverlap == null)
            _rectBoxOverlap = new XBoxRect();

        if (box1 != null && box2 != null)
        {
            // pretend to create a box, if the box exit, overlap!
            int minX = Mathf.Max(box1.MinX, box2.MinX);
            int minY = Mathf.Max(box1.MinY, box2.MinY);
            int maxX = Mathf.Min(box1.MinX + box1.Width, box2.MinX + box2.Width);
            int maxY = Mathf.Min(box1.MinY + box1.Height, box2.MinY + box2.Height);

            if (minX > maxX || minY > maxY)
            {
                _rectBoxOverlap.Height = -1;
                _rectBoxOverlap.Width = -1;
            }
            else
            {
                _rectBoxOverlap.Height = maxY - minY;
                _rectBoxOverlap.Width = maxX - minX;
                _rectBoxOverlap.MinX = minX;
                _rectBoxOverlap.MinY = minY;
            }
        }
        else
        {
            _rectBoxOverlap.Width = -1;
        }

        return _rectBoxOverlap;
    }
}

/// <summary>
/// 
/// </summary>
public class XBodyMoveRectBox : XBoxRect
{
    public int MoveX;
    public int MoveY;
}

/// <summary>
/// 
/// </summary>
public enum XBoxExtraProperty
{
    Wall
}

/// <summary>
/// 
/// </summary>
public class XBoxExtraRect : XBoxRect
{
    public XBoxExtraProperty Property;
}

/// <summary>
/// 
/// </summary>
public class XBoxHugRect : XBoxRect
{
    public XHugType HugType;
}
