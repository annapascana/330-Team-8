namespace CrimsonBookStore.Api.Models;

public class Course
{
    public int CourseID { get; set; }
    public int MajorID { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
}

