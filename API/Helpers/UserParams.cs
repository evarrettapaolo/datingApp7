namespace API.Helpers
{
  public class UserParams
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
    //used for filtering
    public string CurrentUsername { get; set; } 
    public string Gender { get; set; }
    public int MinAge { get; set; } = 18;
    public int MaxAge { get; set; } = 100;
    public string OrderBy { get; set; } = "lastActive";
  }
}