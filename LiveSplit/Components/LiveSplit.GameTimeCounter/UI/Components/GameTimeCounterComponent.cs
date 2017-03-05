using LiveSplit.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using LiveSplit.Model.Input;
using System.Linq;
using System.IO;

namespace LiveSplit.UI.Components
{
	public class GameTimeCounterComponent : IComponent
	{
		#region Public Fields

		public List<IGameTimeCounter> Counters;

		public ISegment PBSplit;

		public bool Redraw = false;

		#endregion Public Fields

		#region Protected Fields

		protected SimpleLabel CounterNameLabel = new SimpleLabel();

		protected SimpleLabel CounterValueLabel = new SimpleLabel();

		protected SimpleLabel TotalCounterNameLabel = new SimpleLabel();

		protected SimpleLabel TotalCounterValueLabel = new SimpleLabel();

		#endregion Protected Fields

		#region Private Fields

		private LiveSplitState state;

		#endregion Private Fields

		#region Public Constructors

		public GameTimeCounterComponent(LiveSplitState state)
		{
			VerticalHeight = 10;
			Settings = new GameTimeCounterComponentSettings();
			Cache = new GraphicsCache();
			CounterNameLabel = new SimpleLabel();
			TotalCounterNameLabel = new SimpleLabel();
			Counters = new List<IGameTimeCounter>();

			this.state = state;
			Settings.CounterReinitialiseRequired += Settings_CounterReinitialiseRequired;
			Settings.IncrementUpdateRequired += Settings_IncrementUpdateRequired;
			
			// Subscribe to input hooks.
			Settings.Hook.KeyOrButtonPressed += hook_KeyOrButtonPressed;
			Settings.GetCounterTextBox().TextChanged += GameTimeCounterComponent_TextChanged;
			Initialized = false;
			Counters.Clear();

			InitCounterFromState();

			int sum = 0;
			for (int i = 0; i <= state.CurrentSplitIndex && i < Counters.Count; ++i)
			{
				sum += Counters[i].Count;
			}

			if (state.CurrentSplitIndex >= 0 && Counters.Count > state.CurrentSplitIndex)
			{
				CounterValueLabel.Text = Counters[state.CurrentSplitIndex].Count.ToString();
				CounterNameLabel.Text = state.CurrentSplit.Name + ":";
			}
			else
			{
				CounterValueLabel.Text = "0";
				CounterNameLabel.Text = state.Run.First().Name + ":";
			}

			TotalCounterValueLabel.Text = sum.ToString(); //TODO: have counters for PB, store PB
			TotalCounterNameLabel.Text = Settings.CounterText;
			Cache.Restart();
			Cache["CounterNameLabel"] = CounterNameLabel.Text;
			Cache["CounterValueLabel"] = CounterValueLabel.Text;
			Cache["TotalCounterNameLabel"] = TotalCounterNameLabel.Text;
			Cache["TotalCounterValueLabel"] = TotalCounterValueLabel.Text;
		}

		private void GameTimeCounterComponent_TextChanged(object sender, EventArgs e)
		{
			Redraw = true;
		}

		#endregion Public Constructors

		#region Public Properties

		public GraphicsCache Cache { get; set; }

		public string ComponentName
		{
			get { return "GameTimeCounter"; }
		}

		public IDictionary<string, Action> ContextMenuControls
		{
			get { return null; }
		}

		public float HorizontalWidth { get; set; }
		public bool Initialized { get; set; }
		public float MinimumHeight { get; set; }

		public float MinimumWidth
		{
			get
			{
				return CounterNameLabel.X + CounterValueLabel.ActualWidth + TotalCounterNameLabel.ActualWidth + TotalCounterValueLabel.ActualWidth;
			}
		}

		public float PaddingBottom { get; set; }
		public float PaddingLeft { get { return 7f; } }
		public float PaddingRight { get { return 7f; } }
		public float PaddingTop { get; set; }
		public GameTimeCounterComponentSettings Settings { get; set; }
		public float VerticalHeight { get; set; }

		#endregion Public Properties

		#region Protected Properties

		protected Font CounterFont { get; set; }

		#endregion Protected Properties

		#region Public Methods

		public void Dispose()
		{
			Settings.Hook.KeyOrButtonPressed -= hook_KeyOrButtonPressed;
		}

		public void DrawHorizontal(Graphics g, Model.LiveSplitState state, float height, Region clipRegion)
		{
			DrawGeneral(g, state, HorizontalWidth, height, LayoutMode.Horizontal);
		}

		public void DrawVertical(System.Drawing.Graphics g, Model.LiveSplitState state, float width, Region clipRegion)
		{
			DrawGeneral(g, state, width, VerticalHeight, LayoutMode.Vertical);
		}

