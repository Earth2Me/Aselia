using System;
using System.IO;
using System.Text;

namespace Aselia
{
	public sealed class SafeStream : Stream
	{
		public const int MAX = 400;

		private readonly Stream Stream;
		private byte[] ReadBuffer;
		private AsyncCallback ReadCallback;
		private readonly StringBuilder Line = new StringBuilder(MAX);

		public event EventHandler Disposed;

		public bool IsDisposed { get; private set; }

		public SafeStream(Stream stream)
		{
			Stream = stream;
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
				Stream.BeginWrite(data, 0, data.Length, OnBeginWriteLine, null);
			}
			catch
			{
				Dispose();
			}
		}

		private void OnBeginWriteLine(IAsyncResult ar)
		{
			if (ar.IsCompleted && Stream.CanWrite)
			{
				try
				{
					Stream.EndWrite(ar);
					Stream.Flush();
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