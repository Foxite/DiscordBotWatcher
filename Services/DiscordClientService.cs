using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace Otocyon {
	public sealed class DiscordClientService : IAsyncDisposable {
		public DiscordSocketClient Client { get; }

		public class Config {
			public string Token { get; set; } = "";
		}

		public DiscordClientService(ILogger<DiscordClientService> logger, Config config) {
			Client = new DiscordSocketClient(new DiscordSocketConfig() {
				AlwaysDownloadUsers = true,
				// TODO we're able to subscribe to status changes by doing this:
				//GatewayIntents = GatewayIntents.GuildPresences
				// There needs to be a mechanism for watchers to issue alerts at any time, rather than via a routine check.
			});
			Client.Log += msg => {
				logger.Log(msg.Severity switch {
					LogSeverity.Verbose => LogLevel.Trace,
					LogSeverity.Debug => LogLevel.Debug,
					LogSeverity.Info => LogLevel.Information,
					LogSeverity.Warning => LogLevel.Warning,
					LogSeverity.Error => LogLevel.Error,
					LogSeverity.Critical => LogLevel.Critical,
					_ => throw new ArgumentOutOfRangeException(nameof(msg), "Message severity")
				}, $"{msg.Source} {msg.Message} {msg.Exception}");
				return Task.CompletedTask;
			};
			ManualResetEvent ready = new ManualResetEvent(false);

			Task ReadyHandler() {
				ready.Set();
				Client.Ready -= ReadyHandler; // Unsubscribe self
				return Task.CompletedTask;
			}
			Client.Ready += ReadyHandler;
			
			Task.Run(async () => {
				await Client.LoginAsync(TokenType.Bot, config.Token);
				await Client.StartAsync();
			}).GetAwaiter().GetResult();
			ready.WaitOne();
		}

		public async ValueTask DisposeAsync() {
			var loggedOut = new ManualResetEvent(false);

			Task LoggedOutHandler() {
				loggedOut.Set();
				Client.LoggedOut -= LoggedOutHandler;
				return Task.CompletedTask;
			}
			Client.LoggedOut += LoggedOutHandler;
			await Client.LogoutAsync();
			Client.Dispose();
			loggedOut.WaitOne();
		}
	}
}