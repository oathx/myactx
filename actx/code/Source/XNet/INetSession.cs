using System;
using System.Collections;
using System.Collections.Generic;
using SLua;

[CustomLuaClass]
public abstract class INetSession
{
	public abstract bool 				Connect(string ipAddress, int nPort);
	public abstract void 				SendPacket(INetPacket packet);
	public abstract void 				PostPacket(INetPacket IPacket);
	public abstract void 				Close ();
	public abstract bool				Connected ();
	public abstract Queue<INetPacket>	GetPacketQueue ();
}
