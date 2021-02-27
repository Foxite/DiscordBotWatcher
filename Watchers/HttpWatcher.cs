using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace Otocyon {
	public class HttpWatcher : BaseWatcher<string> {
		private readonly ILogger<HttpWatcher> m_Logger;
		private readonly Config m_Config;
		private readonly HttpClient m_Client;

		public class Config {
			// Want to make this (and all other Config classes in the project) a record, but Configuration flips its lid when you do that.
			// Funny thing is, the documentation speaks as if it IS possible, as they refer to an example in which a record type is used with Configuration.
			// However, the example that they refer to, actually uses a class, not a record.
			// https://docs.microsoft.com/en-us/dotnet/core/extensions/configuration-providers
			public ICollection<string> Targets { get; set; } = new List<string>();
		}

		public HttpWatcher(ILogger<HttpWatcher> logger, Config config, HttpClientService wcs) : base(config.Targets) {
			m_Logger = logger;
			m_Config = config;
			m_Client = wcs.Client;
		}
		
		public async override IAsyncEnumerable<CheckResult> CheckStatus() {
			foreach (string address in m_Config.Targets) {
				m_Logger.LogDebug($"{address} Checking");
				HttpResponseMessage result = await m_Client.GetAsync(address);

				var newStatus = GetNewStatus(address, result.IsSuccessStatusCode);
				
				result.Dispose();
				m_Logger.LogDebug($"{address} {newStatus.ToString()}");
				yield return new CheckResult(address, address, newStatus);
			}
		}
	}
}
