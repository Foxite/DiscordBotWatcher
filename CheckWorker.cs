using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace Otocyon {
	public class CheckWorker : IHostedService {
		private readonly ILogger<CheckWorker> m_Logger;
		private readonly ICollection<Watcher> m_Watchers = new List<Watcher>();
		private readonly ICollection<Alerter> m_Alerters = new List<Alerter>();
		private readonly Timer m_Timer;
		
		public CheckWorker(IHostEnvironment env, ILogger<CheckWorker> logger, DiscordBotWatcher discordWatcher, HttpWatcher httpWatcher, DiscordAlerter discordAlerter) {
			m_Logger = logger;
			m_Watchers.Add(discordWatcher);
			m_Watchers.Add(httpWatcher);
			m_Alerters.Add(discordAlerter);

			m_Timer = new Timer() {
				Enabled = false,
				AutoReset = false,
				Interval =
					env.IsDevelopment() 
						? TimeSpan.FromSeconds(10).TotalMilliseconds
						: TimeSpan.FromMinutes(10).TotalMilliseconds
			};
		}

		public Task StartAsync(CancellationToken cancellationToken) {
			int consecutiveFailures = 0;
			
			m_Timer.Elapsed += (_, _) => {
				try {
					CheckRoutine();
					consecutiveFailures = 0;
				} catch (Exception ex) {
					consecutiveFailures++;
					m_Logger.LogError(ex, $"Error {consecutiveFailures} during check routine");
				} finally {
					if (consecutiveFailures < 3) {
						m_Timer.Start();
					} else {
						m_Logger.LogCritical("Too many consecutive errors, restarting");
						
					}
				}
			};

			m_Timer.Start();
			
			return Task.CompletedTask;
		}
		
		public Task StopAsync(CancellationToken cancellationToken) {
			m_Timer.Stop();
			return Task.CompletedTask;
		}
		
		private void CheckRoutine() => Task.Run(async () => {
			foreach (Watcher watcher in m_Watchers) {
				await foreach (CheckResult result in watcher.CheckStatus()) {
					foreach (Alerter alerter in m_Alerters) {
						await alerter.IssueAlert(result.Identifier, result.Status);
					}
				}
			}
		}).GetAwaiter().GetResult();
	}
}