using System.Collections.Generic;
using System.Threading.Tasks;

namespace Otocyon {
	public static class LinqExtensions {
		public static async IAsyncEnumerable<T> AwaitItems<T>(this IEnumerable<Task<T>> source) {
			using var enumerator = source.GetEnumerator();
			while (enumerator.MoveNext()) {
				yield return await enumerator.Current;
			}
		} 
	}
}