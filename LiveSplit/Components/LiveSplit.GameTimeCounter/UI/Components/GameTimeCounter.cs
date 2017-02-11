namespace LiveSplit.UI.Components
{
	public interface IGameTimeCounter
	{
		#region Public Properties

		int Count { get; }

		#endregion Public Properties

		#region Public Methods

		bool Decrement();

		bool Increment();

		void Reset();

		void SetCount(int value);

		void SetIncrement(int incrementValue);

		#endregion Public Methods
	}

	public class GameTimeCounter : IGameTimeCounter
	{
		#region Private Fields

		private int increment = 1;
        private int initialValue = 0;

		#endregion Private Fields

		#region Public Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="GameTimeCounter"/> class.
		/// </summary>
		/// <param name="initialValue">The initial value for the counter.</param>
		/// <param name="increment">The amount to be used for incrementing/decrementing.</param>
		public GameTimeCounter(int initialValue = 0, int increment = 1)
        {
            this.initialValue = initialValue;
            this.increment = increment;
            Count = initialValue;
		}

		#endregion Public Constructors

		#region Public Properties

		public int Count { get; private set; }

		#endregion Public Properties

		#region Public Methods

		/// <summary>
		/// Decrements this instance.
		/// </summary>
		public bool Decrement()
		{
			if (Count == int.MinValue)
				return false;

			try
			{
				Count = checked(Count - increment);
			}
			catch (System.OverflowException)
			{
				Count = int.MinValue;
				return false;
			}

			return true;
		}

		/// <summary>
		/// Increments this instance.
		/// </summary>
		public bool Increment()
        {
            if (Count == int.MaxValue)
                return false;

            try
            {
                Count = checked(Count + increment);
            }
            catch (System.OverflowException)
            {
                Count = int.MaxValue;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            Count = initialValue;
        }

        /// <summary>
        /// Sets the count.
        /// </summary>
        public void SetCount(int value)
        {
            Count = value;
        }

        /// <summary>
        /// Sets the value that the counter is incremented by.
        /// </summary>
        /// <param name="incrementValue"></param>
        public void SetIncrement(int incrementValue)
        {
            increment = incrementValue;
		}

		#endregion Public Methods
	}
}