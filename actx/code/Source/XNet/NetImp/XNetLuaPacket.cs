using UnityEngine;
using System;
using System.Collections;
using SLua;

[CustomLuaClass]
public class XNetLuaPacket {
	private INetPacket data = default(INetPacket);

	[DoNotToLua]
	public INetPacket Data {
		get { return data; }
	}

	[DoNotToLua]
	public XNetLuaPacket( INetPacket packet ) {
		data = packet;
	}

	public XNetLuaPacket( int packetType ) {
		data = new INetPacket( packetType );
	}

	public int Type() {
		return data.Type;
	}

	public int Size() {
		return data.Size;
	}

	public void SetOffset(int nOffset) {
		data.Offset = nOffset;
	}

	public void Finish() {
		data.Finish ();
	}

	public char ReadChar() {
		return data.ReadChar();
	}

	public byte ReadByte() {
		return data.ReadByte();
	}

	public short ReadShort() {
		return data.ReadShort();
	}

	public ushort ReadUShort() {
		return data.ReadUShort();
	}

	public int ReadInt() {
		return data.ReadInt();
	}

	public uint ReadUInt() {
		return data.ReadUInt();
	}

	public string ReadString() {
		return data.ReadString();
	}

	public byte[] ReadBlock() {
		return data.ReadBlock();
	}

	public float ReadFloat() {
		return data.ReadFloat();
	}

	public void WriteChar( char value ) {
		data.WriteChar(value);
	}

	public void WriteByte( byte value ) {
		data.WriteByte(value);
	}

	public void WriteShort( short value ) {
		data.WriteShort(value);
	}

	public void WriteUShort( ushort value ) {
		data.WriteUShort(value);
	}

	public void WriteInt( int value ) {
		data.WriteInt(value);
	}

	public void WriteUInt( uint value ) {       
		data.WriteUInt(value);
	}

	public void WriteString( string value ) {
		data.WriteString(value);
	}

	public void WriteBlock( byte[] buffer ) {
		data.WriteBlock(buffer);
	}

	public void WriteFloat( float value ) {
		data.WriteFloat(value);
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int ReadBlock(IntPtr l) {
		try {
			XNetLuaPacket self=(XNetLuaPacket)LuaObject.checkSelf(l);
			byte[] bytes = self.data.ReadBlock();
			LuaObject.pushValue(l,true);
			LuaDLL.lua_pushlstring(l, bytes, bytes.Length);
			return 2;
		}
		catch(Exception e) {
			return LuaObject.error(l,e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int WriteBlock(IntPtr l) {
		try {
            XNetLuaPacket self = (XNetLuaPacket)LuaObject.checkSelf(l);
			byte[] bytes = LuaDLL.lua_tobytes(l, 2);
			self.data.WriteBlock(bytes);
			LuaObject.pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return LuaObject.error(l,e);
		}
	}
}
