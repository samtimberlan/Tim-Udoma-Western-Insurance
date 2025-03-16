namespace Tim_Udoma_Western_Insurance.DTOs.Responses.Http
{
    public class ErrorResult : Result
    {
        public ErrorResult() : base(false)
        {
            Status = 500;
        }

        public ErrorResult(string message) : base(false, message)
        {
            Status = 400;
        }

        public ErrorResult(string message, int status) : base(false, message)
        {
            Status = status;
        }

    }

    public class ErrorResult<T> : Result<T>
    {
        public ErrorResult() : base(false)
        {
            Status = 500;
        }

        public ErrorResult(string message) : base(false, message)
        {
            Status = 400;
        }
        public ErrorResult(string message, int status) : base(false, message)
        {
            Status = status;
        }
    }

}
