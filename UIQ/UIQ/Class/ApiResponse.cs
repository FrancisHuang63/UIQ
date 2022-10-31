namespace UIQ
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        
        public string Message { get; set; }
        
        public T Data { get; set; }

        public ApiResponse()
        {

        }

        public ApiResponse(T data)
        {
            Data = data;
            Success = true;
        }

        public ApiResponse(string message)
        {
            Message = message;
            Success = false;
        }
    }
}
