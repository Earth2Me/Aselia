using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Aselia.Common.Core;
using Aselia.Common.Flags;
using Aselia.Common.Modules;

namespace Aselia.Common.Hotswap
{
	public sealed class DomainManager : MarshalByRefObject
	{
		private readonly Dictionary<Domains, AppDomain> AppDomains = new Dictionary<Domains, AppDomain>();

		public ServerBase Server { get; private set; }

		public Dictionary<string, ReceivedCommandEventHandler> UserCommandHandlers { get; private set; }

		public Dictionary<string, CommandAttribute> UserCommandAttrs { get; private set; }

		public Dictionary<Modes, ReceivedChannelModeEventHandler> AddChannelModeHandlers { get; private set; }

		public Dictionary<Modes, ReceivedChannelModeEventHandler> RemoveChannelModeHandlers { get; private set; }

		public Dictionary<Modes, ChannelModeAttribute> ChannelModeAttrs { get; private set; }

		public DomainManager()
		{
		}

		public void Reload()
		{
			Reload(Domains.Core);
			Reload(Domains.UserCommands);
			Reload(Domains.ChannelModes);
		}

		public void Reload(Domains domain)
		{
			string name = Enum.GetName(typeof(Domains), domain);
			AppDomain ad = AppDomain.CreateDomain(name);
			byte[] assembly;
			FileInfo file = new FileInfo("Aselia." + name + ".dll");
			using (FileStream fs = file.OpenRead())
			{
				assembly = new byte[fs.Length];
				fs.Read(assembly, 0, assembly.Length);
			}
			ad.Load(assembly);

			try
			{
				Initialize(domain, ad);
			}
			catch (Exception ex)
			{
				try
				{
					AppDomain.Unload(ad);
				}
				catch
				{
				}
				throw new Exception("Error initializing new domain.", ex);
			}

			AppDomain remove = AppDomains.ContainsKey(domain) ? AppDomains[domain] : null;
			AppDomains[domain] = ad;
			if (remove != null)
			{
				try
				{
					AppDomain.Unload(remove);
				}
				catch (Exception ex)
				{
					throw new Exception("Loaded new domain, but error unloading old domain.", ex);
				}
			}
		}

		private void Initialize(Domains domain, AppDomain ad)
		{
			switch (domain)
			{
			case Domains.Core:
				InitializeCore(ad);
				break;

			case Domains.UserCommands:
				InitializeUserCommand(ad);
				break;

			case Domains.ChannelModes:
				InitializeChannelMode(ad);
				break;
			}
		}

		private void InitializeCore(AppDomain ad)
		{
			Assembly[] asms = ad.GetAssemblies();
			if (asms.Length != 1)
			{
				throw new InvalidOperationException("There must be exactly one assembly in the core DLL.");
			}

			Type[] types = (from x in asms[0].GetTypes()
							where x.BaseType == typeof(ServerBase)
							select x).ToArray();
			if (types.Length != 1)
			{
				throw new InvalidOperationException("There must be exactly one type extending ServerBase in the core assembly.");
			}

			try
			{
				if (Server == null)
				{
					Server = (ServerBase)types[0].GetConstructor(new Type[] { typeof(DomainManager) }).Invoke(new object[] { this });
					Server.Start();
				}
				else
				{
					ServerBase old = Server;
					Server = (ServerBase)types[0].GetConstructor(new Type[] { typeof(DomainManager), typeof(ServerBase) }).Invoke(new object[] { this, Server });
					old.Unload();
					Server.Load();
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Unable to instantiate or hotswap the core server.", ex);
			}
		}

		private void InitializeUserCommand(AppDomain ad)
		{
			Dictionary<string, ReceivedCommandEventHandler> userCommandHandlers = new Dictionary<string, ReceivedCommandEventHandler>();
			Dictionary<string, CommandAttribute> userCommandAttrs = new Dictionary<string, CommandAttribute>();

			foreach (Assembly a in ad.GetAssemblies())
			{
				IEnumerable<Type> types = from x in a.GetTypes()
										  where x.GetInterfaces().Contains(typeof(ICommand))
										  select x;

				foreach (Type t in types)
				{
					try
					{
						CommandAttribute[] attrs = t.GetCustomAttributes(typeof(CommandAttribute), false).Cast<CommandAttribute>().ToArray();
						if (attrs.Length < 1 || userCommandHandlers.ContainsKey(attrs[0].Command))
						{
							continue;
						}

						ICommand command = (ICommand)t.GetConstructor(Type.EmptyTypes).Invoke(new object[0]);
						userCommandHandlers.Add(attrs[0].Command, command.Handler);
						userCommandAttrs.Add(attrs[0].Command, attrs[0]);
					}
					catch
					{
						Console.WriteLine("Error loading command handler {0}.", t);
					}
				}
			}

			UserCommandHandlers = userCommandHandlers;
			UserCommandAttrs = userCommandAttrs;
		}

		private void InitializeChannelMode(AppDomain ad)
		{
			Dictionary<Modes, ReceivedChannelModeEventHandler> addChannelModeHandlers = new Dictionary<Modes, ReceivedChannelModeEventHandler>();
			Dictionary<Modes, ReceivedChannelModeEventHandler> removeChannelModeHandlers = new Dictionary<Modes, ReceivedChannelModeEventHandler>();
			Dictionary<Modes, ChannelModeAttribute> channelModeAttrs = new Dictionary<Modes, ChannelModeAttribute>();

			foreach (Assembly a in ad.GetAssemblies())
			{
				IEnumerable<Type> types = from x in a.GetTypes()
										  where x.GetInterfaces().Contains(typeof(IChannelMode))
										  select x;

				foreach (Type t in types)
				{
					try
					{
						ChannelModeAttribute[] attrs = t.GetCustomAttributes(typeof(ChannelModeAttribute), false).Cast<ChannelModeAttribute>().ToArray();
						if (attrs.Length < 1 || channelModeAttrs.ContainsKey(attrs[0].Mode))
						{
							continue;
						}

						IChannelMode command = (IChannelMode)t.GetConstructor(Type.EmptyTypes).Invoke(new object[0]);
						addChannelModeHandlers.Add(attrs[0].Mode, command.AddHandler);
						removeChannelModeHandlers.Add(attrs[0].Mode, command.RemoveHandler);
						channelModeAttrs.Add(attrs[0].Mode, attrs[0]);
					}
					catch
					{
						Console.WriteLine("Error loading command handler {0}.", t);
					}
				}
			}

			AddChannelModeHandlers = addChannelModeHandlers;
			RemoveChannelModeHandlers = removeChannelModeHandlers;
			ChannelModeAttrs = channelModeAttrs;
		}
	}
}