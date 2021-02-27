using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Otocyon {
	public static class Program {
		public static Version Version => new Version(0, 2, 0);

		private static void Main() {
			IHost host = new HostBuilder()
				.ConfigureHostConfiguration(config =>
					config
						.SetBasePath(Directory.GetCurrentDirectory())
						.AddJsonFile("appsettings.json")
				)
				.ConfigureLogging(logging =>
					logging
						.AddConsole()
						.AddSystemdConsole()
				)
				.ConfigureServices((hbc, isc) =>
					isc
						.AddHostedService<CheckWorker>()
						// TODO User must be able to specify which watchers/alerters get loaded via config file.
						// I'd like to keep it in a single config file, appsettings.json should be perfect for this.
						// If a component is specified to be listed its configuration should automatically be loaded.
						.AddSingleton(hbc.Configuration.GetSection(nameof(DiscordClientService)).Get<DiscordClientService.Config>())
						.AddSingleton(hbc.Configuration.GetSection(nameof(DiscordBotWatcher)).Get<DiscordBotWatcher.Config>())
						.AddSingleton(hbc.Configuration.GetSection(nameof(DiscordAlerter)).Get<DiscordAlerter.Config>())
						.AddSingleton(hbc.Configuration.GetSection(nameof(HttpWatcher)).Get<HttpWatcher.Config>())
						.AddSingleton<DiscordClientService>()
						.AddSingleton<HttpClientService>()
						.AddSingleton<DiscordBotWatcher>()
						.AddSingleton<HttpWatcher>()
						.AddSingleton<DiscordAlerter>()
				)
				.Build();

			try {
				host.Start();
				host.WaitForShutdown();
			} finally {
				host.Dispose();
			}
		}
	}
}
