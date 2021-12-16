namespace domain
{
    public static class EnvironmentVariableProvider
    {

        #region Public Methods

        public static string GetEnvironmentVariable(string environmentVariableKey)
        {
            string environmentVariable = Environment.GetEnvironmentVariable(environmentVariableKey, EnvironmentVariableTarget.Machine);
            if (!string.IsNullOrEmpty(environmentVariable))
            {
                return environmentVariable;
            }

            environmentVariable = Environment.GetEnvironmentVariable(environmentVariableKey, EnvironmentVariableTarget.User);
            if (!string.IsNullOrEmpty(environmentVariable))
            {
                return environmentVariable;
            }

            environmentVariable = Environment.GetEnvironmentVariable(environmentVariableKey, EnvironmentVariableTarget.Process);
            if (!string.IsNullOrEmpty(environmentVariable))
            {
                return environmentVariable;
            }

            return string.Empty;
        }

        #endregion

    }
}
