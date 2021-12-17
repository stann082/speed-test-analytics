namespace domain
{
    public class RunFrequencyParser
    {

        #region Constants

        public const int DEFAULT_RUN_TIME_MILLISECONDS = 60 * 60 * 1000;

        #endregion

        #region Public Methods

        public int Parse(string rawValue)
        {
            string[] splitRawValue = rawValue.Split(' ');
            if (splitRawValue.Length < 2 && splitRawValue.Length % 2 != 0)
            {
                return DEFAULT_RUN_TIME_MILLISECONDS;
            }

            DateTime dateTime = DateTime.Today;

            for (int i = 0; i < splitRawValue.Length; i += 2)
            {
                int timeValue = int.TryParse(splitRawValue[i], out int time) ? time : 0;
                string timeUnit = i < splitRawValue.Length - 1 ? splitRawValue[i + 1] : string.Empty;
                if (timeValue == 0 || string.IsNullOrEmpty(timeUnit))
                {
                    continue;
                }

                if (timeUnit.Contains("hour"))
                {
                    dateTime = dateTime.AddHours(timeValue);
                }

                if (timeUnit.Contains("minute"))
                {
                    dateTime = dateTime.AddMinutes(timeValue);
                }

                if (timeUnit.Contains("second"))
                {
                    dateTime = dateTime.AddSeconds(timeValue);
                }
            }

            TimeSpan ts = dateTime.TimeOfDay;
            return (int)ts.TotalMilliseconds;
        }

        #endregion

    }
}
