using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Aselia.Flags;

namespace Aselia.Modules
{
	public sealed class DomainManager : MarshalByRefObject
	{
		private readonly Server Server;
		private readonly Dictionary<Domains, AppDomain> AppDomains = new Dictionary<Domains, AppDomain>();

		public Dictionary<Commands, ReceivedCommandEventHandler> UserCommandHandlers { get; private set; }

		public Dictionary<Commands, Authorizations> UserCommandLevels { get; private set; }

		public Dictionary<Modes, ReceivedChannelModeEventHandler> AddChannelModeHandlers { get; private set; }

		public Dictionary<Modes, ReceivedChannelModeEventHandler> RemoveChannelModeHandlers { get; private set; }

		public Dictionary<Modes, ChannelModeAttribute> ChannelModeAttrs { get; private set; }

		public DomainManager(Server server)
		{
			Server = server;
		}

		public void Reload()
		{
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

			Initialize(domain, ad);

			AppDomain remove = AppDomains.ContainsKey(domain) ? AppDomains[domain] : null;
			AppDomains[domain] = ad;
			if (remove != null)
			{
				AppDomain.Unload(remove);
			}
		}

		private void Initialize(Domains domain, AppDomain ad)
		{
			switch (domain)
			{
			case Domains.UserCommands:
				InitializeUserCommand(ad);
				break;
			}
		}

		private void InitializeUserCommand(AppDomain ad)
		{
			Dictionary<Commands, ReceivedCommandEventHandler> userCommandHandlers = new Dictionary<Commands, ReceivedCommandEventHandler>();
			Dictionary<Commands, Authorizations> userCommandLevels = new Dictionary<Commands, Authorizations>();

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
						userCommandLevels.Add(attrs[0].Command, attrs[0].Level);
					}
					catch
					{
						Console.WriteLine("Error loading command handler {0}.", t);
					}
				}
			}

			UserCommandHandlers = userCommandHandlers;
			UserCommandLevels = userCommandLevels;
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