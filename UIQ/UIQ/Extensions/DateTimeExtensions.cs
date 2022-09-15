namespace UIQ
{
    public static class DateTimeExtensions
    {
        public static int GetUnixTimestamp(this DateTime dateTime)
        {
            return (int)dateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }
    }
}
