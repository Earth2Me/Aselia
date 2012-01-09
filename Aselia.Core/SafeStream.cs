using System;
using System.IO;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace Aselia
{
	public sealed class SafeStream : Stream
	{
		public const int MAX = 400;

		private readonly Stream Stream;
		private byte[] ReadBuffer;
		private AsyncCallback ReadCallback;
		private readonly StringBuilder Line = new StringBuilder(MAX);
		private readonly bool Encrypted;
		private readonly SslStream Ssl;
		private readonly X509Certificate2 Certificate;
		private volatile bool IsWriting;

		public event EventHandler Disposed;

		public bool IsDisposed { get; private set; }

		public SafeStream(Stream stream, X509Certificate2 certificate)
		{
			Certificate = certificate;
			Encrypted = certificate != null;
			if (Encrypted)
			{
				Stream = Ssl = new SslStream(stream, false, RemoteVerification, LocalSelection, EncryptionPolicy.RequireEncryption);
			}
			else
			{
				Stream = stream;
				Ssl = null;
			}
		}

		private bool RemoteVerification(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}

		private X509Certificate LocalSelection(object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
		{
			return Certificate;
		}

		public void BeginAuthenticate(AsyncCallback callback)
		{
			if (!Encrypted)
			{
				return;
			}

			Ssl.BeginAuthenticateAsServer(Certificate, true, SslProtocols.Tls, false, OnBeginAuthenticate, callback);
		}

		private void OnBeginAuthenticate(IAsyncResult ar)
		{
			try
			{
				if (!ar.IsCompleted)
				{
					Dispose();
					return;
				}

				Ssl.EndAuthenticateAsServer(ar);
				((AsyncCallback)ar.AsyncState).Invoke(ar);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Dispose();
			}
		}

		public override int ReadByte()
		{
			int b = Stream.ReadByte();
			if (b < 0)
			{
				Dispose();
			}
			return b;
		}

		public void BeginReadLine(AsyncCallback callback, object state)
		{
			Line.Clear();
			ReadBuffer = new byte[1];
			ReadCallback = callback;
			Stream.BeginRead(ReadBuffer, 0, ReadBuffer.Length, OnBeginReadLine, state);
		}

		private void OnBeginReadLine(IAsyncResult ar)
		{
			if (!ar.IsCompleted || !Stream.CanRead)
			{
				Dispose();
				return;
			}

			int read;
			try
			{
				read = Stream.EndRead(ar);
			}
			catch
			{
				Dispose();
				return;
			}
			if (read < 1)
			{
				Dispose();
				return;
			}

			char c = (char)ReadBuffer[0];
			if (c == '\n' || c == '\0' || Line.Length >= MAX)
			{
				ReadCallback.Invoke(ar);
			}
			else
			{
				Line.Append(c);
				Stream.BeginRead(ReadBuffer, 0, ReadBuffer.Length, OnBeginReadLine, ar.AsyncState);
			}
		}

		public string EndReadLine(IAsyncResult ar)
		{
			return Line.ToString().TrimEnd('\r');
		}

		public override void Flush()
		{
			Stream.Flush();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			try
			{
				if (!Stream.CanWrite)
				{
					Dispose();
					return;
				}
				Stream.Write(buffer, offset, count);
			}
			catch
			{
				Dispose();
				return;
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			try
			{
				return Stream.Read(buffer, offset, count);
			}
			catch
			{
				Dispose();
				return -1;
			}
		}

		private void OnDisposed()
		{
			if (Disposed != null)
			{
				Disposed.Invoke(this, new EventArgs());
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (IsDisposed)
				{
					return;
				}
				IsDisposed = true;

				try
				{
					Stream.Dispose();
				}
				catch
				{
				}

				OnDisposed();
			}

			base.Dispose(disposing);
		}

		public void BeginWriteLine(string line)
		{
			byte[] data = ASCIIEncoding.ASCII.GetBytes(line + "\r\n");

			try
			{
				BeginWrite(data, 0, data.Length, OnBeginWriteLine, null);
			}
			catch
			{
				Dispose();
			}
		}

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			while (IsWriting)
			{
				Thread.Sleep(0);
			}
			IsWriting = true;

			return base.BeginWrite(buffer, offset, count, callback, state);
		}

		public override void EndWrite(IAsyncResult asyncResult)
		{
			try
			{
				base.EndWrite(asyncResult);
			}
			finally
			{
				IsWriting = false;
			}
		}

		private void OnBeginWriteLine(IAsyncResult ar)
		{
			if (ar.IsCompleted && Stream.CanWrite)
			{
				try
				{
					EndWrite(ar);
					Flush();
				}
				catch
				{
					Dispose();
				}
			}
			else
			{
				Dispose();
			}
		}

		public override bool CanRead
		{
			get { return Stream.CanRead; }
		}

		public override bool CanSeek
		{
			get { return Stream.CanSeek; }
		}

		public override bool CanWrite
		{
			get { return Stream.CanWrite; }
		}

		public override long Length
		{
			get { return Stream.Length; }
		}

		public override long Position
		{
			get
			{
				return Stream.Position;
			}
			set
			{
				Stream.Position = value;
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return Stream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			Stream.SetLength(value);
		}
	}
}