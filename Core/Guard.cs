namespace Core
{
    public static class Guard
    {
        public static T NotNull<T>(T? value, string name) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(name);
            }

            return value;
        }
    }
}
