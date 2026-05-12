namespace DTOs;

using Entities;
public class SurveyDto
{ 
    public int Id { get; set; }
    public required string  Title { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }
    public required string Status { get; set; }
}