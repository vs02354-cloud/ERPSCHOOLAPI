namespace SchoolERP.Api.Models.DTOs
{
    public class EmployeeDto
    {
        public Employee Employee { get; set; } = new Employee();
        public bool IsActive { get; set; }
    }

    public class PaginatedResponse<T>
    {
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<T> Items { get; set; } = new List<T>();
    }
}
