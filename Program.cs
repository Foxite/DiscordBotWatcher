using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace BotWatcher {
	class Program {
		private static Dictionary<ulong, Status> m_WatchedIds;
		private static Timer m_Timer;
		private static Config m_Config;
		private static DiscordSocketClient m_Client;

		public class Config {
			public ulong[] Targets { get; set; }
			public string Token { get; set; }
			public ulong AlertChannel { get; set; }
		}

		public enum Status {
			LastOnline, LastOffline, AlertIssued
		}

		static void Main(string[] args) {
			m_Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));

			m_WatchedIds = m_Config.Targets.ToDictionary(id => id, _ => Status.LastOnline);

			m_Timer = new Timer() {
				Enabled = true,
				AutoReset = true,
				Interval =
				/*
				TimeSpan.FromSeconds(5).TotalMilliseconds
				/*/
				TimeSpan.FromMinutes(1).TotalMilliseconds,
				//*/
			};
			m_Timer.Elapsed += (o, e) => CheckUsers();

			MainAsync().GetAwaiter().GetResult();
		}

		private static async Task MainAsync() {
			m_Client = new DiscordSocketClient(new DiscordSocketConfig() {
				AlwaysDownloadUsers = true
			});
			m_Client.Log += msg => {
				Log($"{msg.Source} {msg.Severity} {msg.Message} {msg.Exception}");
				return Task.CompletedTask;
			};
			await m_Client.LoginAsync(TokenType.Bot, m_Config.Token);
			await m_Client.StartAsync();
			Log("Running");
			while (Console.ReadLine() != "quit") { }
			Log("Stopping");
			await m_Client.StopAsync();
			await m_Client.LogoutAsync();
		}

		private static void CheckUsers() {
			Log("Check");
			foreach (ulong id in m_WatchedIds.Keys.ToList()) {
				Log($"Check {id}");
				SocketUser user = m_Client.GetUser(id);
				if (user.Status == UserStatus.Offline) {
					if (m_WatchedIds[id] == Status.LastOffline) {
						Log($"{user.Username} Issued alert");
						((SocketTextChannel) m_Client.GetChannel(m_Config.AlertChannel)).SendMessageAsync($"ALERT: {id} {m_Client.GetUser(id).Username} has been offline for 60 seconds.");
						m_WatchedIds[id] = Status.AlertIssued;
					} else if (m_WatchedIds[id] == Status.LastOnline) {
						Log($"{user.Username} Offline");
						m_WatchedIds[id] = Status.LastOffline;
					}
				} else if (m_WatchedIds[id] != Status.LastOnline) {
					Log($"{user.Username} No longer offline");
					((SocketTextChannel) m_Client.GetChannel(m_Config.AlertChannel)).SendMessageAsync($"{id} {m_Client.GetUser(id).Username} is now online.");
					m_WatchedIds[id] = Status.LastOnline;
				}
			}
		}

		private static void Log(string v) {
			Console.WriteLine($"{DateTime.Now:u} {v}");
		}
	}
}
