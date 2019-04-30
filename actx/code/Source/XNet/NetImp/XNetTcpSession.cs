using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.Net.Sockets;
using System.Net;
using SLua;

/// <summary>
/// Net tcp state object.
/// </summary>
public class NetTcpStateObject
{
	/// <summary>
	/// The size of the buffer.
	/// </summary>
	public const int BufferSize = 1024 * 64;

	/// <summary>
	/// The work socket.
	/// </summary>
	public Socket workSocket 	= default(Socket);
	/// <summary>
	/// The buffer.
	/// </summary>
	public byte[] buffer 		= new byte[BufferSize];
	/// <summary>
	/// The write offset.
	/// </summary>
	public int writeOffset 		= 0;
	/// <summary>
	/// The read offset.
	/// </summary>
	public int readOffset 		= 0;
}

/// <summary>
/// Net tcp session.
/// </summary>
[CustomLuaClass]
public class XNetTcpSession : INetSession
{
	/// <summary>
	/// The socket.
	/// </summary>
	protected Socket socket = default(Socket);

	/// <summary>
	/// The packet queue.
	/// </summary>
	protected Queue<INetPacket> 
		recvQueue = new Queue<INetPacket> ();

	/// <summary>
	/// The send queue.
	/// </summary>
	protected Queue<INetPacket> 
		sendQueue = new Queue<INetPacket>();

	// socket thread
	protected Thread connThread = default(Thread);
	protected Thread sendThread = default(Thread);
	protected Thread recvThread = default(Thread);

	/// <summary>
	/// Connect the specified ipAddress and nPort.
	/// </summary>
	/// <param name="ipAddress">Ip address.</param>
	/// <param name="nPort">N port.</param>
	public override bool 	Connect(string szIPAddress, int nPort)
	{
		bool result = Connected ();
		if (result) {
			#if UNITY_EDITOR
			UnityEngine.Debug.LogError(string.Format("{0}:{1} Connect the existing", szIPAddress, nPort));
			#endif

			return true;
		}
		else {
			try{
				IPAddress address;
				if (!IPAddress.TryParse(szIPAddress, out address)){
					IPHostEntry entry = Dns.GetHostEntry(szIPAddress);
					if (entry.AddressList.Length > 0)
					{
						address = entry.AddressList[0];
					}
				}

				socket = new Socket(AddressFamily.InterNetwork, 
					SocketType.Stream, ProtocolType.Tcp);
				socket.Blocking = true;

				socket.SetSocketOption( SocketOptionLevel.Socket, 	SocketOptionName.SendTimeout, 	7000 );
				socket.SetSocketOption( SocketOptionLevel.Socket, 	SocketOptionName.Linger, 		new LingerOption(true, 5) );
				socket.SetSocketOption( SocketOptionLevel.Tcp, 		SocketOptionName.NoDelay, 		true );

				// start connect thread
				connThread = new Thread( new ParameterizedThreadStart(OnConnThread) );
				connThread.Start(
					new IPEndPoint(address, nPort)
				);
					
				return true;
			}
			catch(System.Exception e) {
				#if UNITY_EDITOR
				UnityEngine.Debug.LogError(e.Message);
				#endif

				PostPacket (new INetPacket (PacketType.SOCKET_CONNECT_FAILURE));
				return true;
			}
		}
	}

	/// <summary>
	/// Sends the packet.
	/// </summary>
	/// <param name="packet">Packet.</param>
	public override void 	SendPacket (INetPacket packet)
	{
		packet.Finish ();

		lock (sendQueue) {
			sendQueue.Enqueue (packet);

			#if UNITY_EDITOR
			UnityEngine.Debug.Log(string.Format("Send queue count ({0}))", 
				sendQueue.Count));
			#endif			
		}
	}

	/// <summary>
	/// Posts the packet.
	/// </summary>
	/// <param name="IPacket">I packet.</param>
	public override void 	PostPacket (INetPacket packet)
	{
		packet.Finish ();

		lock (recvQueue) {
			recvQueue.Enqueue (packet);

			#if UNITY_EDITOR
			UnityEngine.Debug.Log(string.Format("Packet type({0}) size({1}) Enqueue queue({2})",
				packet.Type, packet.Size, recvQueue.Count));
			#endif
		}
	}

	/// <summary>
	/// Gets the packet queue.
	/// </summary>
	/// <returns>The packet queue.</returns>
	public override Queue<INetPacket> GetPacketQueue ()
	{
		return recvQueue;
	}

	/// <summary>
	/// Close this instance.
	/// </summary>
	public override void 	Close ()
	{
		sendQueue.Clear ();

		if (socket != default(Socket)) {
			socket.Close ();
		}
	}

	/// <summary>
	/// Connected this instance.
	/// </summary>
	public override bool	Connected ()
	{
		return socket != default(Socket) && socket.Connected;
	}

