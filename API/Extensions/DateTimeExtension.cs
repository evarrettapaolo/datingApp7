namespace API.Extensions
{
  public static class DateTimeExtension
  {
    //not precisely accurate
    public static int CalculateAge(this DateOnly dob)
    {
      var today = DateOnly.FromDateTime(DateTime.UtcNow);
      var age = today.Year - dob.Year;

      //Birthday has not occured yet this year, take one off
      if (dob > today.AddYears(-age)) age--;

      return age;
    }
  }
}