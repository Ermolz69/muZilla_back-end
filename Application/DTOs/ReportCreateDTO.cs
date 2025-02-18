namespace muZilla.Application.DTOs
{
    public class ReportCreateDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; } = "Medium";
        public bool? IsClosed { get; set; }
    }
}
