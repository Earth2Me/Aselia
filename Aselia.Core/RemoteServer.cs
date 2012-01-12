using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using Aselia.Common.Core;
using Aselia.Common.Core.Configuration;
using Aselia.Core.InterServer;

namespace Aselia.Core
{
	public sealed class RemoteServer : MarshalByRefObject, IDisposable
	{
		private readonly Server Local;
		private readonly TcpClient Client;
		private bool IsDisposing;
		private bool IsDisposable;
		private SslStream Ssl;
		private ServerInfo Info;
		private byte[] CommandBuffer = new byte[sizeof(ServerCommands)];
		private byte[] LengthBuffer = new byte[sizeof(int)];
		private byte[] BodyBuffer;
		private ServerCommands ReadCommand;
		private int ReadLength;

		public List<RemoteServer> Forwards { get; private set; }

		public RemoteServer(Server local, TcpClient client)
		{
			Local = local;
			Client = client;
			Forwards = new List<RemoteServer>();
		}

		public RemoteServer(Server local, ServerInfo info)
		{
			Local = local;
			Forwards = new List<RemoteServer>();
			Info = info;

			Client = new TcpClient();
			Client.BeginConnect(info.InterServerIp, info.InterServerPort, OnBeginConnect, null);
		}

		private void OnBeginConnect(IAsyncResult ar)
		{
			if (!ar.IsCompleted || !Client.Connected)
			{
				OnDropped();
				return;
			}

			try
			{
				Ssl = new SslStream(Client.GetStream(), false, RemoteVerification, LocalSelection, EncryptionPolicy.RequireEncryption);
				Ssl.BeginAuthenticateAsClient(Info.Id, OnBeginAuthenticateAsClient, null);
			}
			catch
			{
				OnDropped();
			}
		}

		private void OnDropped()
		{
			if (IsDisposable)
			{
				Dispose("Connection drop negotiated.");
			}
			else
			{
				// TODO: Handle non-negotiated drops.
			}
		}

		private void BeginReadCommand()
		{
			try
			{
				if (!Ssl.CanRead)
				{
					OnDropped();
				}

				Ssl.BeginRead(CommandBuffer, 0, CommandBuffer.Length, OnBeginReadCommand, null);
			}
			catch
			{
				OnDropped();
			}
		}

		private void BeginWrite(ServerCommands command)
		{
			Ssl.BeginWrite(new byte[] { (byte)command }, 0, sizeof(byte), OnBeginWrite, null);
		}

		private void OnBeginWrite(IAsyncResult ar)
		{
			try
			{
				if (!ar.IsCompleted)
				{
					OnDropped();
					return;
				}

				Ssl.EndWrite(ar);
			}
			catch
			{
				OnDropped();
			}
		}

		public void SendReloading()
		{
			IsDisposable = true;
			BeginWrite(ServerCommands.Reloading);
		}

		private void BeginReadLength()
		{
			try
			{
				if (!Ssl.CanRead)
				{
					OnDropped();
				}

				Ssl.BeginRead(LengthBuffer, 0, LengthBuffer.Length, OnBeginReadLength, null);
			}
			catch
			{
				OnDropped();
			}
		}

		private void BeginReadBody()
		{
			try
			{
				if (!Ssl.CanRead)
				{
					OnDropped();
				}

				Ssl.BeginRead(BodyBuffer = new byte[ReadLength], 0, ReadLength, OnBeginReadBody, null);
			}
			catch
			{
				OnDropped();
			}
		}

		private void OnBeginReadCommand(IAsyncResult ar)
		{
			try
			{
				if (!ar.IsCompleted)
				{
					OnDropped();
				}
				if (Ssl.EndRead(ar) != CommandBuffer.Length)
				{
					OnDropped();
				}

				ReadCommand = (ServerCommands)CommandBuffer[0];

				try
				{
					switch (ReadCommand)
					{
					case ServerCommands.Void:
						break;

					case ServerCommands.Reloading:
						OnReloading();
						break;

					case ServerCommands.JoinedLate:
						OnJoinedLate();
						break;

					case ServerCommands.Dispose:
						OnDispose();
						break;

					case ServerCommands.CacheRequest:
						OnCacheRequest();
						break;

					default:
						BeginReadLength();
						return;
					}
				}
				catch
				{
				}
				// NOT finally:
				BeginReadCommand();
			}
			catch
			{
				OnDropped();
			}
		}

		private void OnCacheRequest()
		{
			using (MemoryStream mem = new MemoryStream())
			{
				mem.WriteByte(unchecked((byte)ServerCommands.Cache));
				Local.Cache.Serialize(mem, true);

				byte[] buffer = mem.ToArray();
				Ssl.BeginWrite(buffer, 0, buffer.Length, OnBeginWrite, null);
			}
		}

