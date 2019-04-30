using System;
using System.Net;
using SLua;

[CustomLuaClass]
public class PacketType
{
	public const int SOCKET_CONNECT_FAILURE = 1;
	public const int SOCKET_CONNECT_SUCCESS = 2;
	public const int SOCKET_DISCONNECT		= 3;
}

/// <summary>
/// Net packet.
/// </summary>
public class INetPacket
{
	public const int BufferSize = 4096;
	protected int size = 0;
	protected int type = 0;
	protected bool storing = false;
	protected byte[] data = null;
	protected int offset = 0;

	/// <summary>
	/// Initializes a new instance of the <see cref="INetPacket"/> class.
	/// </summary>
	public INetPacket() {
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="INetPacket"/> class.
	/// </summary>
	/// <param name="packetType">Packet type.</param>
	public INetPacket( int packetType ) {
		type = packetType;
		data = new byte[BufferSize];
		storing = true;
		offset = sizeof(int)*2; // skip size and type
		size = sizeof(int);
	}

	/// <summary>
	/// Gets or sets the size.
	/// </summary>
	/// <value>The size.</value>
	public int Size {           // type+data
		get { return size; }
		internal set { size = value; }
	}

	/// <summary>
	/// Gets or sets the offset.
	/// </summary>
	/// <value>The offset.</value>
	public int Offset {
		set { offset = value;}
		get { return offset;}
	}

	/// <summary>
	/// Gets or sets the type.
	/// </summary>
	/// <value>The type.</value>
	public int Type {
		get { return type; }
		internal set { type = value; }
	}

	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="INetPacket"/> is storing.
	/// </summary>
	/// <value><c>true</c> if storing; otherwise, <c>false</c>.</value>
	public bool Storing {
		get { return storing; }
		internal set { storing = value; }
	}

	/// <summary>
	/// Sets the type.
	/// </summary>
	/// <param name="newType">New type.</param>
	public void SetType( int newType ) {
		type = newType;
	}

	/// <summary>
	/// Gets the bytes.
	/// </summary>
	/// <returns>The bytes.</returns>
	public byte[] GetBytes() {
		return data;
	}

	/// <summary>
	/// Set the specified packetSize, buffer and start.
	/// </summary>
	/// <param name="packetSize">Packet size.</param>
	/// <param name="buffer">Buffer.</param>
	/// <param name="start">Start.</param>
	public void Set( int packetSize, byte[] buffer, int start ) {
		size = packetSize;
		type = BitConverter.ToInt32(buffer, start);

		data = new byte[size];
		Buffer.BlockCopy(buffer, start, data, 0, size);
		offset = sizeof(int);   //skip packet type
	}

	/// <summary>
	/// Reads the char.
	/// </summary>
	/// <returns>The char.</returns>
	public char ReadChar() {
		if( offset >= data.Length ) return '0';
		char value = (char)data[offset];
		offset++;
		return value;
	}

	/// <summary>
	/// Reads the byte.
	/// </summary>
	/// <returns>The byte.</returns>
	public byte ReadByte() {
		if( offset >= data.Length ) return 0;
		byte value = data[offset];
		offset++;
		return value;
	}

	/// <summary>
	/// Reads the short.
	/// </summary>
	/// <returns>The short.</returns>
	public short ReadShort() {
		if( offset >= data.Length ) return 0;
		short value = BitConverter.ToInt16(data, offset);
		offset += sizeof(short);
		return value;
	}

	/// <summary>
	/// Reads the U short.
	/// </summary>
	/// <returns>The U short.</returns>
	public ushort ReadUShort() {
		return (ushort)ReadShort();
	}

	/// <summary>
	/// Reads the int.
	/// </summary>
	/// <returns>The int.</returns>
	public int ReadInt() {
		if( offset >= data.Length ) return 0;
		int value = BitConverter.ToInt32(data, offset);
		offset += sizeof(int);
		return value;
	}

	/// <summary>
	/// Reads the U int.
	/// </summary>
	/// <returns>The U int.</returns>
	public uint ReadUInt() {
		return (uint)ReadInt();
	}

	/// <summary>
	/// Reads the string.
	/// </summary>
	/// <returns>The string.</returns>
	public string ReadString() {
		ushort len = ReadUShort();
		if( offset+len > data.Length ) return "";
		string str = System.Text.Encoding.UTF8.GetString(data, offset, len);
		offset += len;
		return str;
	}

	/// <summary>
	/// Reads the bytes.
	/// </summary>
	/// <returns>The bytes.</returns>
	/// <param name="len">Length.</param>
	public byte[] ReadBytes( int len ) {
		if( offset+len > data.Length ) return null;
		byte[] buffer = new byte[len];
		Buffer.BlockCopy(data, offset, buffer, 0, len);
		offset += len;
		return buffer;
	}

	/// <summary>
	/// Reads the block.
	/// </summary>
	/// <returns>The block.</returns>
	public byte[] ReadBlock() {
		if( offset >= data.Length ) return null;
		int len = ReadInt();
		return ReadBytes(len); 
	}

	/// <summary>
	/// Reads the float.
	/// </summary>
	/// <returns>The float.</returns>
	public float ReadFloat() {
		if( offset >= data.Length ) return .0f;
		float value = BitConverter.ToSingle(data, offset);
		offset += sizeof(float);
		return value;
	}

	/// <summary>
	/// Writes the char.
	/// </summary>
	/// <param name="value">Value.</param>
	public void WriteChar( char value ) {
		if(!storing) return;

		data[offset] = (byte)value;
		offset++;
		size++;
	}

	/// <summary>
	/// Writes the byte.
	/// </summary>
	/// <param name="value">Value.</param>
	public void WriteByte( byte value ) {
		if(!storing) return;

		data[offset] = value;
		offset++;
		size++;
	}

	/// <summary>
	/// Writes the short.
	/// </summary>
	/// <param name="value">Value.</param>
	public void WriteShort( short value ) {
		if(!storing) return;

		byte[] bytes = BitConverter.GetBytes(value);
		Buffer.BlockCopy(bytes, 0, data, offset, bytes.Length);
		offset += bytes.Length;
		size += bytes.Length;
	}

	/// <summary>
	/// Writes the U short.
	/// </summary>
	/// <param name="value">Value.</param>
	public void WriteUShort( ushort value ) {
		WriteShort((short)value);
	}

	/// <summary>
	/// Writes the int.
	/// </summary>
	/// <param name="value">Value.</param>
	public void WriteInt( int value ) {
		if(!storing) return;

		byte[] bytes = BitConverter.GetBytes(value);
		Buffer.BlockCopy(bytes, 0, data, offset, bytes.Length);
		offset += bytes.Length;
		size += bytes.Length;
	}

	/// <summary>
	/// Writes the U int.
	/// </summary>
	/// <param name="value">Value.</param>
	public void WriteUInt( uint value ) {       
		WriteInt((int)value);
	}

	/// <summary>
	/// Writes the string.
	/// </summary>
	/// <param name="value">Value.</param>
	public void WriteString( string value ) {
		if(!storing) return;
		if(value.Length > UInt16.MaxValue) return;

		byte[] bytes = System.Text.Encoding.UTF8.GetBytes(value);
		ushort len = (ushort)bytes.Length;
		WriteUShort(len);
		Buffer.BlockCopy(bytes, 0, data, offset, len);
		offset += len;
		size += len;
	}

	/// <summary>
	/// Writes the bytes.
	/// </summary>
	/// <param name="buffer">Buffer.</param>
	public void WriteBytes( byte[] buffer ) {
		if(!storing) return;

		int len = buffer.Length;
		Buffer.BlockCopy(buffer, 0, data, offset, len);
		offset += len;
		size += len;
	}

	/// <summary>
	/// Writes the block.
	/// </summary>
	/// <param name="buffer">Buffer.</param>
	public void WriteBlock( byte[] buffer ) {
		WriteInt(buffer.Length);
		WriteBytes(buffer);
	}

	/// <summary>
	/// Writes the float.
	/// </summary>
	/// <param name="value">Value.</param>
	public void WriteFloat( float value ) {
		if(!storing) return;

		byte[] bytes = BitConverter.GetBytes(value);
		Buffer.BlockCopy(bytes, 0, data, offset, bytes.Length);
		offset += bytes.Length;
		size += bytes.Length;
	}

	/// <summary>
	/// Finish this instance.
	/// </summary>
	public void Finish() {
		if(!storing) return;

		byte[] bytes = BitConverter.GetBytes(size);
		Buffer.BlockCopy(bytes, 0, data, 0, sizeof(int));

		// write packet type
		bytes = BitConverter.GetBytes(type);
		Buffer.BlockCopy(bytes, 0, data, sizeof(int), sizeof(int));
	}

	/// <summary>
	/// Determines whether the specified <see cref="INetPacket"/> is equal to the current <see cref="INetPacket"/>.
	/// </summary>
	/// <param name="other">The <see cref="INetPacket"/> to compare with the current <see cref="INetPacket"/>.</param>
	/// <returns><c>true</c> if the specified <see cref="INetPacket"/> is equal to the current <see cref="INetPacket"/>; otherwise, <c>false</c>.</returns>
	public bool Equals( INetPacket other ) {
		if( size == other.Size )
		{
			byte[] otherData = other.GetBytes();
			for( int i = 0; i < size; i++ )
			{
				if( data[i] != otherData[i] )
					return false;
			}
			return true;
		}

		return false;
	}
}


