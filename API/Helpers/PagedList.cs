using Microsoft.EntityFrameworkCore;

namespace API.Helpers
{
  public class PagedList<T> : List<T>
  {
    public PagedList(IEnumerable<T> items, int count, int pageNumber, int pageSize)
    {
      CurrentPage = pageNumber;
      TotalPages = (int) Math.Ceiling(count / (double) pageSize); //rounding up, pages must be more than items
      PageSize = pageSize;
      TotalCount = count;
      AddRange(items); //pass the parameters values to the class instance
    }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }

    public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
      //executes query
      var count = await source.CountAsync();
      //not skipping anything  on first page, skip some on second and so
      var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
      //return a new instance of the class, which is of List type
      return new PagedList<T>(items, count, pageNumber, pageSize);
    }
  }
}