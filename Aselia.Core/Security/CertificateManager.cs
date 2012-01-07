using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Aselia.Common.Security;

namespace Aselia.Core.Security
{
	public sealed class CertificateManager : CertificateManagerBase
	{
		public override X509Certificate2 Certificate { get; set; }

		public override bool Load(string id, string password)
		{
			DirectoryInfo dir = new DirectoryInfo(Environment.CurrentDirectory);
			FileInfo[] files = dir.EnumerateFiles("Certificate." + id + ".*").ToArray();
			if (files.Length != 1)
			{
				return false;
			}

			try
			{
				Certificate = new X509Certificate2(files[0].FullName, password);
				if (Certificate == null)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
			catch
			{
				return false;
			}
		}

		public override bool Generate(string id, string password)
		{
			try
			{
				byte[] certificate = CertificateGenerator.CreateSelfSignCertificatePfx(
					"CN=" + id,
					DateTime.Now,
					DateTime.Now + new TimeSpan(365 * 30, 6 * 30, 0, 0),
					password);

				using (FileStream fs = File.Create("Certificate." + id + ".pfx"))
				{
					fs.Write(certificate, 0, certificate.Length);
					fs.Flush();
				}

				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}