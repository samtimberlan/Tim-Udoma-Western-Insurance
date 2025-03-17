namespace Tim_Udoma_Western_Insurance.DTOs.Responses.Http.Interfaces
{
    public interface IPagedList
    {
        int PageIndex { get; }
        int PageSize { get; }
        int TotalPages { get; }
        int TotalItems { get; }
        bool HasPreviousPage { get; }
        bool HasNextPage { get; }
    }
}
