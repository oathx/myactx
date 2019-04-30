using System;
using System.Collections;
using System.Collections.Generic;
using SLua;

/// <summary>
/// Net server config.
/// </summary>
[CustomLuaClass]
public class TcpServerConfig {
	public string 		ipAddress;
	public int 			ipPort;
	public int 			timeOut;
	public LuaFunction	netFunc;

	/// <summary>
	/// Initializes a new instance of the <see cref="NetServerConfig"/> class.
	/// </summary>
	public TcpServerConfig()
	{
	}
}

/// <summary>
/// Net server.
/// </summary>
[CustomLuaClass]
public class XTcpServer
{
	/// <summary>
	/// The instance.
	/// </summary>
    private static readonly XTcpServer instance = new XTcpServer();

	/// <summary>
	/// Gets the singleton.
	/// </summary>
	/// <returns>The singleton.</returns>
    public static XTcpServer GetSingleton()
	{
		return instance;
	}

	/// <summary>
	/// The session.
	/// </summary>
	protected INetSession 		session = null;
	protected TcpServerConfig 	config 	= null;
	protected LuaFunction		netFunc = null;

	/// <summary>
	/// Initliaze this instance.
	/// </summary>
	public virtual void 	Initliaze()
	{
		if (session == null) {
			session = new XNetTcpSession ();
		}		
	}

	/// <summary>
	/// Startup this instance.
	/// </summary>
	public virtual void 	Startup(TcpServerConfig serverConfig)
	{
		config 	= serverConfig;
		netFunc = config.netFunc;
	}

	/// <summary>
	/// Connect the specified szIPAddress and nPort.
	/// </summary>
	/// <param name="szIPAddress">Size IP address.</param>
	/// <param name="nPort">N port.</param>
	public virtual bool 	Connect()
	{
		return session.Connect (config.ipAddress, config.ipPort);
	}

	/// <summary>
	/// Connected this instance.
	/// </summary>
	public virtual bool		Connected()
	{
		return session.Connected();
	}

	/// <summary>
	/// Disconnect this instance.
	/// </summary>
	public virtual void 	Disconnect()
	{
		if (session.Connected ()) {
			session.Close ();
		}
	}

	/// <summary>
	/// Reconnect this instance.
	/// </summary>
	public virtual bool 	Reconnect()
	{
		Disconnect ();

		#if UNITY_EDITOR
		UnityEngine.Debug.Log(string.Format("Try Reconnect {0}:{1}", config.ipAddress, config.ipPort));
		#endif

		return Connect ();
	}

	/// <summary>
	/// Send the specified packet.
	/// </summary>
	/// <param name="packet">Packet.</param>
	[DoNotToLuaAttribute]
	public virtual void 	Send(INetPacket packet)
	{
		if (session.Connected ()) {
			session.SendPacket (packet);
		}
	}

	public virtual void 	Send(XNetLuaPacket packet)
	{
		Send (packet.Data);
	}

	/// <summary>
	/// Post the specified packet.
	/// </summary>
	/// <param name="packet">Packet.</param>
	[DoNotToLuaAttribute]
	public virtual void 	Post(INetPacket packet)
	{
		session.PostPacket (packet);
	}

	/// <summary>
	/// Post the specified packet.
	/// </summary>
	/// <param name="packet">Packet.</param>
	public virtual void 	Post(XNetLuaPacket packet)
	{
		Post (packet.Data);	
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	public virtual void 	Update()
	{
		if (session != null) {
			Queue<INetPacket> recvQueue = session.GetPacketQueue ();

			while (recvQueue.Count > 0) {
				INetPacket packet = null;

				lock (recvQueue) {
					packet = recvQueue.Dequeue ();
				}

				if (packet != null) {
					netFunc.call (new XNetLuaPacket (packet));	
				}
			}
		}
	}

	/// <summary>
	/// Shutdown this instance.
	/// </summary>
	public virtual void 	Shutdown()
	{
		if (session != null) {
			Queue<INetPacket> recvQueue = session.GetPacketQueue ();

			lock (recvQueue) {
				recvQueue.Clear ();
			}

			Disconnect ();
		}
	}
}

