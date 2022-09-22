namespace UIQ
{
    public class PageDataResponse<T>
    {
        public string Result { get; set; } = "ERROR";
        public int TotalRecordCount { get; set; }
        public T Records { get; set; }

        public PageDataResponse()
        {

        }

        public PageDataResponse(T data, int total)
        {
            Records = data;
            TotalRecordCount = total;
            Result = "OK";
        }
    }
}