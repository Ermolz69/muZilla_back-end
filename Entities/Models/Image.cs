namespace muZilla.Entities.Models
{
    public class Image : IModel
    {
        public int Id { get; set; }
        public string ImageFilePath { get; set; }
        public string? DomainColor { get; set; }
    }
}
