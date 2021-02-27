using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Otocyon {
	public class DiscordAlerter : Alerter {
		private readonly DiscordSocketClient m_Client;
		private readonly Config m_Config;

		public class Config {
			public ICollection<ulong> ChannelTargets { get; set; } = new List<ulong>();
			public ICollection<ulong> UserTargets { get; set; } = new List<ulong>();
		}

		public DiscordAlerter(DiscordClientService client, Config config) {
			m_Client = client.Client;
			m_Config = config;
		}

		public async override Task IssueAlert(string identifier, Status status) {
			// It's a mess, but it's my mess and I think it's beautiful.
			await foreach (IMessageChannel channel in
				(
					from id in m_Config.ChannelTargets
					select Task.FromResult((IMessageChannel) m_Client.GetChannel(id))
				).Concat(
					from id in m_Config.UserTargets
					select m_Client.GetUser(id).GetOrCreateDMChannelAsync().ContinueWith(t => (IMessageChannel) t.Result)
				).AwaitItems()
			) {
				await (status switch {
					Status.NowOffline => channel.SendMessageAsync($"ALERT: {identifier} has gone offline."),
					Status.NowOnline => channel.SendMessageAsync($"{identifier} is now online."),
					_ => Task.CompletedTask
				});
			}
		}
	}
}
