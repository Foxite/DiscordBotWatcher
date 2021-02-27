using System.Threading.Tasks;

namespace Otocyon {
	public abstract class Alerter {
		public abstract Task IssueAlert(string identifier, Status status);
	}
}