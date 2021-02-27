using System.Collections.Generic;
using System.Linq;

namespace Otocyon {
	public abstract class Watcher {
		public abstract IAsyncEnumerable<CheckResult> CheckStatus();
	}

	public abstract class BaseWatcher<TKey> : Watcher where TKey : notnull {
		private readonly Dictionary<TKey, Status> m_WatchedTargets;
		
		protected BaseWatcher(IEnumerable<TKey> keys) {
			m_WatchedTargets = keys.ToDictionary(key => key, _ => Status.NowOnline);
		}

		protected Status GetNewStatus(TKey key, bool isCurrentlyOnline) {
			Status newStatus;
			if (isCurrentlyOnline) {
				newStatus =
					m_WatchedTargets[key] is Status.NowOffline or Status.StillOffline
						? Status.NowOnline
						: Status.StillOnline;
			} else {
				newStatus =
					m_WatchedTargets[key] is Status.NowOffline or Status.StillOffline
						? Status.StillOffline
						: Status.NowOffline;
			}
			m_WatchedTargets[key] = newStatus;
			return newStatus;
		}
	}
}