		public System.Xml.XmlNode GetSettings(System.Xml.XmlDocument document)
		{
			return Settings.GetSettings(document);
		}

		public Control GetSettingsControl(LayoutMode mode)
		{
			return Settings;
		}

		public int GetSettingsHashCode()
		{
			return Settings.GetSettingsHashCode();
		}

		public void SetSettings(System.Xml.XmlNode settings)
		{
			Settings.SetSettings(settings);

			// Initialise Counter from settings.
			// Counter = new Counter(Settings.InitialValue, Settings.Increment);
			//Counters = new List<IGameTimeCounter>();
			Initialized = false;
		}

		int GetAllSecondsFromTimeSpan(TimeSpan? val)
		{
			int ret = 0;

			if (val != null)
			{
				ret = val.Value.Seconds + val.Value.Minutes * 60 + val.Value.Hours * 3600 + val.Value.Days * 86400;
			}

			return ret;
		}

		void InitCounterFromState()
		{
			if(state == null)
			{
				return;
			}
			Counters = new List<IGameTimeCounter>();
			bool beforeCurrentSplit = true;
			TimeSpan? previousSplitTime = default(TimeSpan);
			TimeSpan? splitTime = default(TimeSpan);
			TimeSpan? splitTimeDif = default(TimeSpan);
			foreach (ISegment split in state.Run)
			{
				if(split == state.CurrentSplit || state.CurrentSplitIndex < 0)
				{
					beforeCurrentSplit = false;
				}

				if(beforeCurrentSplit)
				{
					splitTime = split.SplitTime.GameTime;

					if(splitTime != null)
					{
						splitTimeDif = splitTime - previousSplitTime;
					}
					else
					{
						splitTimeDif = splitTime;
					}

					Counters.Add(new GameTimeCounter(GetAllSecondsFromTimeSpan(splitTimeDif), Settings.Increment));
				}
				else
				{
					Counters.Add(new GameTimeCounter(Settings.InitialValue, Settings.Increment));
				}

				if(splitTime != null)
				{
					previousSplitTime = splitTime;
				}
				


			}
		}


