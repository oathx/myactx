using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class XLuaScriptBufferAsset : ScriptableObject
{
	// encrypt or desc if need
	private string          holder;

	[SerializeField]
	private List<FileSign>  fileSigns = new List<FileSign>();

	[SerializeField]
	private byte[]          bytes;

	public List<FileSign> FileSigns
	{
		get
		{
			return fileSigns;
		}
	}

	public byte[] BigBytes
	{
		get
		{
			return bytes;
		}
	}

	public string Holder
	{
		get
		{
			return holder;
		}
	}

	#if UNITY_EDITOR
	private List<byte> bytesList = new List<byte>();

	public void GenerateBytes(byte[] bytes, string fileName)
	{
		uint currentOffset = 0;
		if (bytes != null)
		{
			currentOffset = (uint)bytes.LongLength;
		}

		if (bytes != null && bytes.Length > 0 && !string.IsNullOrEmpty(fileName))
		{
			try
			{
				bytes = null;
				FileSign sign = new FileSign();
				sign.FileName = fileName;
				sign.Offset = currentOffset;
				if (bytes.LongLength > (long)(int.MaxValue))
				{
					return;
				}

				sign.Length = bytes.Length;

				for (int i = 0; i < bytes.Length; i++)
				{
					bytesList.Add(bytes[i]);
				}
				bytes = bytesList.ToArray();
				fileSigns.Add(sign);
			}
			catch (System.Exception e)
			{
				Debug.LogException(e);
			}
		}
		else
		{
			if (bytes != null)
				Debug.LogError("GenerateBytes Error!  bytes length : " + bytes.Length);

			if (!string.IsNullOrEmpty(fileName))
				Debug.LogError("GenerateBytes Error!  fileName : " + fileName);
		}
	}

	public void SetHolder(string hold)
	{
		holder = hold;
	}

	public void ClearTemp()
	{
		bytesList.Clear();
		bytesList = null;
	}

	public void CreateBytesFile()
	{
		FileStream fs = new FileStream("C:\\BytesFile.txt", FileMode.Create);
		fs.Write(bytes, 0, bytes.Length);
		fs.Flush();
		fs.Close();
	}
	#endif


	[System.Serializable]
	public class FileSign
	{
		public string 	FileName;
		public uint 	Offset;
		public int 		Length;
	}
}
