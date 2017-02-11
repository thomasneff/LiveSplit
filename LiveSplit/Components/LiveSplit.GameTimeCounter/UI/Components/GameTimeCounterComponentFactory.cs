using LiveSplit.Model;
using System;

namespace LiveSplit.UI.Components
{
	public class GameTimeCounterComponentFactory : IComponentFactory
	{
		#region Public Properties

		public ComponentCategory Category => ComponentCategory.Other;
		public string ComponentName => "GameTimeCounter";

		public string Description => "A Counter, using the Game Time of each split to count up/down. Can be used to e.g. track random encounters per split.";
		public string UpdateName => ComponentName;

		public string UpdateURL => "http://livesplit.org/update/";

		public Version Version => Version.Parse("1.0");

		public string XMLURL => "http://livesplit.org/update/Components/update.LiveSplit.Counter.xml";

		#endregion Public Properties

		#region Public Methods

		public IComponent Create(LiveSplitState state) => new GameTimeCounterComponent(state);

		#endregion Public Methods
	}
}