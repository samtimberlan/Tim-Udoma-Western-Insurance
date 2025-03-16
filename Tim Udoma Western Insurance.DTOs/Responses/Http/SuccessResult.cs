namespace Tim_Udoma_Western_Insurance.DTOs.Responses.Http
{
    public class SuccessResult : Result
    {
        public SuccessResult() : base(true)
        {
            Status = 200;
        }

        public SuccessResult(object content) : base(true, string.Empty, content)
        {
            Message = "Successful";
            Status = 200;
        }

        public SuccessResult(string message) : base(true, message, null)
        {
            Status = 200;
        }

        public SuccessResult(object content, string message) : base(true, message, content)
        {
            Status = 200;
        }

        public SuccessResult(object content, string message, int statusCode) : base(true, message, content)
        {
            Status = statusCode;
        }

    }

    public class SuccessResult<T> : Result<T>
    {
        public SuccessResult() : base(true)
        {
            Status = 200;
        }

        public SuccessResult(T content) : base(true, string.Empty, content)
        {
            Status = 200;
        }

        public SuccessResult(T content, string message) : base(true, message, content)
        {
            Status = 200;
        }

        public SuccessResult(T content, string message, int statusCode) : base(true, message, content)
        {
            Status = statusCode;
        }

    }

}
