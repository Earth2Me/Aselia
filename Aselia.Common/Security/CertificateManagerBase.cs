using System;
using System.Security.Cryptography.X509Certificates;

namespace Aselia.Common.Security
{
	[Serializable]
	public abstract class CertificateManagerBase : MarshalByRefObject
	{
		public abstract X509Certificate2 Certificate { get; set; }

		public abstract bool Load(string id, string password);

		public abstract bool Generate(string id, string password);

		public CertificateManagerBase()
		{
		}

		public CertificateManagerBase(CertificateManagerBase clone)
		{
			Certificate = clone.Certificate;
		}
	}
}