		public void Update(IInvalidator invalidator, Model.LiveSplitState state, float width, float height, LayoutMode mode)
		{
			try
			{
				if (Settings.Hook != null)
					Settings.Hook.Poll();
			}
			catch { }

			this.state = state;

			if (state == null)
				return;

			if (Initialized == false)
			{
				Counters.Clear();
				Initialized = true;

				InitCounterFromState();

				state.OnStart += State_OnStart;
				state.OnReset += State_OnReset;
				state.OnSkipSplit += State_OnSkipSplit;
				state.OnSplit += State_OnSplit;
				state.OnSwitchComparisonNext += State_OnSwitchComparisonNext;
				state.OnSwitchComparisonPrevious += State_OnSwitchComparisonPrevious;
				state.OnUndoSplit += State_OnUndoSplit;
				PBSplit = state.Run.Last();
			}
			int sum = 0;

			for (int i = 0; i <= state.CurrentSplitIndex && i < Counters.Count; ++i)
			{
				sum += Counters[i].Count;
			}
			if (CounterNameLabel != null)
			{
				if (state.CurrentSplit != null)
				{
					CounterNameLabel.Text = state.CurrentSplit.Name + ":";
				}
				else
				{
					CounterNameLabel.Text = state.Run.First().Name + ":";
				}
			}

			if (CounterValueLabel != null)
			{
				if (state.CurrentSplitIndex < 0 || state.CurrentSplitIndex >= Counters.Count)
				{
					CounterValueLabel.Text = "0";
				}
				else
				{
					CounterValueLabel.Text = Counters[state.CurrentSplitIndex].Count.ToString();
				}
			}

			if (TotalCounterValueLabel != null)
				TotalCounterValueLabel.Text = sum.ToString(); //TODO: have counters for PB, store PB
			if (TotalCounterNameLabel != null)
				TotalCounterNameLabel.Text = Settings.CounterText;

			TimeSpan? timeChange = null;
			string comparison = "";
			if (state.CurrentSplitIndex >= 0 && state.CurrentSplitIndex < Counters.Count)
			{
				comparison = state.CurrentComparison;
				var splitTime = state.Run[state.CurrentSplitIndex].Comparisons[comparison][TimingMethod.GameTime];
				TimeSpan? previousSplitTime = new TimeSpan(0, 0, 0);
				if (state.CurrentSplitIndex - 1 >= 0)
					previousSplitTime = state.Run[state.CurrentSplitIndex - 1].Comparisons[comparison][TimingMethod.GameTime];

				if(previousSplitTime == null)
				{
					previousSplitTime = new TimeSpan(0, 0, 0);
				}

				timeChange = new TimeSpan(0, 0, Counters[state.CurrentSplitIndex].Count - (GetAllSecondsFromTimeSpan(splitTime) - GetAllSecondsFromTimeSpan(previousSplitTime)));//LiveSplitStateHelper.GetLiveSegmentDelta(state, state.CurrentSplitIndex, comparison, TimingMethod.GameTime);
			}

			if (timeChange != null)
			{
				Color color = Settings.OverrideTextColor ? Settings.CounterTextColor : state.LayoutSettings.TextColor;

				if (GetAllSecondsFromTimeSpan(timeChange) == 0)
				{
				}
				else if (Counters[state.CurrentSplitIndex].Count < GetAllSecondsFromTimeSpan(state.Run[state.CurrentSplitIndex].BestSegmentTime.GameTime))
				{
					//best seg
					if (state.LayoutSettings.ShowBestSegments)
					{
						color = GetBestSegmentColor(state);
					}
				}
				else
				{
					color = LiveSplitStateHelper.GetSplitColor(state, timeChange, state.CurrentSplitIndex, false, false, comparison, TimingMethod.GameTime).Value;
				}

				if (color != CounterValueLabel.ForeColor)
					Redraw = true;

				CounterValueLabel.ForeColor = color;
			}
			else
			{
				var color = Settings.OverrideTextColor ? Settings.CounterTextColor : state.LayoutSettings.TextColor;

				if (color != CounterValueLabel.ForeColor)
					Redraw = true;

				CounterValueLabel.ForeColor = color;
			}

			//TODO: total counter color
			if (state.CurrentSplitIndex >= 0 && state.CurrentSplitIndex < Counters.Count)
			{
				comparison = state.CurrentComparison;
				var splitTime = state.Run[state.CurrentSplitIndex].Comparisons[comparison][TimingMethod.GameTime];
				timeChange = new TimeSpan(0, 0, sum - GetAllSecondsFromTimeSpan(splitTime));//LiveSplitStateHelper.GetLiveSegmentDelta(state, state.CurrentSplitIndex, comparison, TimingMethod.GameTime);
			}

			if (timeChange != null)
			{
				Color color = Settings.OverrideTextColor ? Settings.CounterTextColor : state.LayoutSettings.TextColor;

				if (GetAllSecondsFromTimeSpan(timeChange) == 0)
				{
				}
				else
				{
					color = LiveSplitStateHelper.GetSplitColor(state, timeChange, state.CurrentSplitIndex, false, false, comparison, TimingMethod.GameTime).Value;
				}

				if (color != TotalCounterValueLabel.ForeColor)
					Redraw = true;

				TotalCounterValueLabel.ForeColor = color;
			}
			else
			{
				var color = Settings.OverrideTextColor ? Settings.CounterTextColor : state.LayoutSettings.TextColor;

				if (color != TotalCounterValueLabel.ForeColor)
					Redraw = true;

				TotalCounterValueLabel.ForeColor = color;
			}

			Cache.Restart();
			Cache["CounterNameLabel"] = CounterNameLabel.Text;
			Cache["CounterValueLabel"] = CounterValueLabel.Text;
			Cache["TotalCounterNameLabel"] = TotalCounterNameLabel.Text;
			Cache["TotalCounterValueLabel"] = TotalCounterValueLabel.Text;

			if (invalidator != null && (Cache.HasChanged || Redraw))
			{
				Redraw = false;
				invalidator.Invalidate(0, 0, width, height);
			}
		}

		

		private void State_OnSwitchComparisonPrevious(object sender, EventArgs e)
		{
			Redraw = true;
		}

		private void State_OnSwitchComparisonNext(object sender, EventArgs e)
		{
			Redraw = true;
		}

		private Color GetBestSegmentColor(LiveSplitState state)
		{
			if (state.LayoutSettings.UseRainbowColor)
			{
				var hue = (((int)DateTime.Now.TimeOfDay.TotalMilliseconds / 100) % 36) * 10;
				var rainbowColor = ColorExtensions.FromHSV(hue, 1, 1);
				return Color.FromArgb((rainbowColor.R * 2 + 255 * 1) / 3, (rainbowColor.G * 2 + 255 * 1) / 3, (rainbowColor.B * 2 + 255 * 1) / 3);
			}

			return state.LayoutSettings.BestSegmentColor;
		}

		#endregion Public Methods

		#region Private Methods

