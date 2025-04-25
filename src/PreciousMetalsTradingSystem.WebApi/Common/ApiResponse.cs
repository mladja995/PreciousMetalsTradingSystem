namespace PreciousMetalsTradingSystem.WebApi.Common
{
    public class ApiResponse
    {
        public object? Data { get; private set; } 
        public bool IsSuccess { get; init; }
        public bool IsFailure => !IsSuccess;
        public IEnumerable<ApiError> Errors { get; init; } = [];
        public int StatusCode { get; init; }

        public ApiResponse(object? data, bool isSuccess, int statusCode)
        {
            Data = data;
            IsSuccess = isSuccess;
            StatusCode = statusCode;
        }

        public static ApiResponse Success(object? data = null, int statusCode = StatusCodes.Status200OK)
            => new(data, isSuccess: true, statusCode: statusCode);

        public static ApiResponse Failure(IEnumerable<ApiError> errors, int statusCode = StatusCodes.Status400BadRequest)
            => new(null, isSuccess: false, statusCode: statusCode)
            {
                Errors = errors
            };

        public static ApiResponse Failure(ApiError error, int statusCode = StatusCodes.Status400BadRequest)
            => Failure([error], statusCode);
    }
}
