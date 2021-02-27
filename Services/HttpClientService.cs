using System;
using System.Net.Http;

namespace Otocyon {
	public sealed class HttpClientService : IDisposable {
		public HttpClient Client { get; }
		
		public HttpClientService() {
			Client = new HttpClient() {
				DefaultRequestHeaders = {
					{ "User-Agent", $"Otocyon/{Program.Version}" }
				}
			};
		}

		public void Dispose() => Client.Dispose();
	}
}