		private void DrawGeneral(Graphics g, Model.LiveSplitState state, float width, float height, LayoutMode mode)
		{
			// Set Background colour.
			if (Settings.BackgroundColor.ToArgb() != Color.Transparent.ToArgb()
				|| Settings.BackgroundGradient != GradientType.Plain
				&& Settings.BackgroundColor2.ToArgb() != Color.Transparent.ToArgb())
			{
				var gradientBrush = new LinearGradientBrush(
							new PointF(0, 0),
							Settings.BackgroundGradient == GradientType.Horizontal
							? new PointF(width, 0)
							: new PointF(0, height),
							Settings.BackgroundColor,
							Settings.BackgroundGradient == GradientType.Plain
							? Settings.BackgroundColor
							: Settings.BackgroundColor2);

				g.FillRectangle(gradientBrush, 0, 0, width, height);
			}

			// Set Font.
			CounterFont = Settings.OverrideCounterFont ? Settings.CounterFont : state.LayoutSettings.TextFont;

			// Calculate Height from Font.
			var textHeight = g.MeasureString("A", CounterFont).Height;
			VerticalHeight = 1.2f * textHeight;
			MinimumHeight = MinimumHeight;

			PaddingTop = Math.Max(0, ((VerticalHeight - 0.75f * textHeight) / 2f));
			PaddingBottom = PaddingTop;

			// Assume most users won't count past four digits (will cause a layout resize in Horizontal Mode).
			float fourCharWidth = g.MeasureString("1000", CounterFont).Width;
			HorizontalWidth = CounterNameLabel.X + CounterNameLabel.ActualWidth + TotalCounterNameLabel.ActualWidth + TotalCounterValueLabel.ActualWidth + (fourCharWidth > CounterValueLabel.ActualWidth ? fourCharWidth : CounterValueLabel.ActualWidth) + 5;

			// Set Counter Name Label
			CounterNameLabel.HorizontalAlignment = mode == LayoutMode.Horizontal ? StringAlignment.Near : StringAlignment.Near;
			CounterNameLabel.VerticalAlignment = StringAlignment.Center;
			CounterNameLabel.X = 5;
			CounterNameLabel.Y = 0;
			CounterNameLabel.Width = (width / 2 - fourCharWidth - 5);
			CounterNameLabel.Height = height;
			CounterNameLabel.Font = CounterFont;
			CounterNameLabel.Brush = new SolidBrush(Settings.OverrideTextColor ? Settings.CounterTextColor : state.LayoutSettings.TextColor);
			CounterNameLabel.HasShadow = state.LayoutSettings.DropShadows;
			CounterNameLabel.ShadowColor = state.LayoutSettings.ShadowsColor;
			CounterNameLabel.Draw(g);

			// Set Counter Name Label
			TotalCounterNameLabel.HorizontalAlignment = mode == LayoutMode.Horizontal ? StringAlignment.Far : StringAlignment.Far;
			TotalCounterNameLabel.VerticalAlignment = StringAlignment.Center;
			TotalCounterNameLabel.X = TotalCounterNameLabel.ActualWidth + fourCharWidth;
			TotalCounterNameLabel.Y = 0;
			TotalCounterNameLabel.Width = (width / 2 - fourCharWidth - 5);//(width - fourCharWidth - 5);
			TotalCounterNameLabel.Height = height;
			TotalCounterNameLabel.Font = CounterFont;
			TotalCounterNameLabel.Brush = new SolidBrush(Settings.OverrideTextColor ? Settings.CounterTextColor : state.LayoutSettings.TextColor);
			TotalCounterNameLabel.HasShadow = state.LayoutSettings.DropShadows;
			TotalCounterNameLabel.ShadowColor = state.LayoutSettings.ShadowsColor;
			TotalCounterNameLabel.Draw(g);

			// Set Counter Value Label.
			CounterValueLabel.HorizontalAlignment = mode == LayoutMode.Horizontal ? StringAlignment.Near : StringAlignment.Near;
			CounterValueLabel.VerticalAlignment = StringAlignment.Center;
			CounterValueLabel.X = width / 2 - fourCharWidth;
			CounterValueLabel.Y = 0;
			CounterValueLabel.Width = width / 2 - 10;//(width - 10);
			CounterValueLabel.Height = height;
			CounterValueLabel.Font = CounterFont;
			CounterValueLabel.Brush = new SolidBrush(CounterValueLabel.ForeColor);//new SolidBrush(Settings.OverrideTextColor ? Settings.CounterValueColor : state.LayoutSettings.TextColor);
			CounterValueLabel.HasShadow = state.LayoutSettings.DropShadows;
			CounterValueLabel.ShadowColor = state.LayoutSettings.ShadowsColor;
			CounterValueLabel.Draw(g);

			// Set Counter Value Label.
			TotalCounterValueLabel.HorizontalAlignment = mode == LayoutMode.Horizontal ? StringAlignment.Far : StringAlignment.Far;
			TotalCounterValueLabel.VerticalAlignment = StringAlignment.Center;
			TotalCounterValueLabel.X = 5;
			TotalCounterValueLabel.Y = 0;
			TotalCounterValueLabel.Width = (width - 10);//(width - 10);
			TotalCounterValueLabel.Height = height;
			TotalCounterValueLabel.Font = CounterFont;
			TotalCounterValueLabel.Brush = new SolidBrush(TotalCounterValueLabel.ForeColor);//new SolidBrush(Settings.OverrideTextColor ? Settings.CounterValueColor : state.LayoutSettings.TextColor);
			TotalCounterValueLabel.HasShadow = state.LayoutSettings.DropShadows;
			TotalCounterValueLabel.ShadowColor = state.LayoutSettings.ShadowsColor;
			TotalCounterValueLabel.Draw(g);
		}

