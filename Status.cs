namespace Otocyon {
	public enum Status {
		/// <summary>
		/// During the current check, the target has been found offline, while the previous check indicated it was online.
		/// </summary>
		NowOffline,
		
		/// <summary>
		/// The previous and current checks have both found the target to be offline.
		/// An alert will be issued when a target reaches this status.
		/// </summary>
		StillOffline,
		
		/// <summary>
		/// During the current check, the target has been found online, while the previous check indicated it was offline.
		/// </summary>
		NowOnline,
		
		StillOnline
	}
}