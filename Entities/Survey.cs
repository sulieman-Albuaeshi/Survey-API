namespace Entities;

public enum SurveyStatus : Byte
{
    Draft = 0,
    Published = 1,
    Archived = 2
}
public enum EnMode { AddNew = 0, Update = 1 };

public class Survey
{   
    public EnMode Mode = EnMode.AddNew;
    
    public int Id { get; set; }
    public string  Title { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsAnonymous { get; set; }
    public int UserId { get; set; }
    public SurveyStatus Status { get; set; }
    public int QuestionCount { get; set; }
}