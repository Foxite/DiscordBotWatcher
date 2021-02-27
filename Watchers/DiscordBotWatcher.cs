using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace Otocyon {
	public class DiscordBotWatcher : BaseWatcher<ulong> {
		private readonly ILogger<DiscordBotWatcher> m_Logger;
		private readonly DiscordSocketClient m_Discord;
		private readonly Dictionary<ulong, Status> m_WatchedIds;

		public class Config {
			public ICollection<ulong> WatchedUsers { get; set; } = new List<ulong>();
		}

		public DiscordBotWatcher(ILogger<DiscordBotWatcher> logger, DiscordClientService discord, Config config) : base(config.WatchedUsers) {
			m_Logger = logger;
			m_Discord = discord.Client;
			m_WatchedIds = config.WatchedUsers.ToDictionary(id => id, _ => Status.NowOnline);
		}

		public async override IAsyncEnumerable<CheckResult> CheckStatus() {
			foreach (ulong id in m_WatchedIds.Keys.ToArray()) { // ToArray because otherwise there will be a CollectionModifiedException
				m_Logger.LogDebug($"{id} Checking");
				IUser user = await ((IDiscordClient) m_Discord).GetUserAsync(id);
				
				var newStatus = GetNewStatus(id, user.Status == UserStatus.Online);
				
				m_Logger.LogDebug($"{id} {newStatus.ToString()}");
				yield return new CheckResult($"{user.Username}#{user.Discriminator}", id.ToString(), newStatus);
			}
		}
	}
}
