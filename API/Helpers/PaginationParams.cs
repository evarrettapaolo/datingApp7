namespace API.Helpers
{
  public class PaginationParams
  {
    private const int MaxPageSize = 50; //max size
    private int _pageSize = 10; //min size

    //Actual class public property
    public int PageNumber { get; set; } = 1;
    public int PageSize
    {
      get => _pageSize;
      set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }
  }
}