		private void OnDispose()
		{
			IsDisposable = true;
			Dispose("Remote server requested disposal.");
		}

		public void SendDispose()
		{
			IsDisposable = true;
			BeginWrite(ServerCommands.Dispose);
		}

		private void OnJoinedLate()
		{
			if (!Local.NetworkEstablished)
			{
				Local.OnJoinedLate();
			}
		}

		private void OnBeginReadBody(IAsyncResult ar)
		{
			try
			{
				if (!ar.IsCompleted)
				{
					OnDropped();
				}
				if (Ssl.EndRead(ar) != ReadLength)
				{
					OnDropped();
				}

				try
				{
					switch (ReadCommand)
					{
					case ServerCommands.ToChannel:
						OnToChannel();
						break;

					case ServerCommands.ToUser:
						OnToUser();
						break;

					case ServerCommands.Cache:
						OnCache();
						break;
					}
				}
				catch
				{
				}
				finally
				{
					BeginReadCommand();
				}
			}
			catch
			{
				OnDropped();
			}
		}

		private void OnCache()
		{
			using (MemoryStream mem = new MemoryStream(BodyBuffer))
			{
				Local.Cache = Cache.Load(mem);
			}

			Local.CommitCache();
		}

		private unsafe void ReadMessagePacket(out string target, out string origin, out string command, out string[] args)
		{
			List<string> lines = new List<string>();
			fixed (byte* body = BodyBuffer)
			{
				int index = 1;
				while (index < BodyBuffer.Length)
				{
					ushort length = *(ushort*)&body[index];
					index += 2;
					Packer.Unpack(BodyBuffer, index, length);
				}
			}

			command = Rfc.ToCommand(BodyBuffer[0]);
			target = lines[1];
			origin = lines[2];
			args = lines.ToArray();
		}

		private void WritePacket(string target, string origin, string command, string[] args)
		{
			byte[][] packed = new byte[args.Length + 3][];
			packed[0] = new byte[] { Rfc.FromCommand(command) };
			packed[1] = Packer.Pack(target);
			packed[2] = Packer.Pack(origin);
			for (int i = 2; i < args.Length + 3; i++)
			{
				packed[i] = Packer.Pack(args[i]);
			}

			byte[] buffer = new byte[packed.Length];
			for (int p = 0, b = 0; b < buffer.Length; b += packed[p++].Length)
			{
				Array.Copy(packed[p], 0, buffer, b, packed[p].Length);
			}
		}

		private void OnToUser()
		{
			string target, origin, command;
			string[] args;
			ReadMessagePacket(out target, out origin, out command, out args);
			if (command == null)
			{
				Console.WriteLine("Received invalid command from remote server.");
				return;
			}

			UserBase user = Local.GetUser(target);
			if (user != null)
			{
				string[] buffer = new string[args.Length + 1];
				user.SendCommand(command, origin, buffer);
			}
		}

		private void OnToChannel()
		{
			string target, origin, command;
			string[] args;
			ReadMessagePacket(out target, out origin, out command, out args);
		}

		private void OnReloading()
		{
			IsDisposable = true;
			Dispose("Remote server reloading.");
		}

		public void Dispose(string reason)
		{
			Console.WriteLine("Disposing remote server: {0}", reason);
		}

		private void OnBeginReadLength(IAsyncResult ar)
		{
			try
			{
				if (!ar.IsCompleted)
				{
					OnDropped();
				}
				if (Ssl.EndRead(ar) != LengthBuffer.Length)
				{
					OnDropped();
				}

				ReadLength = BitConverter.ToInt32(LengthBuffer, 0);
				BeginReadBody();
			}
			catch
			{
				OnDropped();
			}
		}

		private void OnBeginAuthenticateAsClient(IAsyncResult ar)
		{
			try
			{
				Ssl.EndAuthenticateAsClient(ar);
				BeginReadCommand();
				if (Local.NetworkEstablished)
				{
					BeginWrite(ServerCommands.JoinedLate);
				}
			}
			catch
			{
				OnDropped();
			}
		}

		private bool RemoteVerification(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}

		private X509Certificate LocalSelection(object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
		{
			return Local.Certificates.Certificate;
		}

		public void Dispose()
		{
			if (IsDisposing)
			{
				return;
			}
			IsDisposing = true;

			try
			{
				if (Client.Connected)
				{
					Client.Close();
				}
			}
			catch
			{
			}
		}
	}
}