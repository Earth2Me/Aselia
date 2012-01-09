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

		public bool Alive { get; set; }

		public Dictionary<string, ReceivedCommandEventHandler> UserCommandHandlers { get; private set; }

		public Dictionary<string, CommandAttribute> UserCommandAttrs { get; private set; }

		public Dictionary<Modes, ReceivedChannelModeEventHandler> AddChannelModeHandlers { get; private set; }

		public Dictionary<Modes, ReceivedChannelModeEventHandler> RemoveChannelModeHandlers { get; private set; }

		public Dictionary<Modes, ChannelModeAttribute> ChannelModeAttrs { get; private set; }

		public DomainManager()
		{
			Alive = true;
		}

		public void Reload()
		{
			Reload(Domains.UserCommands);
			Reload(Domains.ChannelModes);
			Reload(Domains.Core);
		}

		public void Reload(Domains domain)
		{
			FileInfo file;
#if DEBUG
			AppDomain ad = AppDomain.CurrentDomain;
			string name;
			switch (domain)
			{
			case Domains.Core:
				name = "Aselia.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=4c128b1c5b34b028";
				break;

			case Domains.UserCommands:
				name = "Aselia.UserCommands, Version=1.0.0.0, Culture=neutral, PublicKeyToken=c6a0b92f47071a54";
				break;

			case Domains.ChannelModes:
				name = "Aselia.ChannelModes, Version=1.0.0.0, Culture=neutral, PublicKeyToken=cf0f7bf703229a1e";
				break;

			default:
				throw new Exception("Unknown domain specified: " + domain);
			}

			Assembly asm = Assembly.Load(name);
#else
			string name = Enum.GetName(typeof(Domains), domain);
			AppDomain ad = AppDomain.CreateDomain(name);

			byte[] assembly;
			file = new FileInfo("Aselia." + name + ".dll");
			using (FileStream fs = file.OpenRead())
			{
				assembly = new byte[fs.Length];
				fs.Read(assembly, 0, assembly.Length);
			}

			Assembly asm = null;
			ad.Load(assembly);
			foreach (Assembly a in ad.GetAssemblies())
			{
				if (a.FullName.Contains("Aselia"))
				{
					asm = a;
					break;
				}
			}
			if (asm == null)
			{
				throw new Exception("Could not find valid Aselia assembly in DLL.");
			}
#endif

			try
			{
				Initialize(domain, asm);
				Console.WriteLine("Loaded {0}.", asm.FullName);
			}
			catch (Exception ex)
			{
#if !DEBUG
				try
				{
					AppDomain.Unload(ad);
				}
				catch
				{
				}
#endif
				throw new Exception("Error initializing new domain.", ex);
			}

#if !DEBUG
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
#endif
		}

		private void Initialize(Domains domain, Assembly asm)
		{
			switch (domain)
			{
			case Domains.Core:
				InitializeCore(asm);
				break;

			case Domains.UserCommands:
				InitializeUserCommand(asm);
				break;

			case Domains.ChannelModes:
				InitializeChannelMode(asm);
				break;
			}
		}

		private void InitializeCore(Assembly asm)
		{
			Type[] types = (from x in asm.GetTypes()
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

		private void InitializeUserCommand(Assembly asm)
		{
			Dictionary<string, ReceivedCommandEventHandler> userCommandHandlers = new Dictionary<string, ReceivedCommandEventHandler>();
			Dictionary<string, CommandAttribute> userCommandAttrs = new Dictionary<string, CommandAttribute>();

			IEnumerable<Type> types = from x in asm.GetTypes()
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

			UserCommandHandlers = userCommandHandlers;
			UserCommandAttrs = userCommandAttrs;
		}

		private void InitializeChannelMode(Assembly asm)
		{
			Dictionary<Modes, ReceivedChannelModeEventHandler> addChannelModeHandlers = new Dictionary<Modes, ReceivedChannelModeEventHandler>();
			Dictionary<Modes, ReceivedChannelModeEventHandler> removeChannelModeHandlers = new Dictionary<Modes, ReceivedChannelModeEventHandler>();
			Dictionary<Modes, ChannelModeAttribute> channelModeAttrs = new Dictionary<Modes, ChannelModeAttribute>();

			IEnumerable<Type> types = from x in asm.GetTypes()
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

			AddChannelModeHandlers = addChannelModeHandlers;
			RemoveChannelModeHandlers = removeChannelModeHandlers;
			ChannelModeAttrs = channelModeAttrs;
		}
	}
}