		// Basic support for keyboard/button input.
		private void hook_KeyOrButtonPressed(object sender, KeyOrButton e)
		{
			if ((Form.ActiveForm == state.Form && !Settings.GlobalHotkeysEnabled)
				|| Settings.GlobalHotkeysEnabled)
			{
				if (Counters == null)
				{
					//initialize counters with run
					InitCounterFromState();
				}

				if (state.CurrentSplitIndex >= Counters.Count)
					return;

				if (state.CurrentSplitIndex < 0)
					return;

				if (e == Settings.IncrementKey)
					Counters[state.CurrentSplitIndex].Increment();

				if (e == Settings.DecrementKey)
					Counters[state.CurrentSplitIndex].Decrement();

				if (e == Settings.ResetKey)
				{
					Counters[state.CurrentSplitIndex].Reset();
				}

				int sum = 0;

				for (int i = 0; i < state.CurrentSplitIndex; ++i)
				{
					if (i >= Counters.Count)
						continue;
					sum += Counters[i].Count;
				}

				Time gametime = new Time(state.Run[state.CurrentSplitIndex].SplitTime.RealTime, new TimeSpan(0, 0, sum));
				state.Run[state.CurrentSplitIndex].SplitTime = gametime;
			}
		}

		/// <summary>
		/// Handles the CounterReinitialiseRequired event of the Settings control.
		/// </summary>
		private void Settings_CounterReinitialiseRequired(object sender, EventArgs e)
		{
			//Counter = new Counter(Settings.InitialValue, Settings.Increment);
			Counters = new List<IGameTimeCounter>();
			Initialized = false;
		}

		private void Settings_IncrementUpdateRequired(object sender, EventArgs e)
		{
			foreach (IGameTimeCounter c in Counters)
				c.SetIncrement(Settings.Increment);
		}

		private void State_OnReset(object sender, TimerPhase value)
		{
			//throw new NotImplementedException();

			//
			Initialized = false;
		}

		private void State_OnSkipSplit(object sender, EventArgs e)
		{
			//throw new NotImplementedException();

			state.Run[state.CurrentSplitIndex].SplitTime = default(Time);
		}

		private void State_OnUndoSplit(object sender, EventArgs e)
		{
			//clear counter
			if(state.CurrentSplitIndex + 1 < Counters.Count)
				Counters[state.CurrentSplitIndex + 1] = new GameTimeCounter();
		}

		private void State_OnSplit(object sender, EventArgs e)
		{
			//throw new NotImplementedException();

			int sum = 0;


			if (state == null)
			{
				return;
			}

			if(state.CurrentSplitIndex < 1 || state.CurrentSplitIndex > Counters.Count)
			{
				return;
			}

			sum = Counters[state.CurrentSplitIndex - 1].Count;
			for (int idx = state.CurrentSplitIndex - 2; idx >= 0; idx--)
			{
				TimeSpan? splitTime = state.Run[idx].SplitTime.GameTime;
				if (splitTime == null)
				{
					continue;
				}

				sum += GetAllSecondsFromTimeSpan(splitTime);
				break;
			}


			Time gametime = new Time(state.Run[state.CurrentSplitIndex - 1].SplitTime.RealTime, new TimeSpan(0, 0, sum));
			state.Run[state.CurrentSplitIndex - 1].SplitTime = gametime;
		}

		private void State_OnStart(object sender, EventArgs e)
		{
			//throw new NotImplementedException();
			Initialized = false;
		}

		#endregion Private Methods
	}
}