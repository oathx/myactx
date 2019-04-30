using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using SLua;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text;

/// <summary>
/// X utility.
/// </summary>
[CustomLuaClassAttribute]
public static class XUtility {
	static System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
	static XCrc32 crc32 = new XCrc32();

    /// <summary>
    /// Determines if hash string the specified str.
    /// </summary>
    /// <returns><c>true</c> if hash string the specified str; otherwise, <c>false</c>.</returns>
    /// <param name="str">String.</param>
    public static int HashString( string str ) {
		const uint InitialFNV = 2166136261U;
		const uint FNVMultiple = 16777619;

		uint hash = InitialFNV;
		for( int i = 0; i < str.Length; i++ )
		{
			hash = hash ^ str[i];
			hash = hash * FNVMultiple;
		}

		return (int)(hash&0x7FFFFFFF);
	}

	/// <summary>
	/// Md5s the sum.
	/// </summary>
	/// <returns>The sum.</returns>
	/// <param name="input">Input.</param>
	public static string Md5Sum(string input) {
		return Md5Sum(System.Text.Encoding.ASCII.GetBytes(input));
	}

	/// <summary>
	/// Md5s the sum.
	/// </summary>
	/// <returns>The sum.</returns>
	/// <param name="bytes">Bytes.</param>
	public static string Md5Sum(byte[] bytes) {
		byte[] hash = md5.ComputeHash(bytes);
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		for (int i = 0; i < hash.Length; i++)
		{
			sb.Append(hash[i].ToString("x2"));
		} 
		return sb.ToString(); 
	}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
	public static uint CrcHash(string input) {
		return CrcHash(System.Text.Encoding.ASCII.GetBytes(input));
	}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
	[DoNotToLua]
	public static uint CrcHash(byte[] bytes) {
		return XCrc32.Compute(bytes);
	}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
	public static uint ComputeFileChecksum( string path ) {
		FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
		crc32.ComputeHash(fs);
		fs.Close();
		return crc32.GetHashResult();
	}

	/// <summary>
	/// Times the stamp to date time.
	/// </summary>
	/// <returns>The stamp to date time.</returns>
	/// <param name="timeStamp">Time stamp.</param>
	public static DateTime TimeStampToDateTime(int timeStamp){
		DateTime dateTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
		dateTime = dateTime.AddSeconds(timeStamp);
		return dateTime;
	}

	/// <summary>
	/// Dates the time to time stamp.
	/// </summary>
	/// <returns>The time to time stamp.</returns>
	/// <param name="dateTime">Date time.</param>
	public static int DateTimeToTimeStamp(DateTime dateTime){
		DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
		return (int)(dateTime - startTime).TotalSeconds;
	}

	/// <summary>
	/// Gets the natural days count.
	/// </summary>
	/// <returns>The natural days count.</returns>
	/// <param name="dtStart">Dt start.</param>
	/// <param name="dtEnd">Dt end.</param>
	public static int GetNaturalDaysCount(DateTime dtStart, DateTime dtEnd){
		TimeSpan tsStart = new TimeSpan(dtStart.Ticks);
		TimeSpan tsEnd = new TimeSpan(dtEnd.Ticks);
		return tsEnd.Days - tsStart.Days;
	}

	/// <summary>
	/// Gets the hours count.
	/// </summary>
	/// <returns>The hours count.</returns>
	/// <param name="dtStart">Dt start.</param>
	/// <param name="dtEnd">Dt end.</param>
	public static int GetHoursCount(DateTime dtStart, DateTime dtEnd) {
		TimeSpan tsStart = new TimeSpan(dtStart.Ticks);
		TimeSpan tsEnd = new TimeSpan(dtEnd.Ticks);
		TimeSpan n = tsEnd - tsStart;
		return n.Hours + n.Days * 24;
	}

	/// <summary>
	/// The server time offset.
	/// </summary>
	public static int serverTimeOffset = 0;

    /// <summary>
    /// 
    /// </summary>
	public static int ServerTime {
		get {
			return DateTimeToTimeStamp(DateTime.Now)+serverTimeOffset;
		}
	}

	/// <summary>
	/// Gets the server date time.
	/// </summary>
	/// <value>The server date time.</value>
	public static DateTime ServerDateTime {
		get {
			return TimeStampToDateTime(ServerTime);
		}
	}

