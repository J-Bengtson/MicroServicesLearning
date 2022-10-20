namespace Core.Domain
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Error { get; set; }

        public static ApiResponse<T> CreateSuccess(T data)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Error = null
            };
        }

        public static ApiResponse<T> CreateFail(string error)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Data = default,
                Error = error
            };
        }
    }
}