	/// <summary>
	/// Raises the conn thread event.
	/// </summary>
	/// <param name="ipEndPoint">Ip end point.</param>
	public void 			OnConnThread(object ipEndPoint)
	{
		IPEndPoint remoteEP = (IPEndPoint)ipEndPoint;
		try{
			socket.Connect(remoteEP);
			
			// start send thread
			sendThread = new Thread( new ThreadStart(OnSendThread) );
			sendThread.Start();

			// start recv thread
			recvThread = new Thread( new ThreadStart(OnRecvThread) );
			recvThread.Start();

			PostPacket(
				new INetPacket(PacketType.SOCKET_CONNECT_SUCCESS)
			);
		}
		catch(Exception e ) {
			#if UNITY_EDITOR
			UnityEngine.Debug.LogError(e.Message);
			#endif

			PostPacket(
				new INetPacket(PacketType.SOCKET_CONNECT_FAILURE)
			);
		}

		connThread.Abort ();
	}

	/// <summary>
	/// Raises the send thread event.
	/// </summary>
	/// <param name="o">O.</param>
	public void 		OnSendThread()
	{
		try{
			if (!socket.Poll (10000000, SelectMode.SelectWrite)) {
				socket.Close ();
			} else {
				while (true) {
					if (!Connected())
						break;
					
					if (sendQueue.Count > 0) {
						INetPacket packet = default(INetPacket);

						lock (sendQueue) {
							packet = sendQueue.Dequeue ();	
						}

						if (packet != null) {
							int nSendBytes = socket.Send (packet.GetBytes(), 0, packet.Size + sizeof(int), SocketFlags.None);

							#if UNITY_EDITOR
							UnityEngine.Debug.Log(string.Format("Send packet type({0}) size({1})", 
								packet.Type, nSendBytes));
							#endif
						}
					}

					Thread.Sleep (10);
				}
			}			
		} catch (Exception e) {
			#if UNITY_EDITOR
			UnityEngine.Debug.LogError(e.Message);
			#endif

			PostPacket (
				new INetPacket(PacketType.SOCKET_DISCONNECT)
			);
		}

		sendThread.Abort ();		
	}

	/// <summary>
	/// Raises the recv thread event.
	/// </summary>
	public void 		OnRecvThread()
	{
		NetTcpStateObject netState = new NetTcpStateObject ();
		netState.workSocket = socket;

		try{
			while (true) {
				if (!Connected())
					break;

				int nReadBytes = socket.Receive(netState.buffer, 
					netState.writeOffset, NetTcpStateObject.BufferSize - netState.writeOffset, SocketFlags.None);
				if (nReadBytes <= 0)
					break;

				nReadBytes += netState.writeOffset;

				int nIndex 		= netState.readOffset;
				int nPacketSize = 0;

				while( nIndex < nReadBytes )
				{
					int nOffset = netState.readOffset;
					nPacketSize = BitConverter.ToInt32(netState.buffer, nOffset);

					if (nIndex + sizeof(int) + nPacketSize <= nReadBytes)
					{
						nOffset += sizeof(int);

						INetPacket packet = new INetPacket();
						packet.Set(nPacketSize, netState.buffer, nOffset);

						if (packet.Type != 0){
							PostPacket(packet);
						}

						netState.readOffset += (sizeof(int) + nPacketSize);
					}

					nIndex += (sizeof(int) + nPacketSize); 
				}
					
				if( nIndex == nReadBytes)
				{
					netState.readOffset 	= 0;
					netState.writeOffset 	= 0;
				}
				else
				{
					if( netState.readOffset + nPacketSize > NetTcpStateObject.BufferSize )
					{
						byte[] newBuffer = new byte[NetTcpStateObject.BufferSize];

						int bytesCopy = nReadBytes - netState.readOffset;
						Buffer.BlockCopy(netState.buffer, 
							netState.readOffset, newBuffer, 0, bytesCopy);

						netState.buffer 		= newBuffer;
						netState.readOffset 	= 0;
						netState.writeOffset 	= bytesCopy;

						#if UNITY_EDITOR
						UnityEngine.Debug.Log(string.Format("Receive create new buffer readOffset({0}) packetSize({1}) writeOffset({2})", 
							netState.readOffset, nPacketSize, netState.writeOffset ));
						#endif
					}
					else
					{
						netState.writeOffset = nReadBytes;
					}
				}
			}
		} catch(Exception e) {
			#if UNITY_EDITOR
			UnityEngine.Debug.LogError(e.Message);
			#endif

			PostPacket (
				new INetPacket(PacketType.SOCKET_DISCONNECT)
			);
		}

		// close net recv thread
		Close ();

		recvThread.Abort ();
	}
}