	/// <summary>
	/// Syncs the time from server.
	/// </summary>
	/// <param name="serverTime">Server time.</param>
	public static void SyncTimeFromServer( int serverTime ) {
		serverTimeOffset = serverTime - DateTimeToTimeStamp(DateTime.Now);
	}

	/// <summary>
	/// Nos the cache URL.
	/// </summary>
	/// <returns>The cache URL.</returns>
	/// <param name="url">URL.</param>
	public static string NoCacheUrl( string url ) {
		return string.Format("{0}?r={1}", url, ServerTime);
	}

	/// <summary>
	/// Uns the zip file.
	/// </summary>
	/// <param name="bytes">Bytes.</param>
	/// <param name="outPath">Out path.</param>
	public static void UnZipFile(byte[] bytes, string outPath)
	{
		using (var memStream = new MemoryStream(bytes))
		{
			UnZipFile(memStream, outPath);
		}
	}

	/// <summary>
	/// Uns the zip file.
	/// </summary>
	/// <param name="stream">Stream.</param>
	/// <param name="outPath">Out path.</param>
	private static void UnZipFile(Stream stream, string outPath)
	{
		if (!Directory.Exists(outPath))
		{
			Directory.CreateDirectory(outPath);
		}

		using (var zipStream = new ZipInputStream(stream))
		{
			ZipEntry theEntry;
			while ((theEntry = zipStream.GetNextEntry()) != null)
			{
				string dirName = Path.GetDirectoryName(theEntry.Name);
				string fileName = Path.GetFileName(theEntry.Name);

				if (!string.IsNullOrEmpty(dirName))
				{
					Directory.CreateDirectory(outPath + dirName);
				}

				if (!string.IsNullOrEmpty(fileName))
				{
					using (var streamWriter = File.Create(outPath + theEntry.Name))
					{
						int size = 2048;
						var data = new byte[size];
						while (true)
						{
							size = zipStream.Read(data, 0, data.Length);
							if (size > 0)
							{
								streamWriter.Write(data, 0, size);
							}
							else
							{
								break;
							}
						}
					}
				}
			}
		}
	}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entryName"></param>
    /// <param name="bytes"></param>
    /// <param name="outBytes"></param>
    /// <returns></returns>
    [DoNotToLua]
    public static int CompressBytes(String entryName, byte[] bytes, out byte[] outBytes)
    {
        using (MemoryStream zipStream = new MemoryStream())
        {
            using (ZipOutputStream outStream = new ZipOutputStream(zipStream))
            {
                ZipEntry entry = new ZipEntry(entryName);
                entry.DateTime = new DateTime(0);

                outStream.PutNextEntry(entry);
                outStream.Write(bytes, 0, bytes.Length);
                outStream.Finish();

                zipStream.Seek(0, SeekOrigin.Begin);

                int compressLength = (int)zipStream.Length;
                outBytes = new byte[compressLength];

                int readLength = zipStream.Read(outBytes, 0, compressLength);

                outStream.Close();
                zipStream.Close();

                return readLength;
            }
        }
    }

    public static bool IsLowMemoryDevice()
    {
        return SystemInfo.systemMemorySize < 1100;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string EncodeNonAsciiCharacters(string value)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (char c in value)
        {
            if (c > 127)
            {
                string encodedValue = "\\u" + ((int)c).ToString("x4");
                sb.Append(encodedValue);
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public static XMoveCurve ToMoveCurve(AnimationEvent e)
    {
        return e.objectReferenceParameter as XMoveCurve;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public static XBoxAttackCheckPointObject ToHeroAttackCheckPoint(AnimationEvent e)
    {
        XBoxAttackCheckPointObject obj = e.objectReferenceParameter as XBoxAttackCheckPointObject;
        if (obj != null)
        {
            obj.attackName = e.stringParameter.Split('#')[0];
            obj.dmgCurrent = Mathf.FloorToInt(e.floatParameter);
            obj.dmgTotal = e.intParameter;
            if (obj.dmgTotal > 1)
                obj.dmgRate = Mathf.RoundToInt((e.floatParameter - obj.dmgCurrent) * 100);
            else
                obj.dmgRate = 0;

            obj.simulation = false;
            obj.aniClip = e.animatorClipInfo.clip;
            obj.stringParam = e.stringParameter;
        }

        return obj;
    }
}
