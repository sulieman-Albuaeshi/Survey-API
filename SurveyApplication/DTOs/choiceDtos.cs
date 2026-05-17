namespace DTOs;

public class ChoiceDto
{
    public int Id { get; set; }
    public string ChoiceText { get; set; }
    public int OrderIndex { get; set; }
}

public class CreateChoiceDto
{
    public string ChoiceText { get; set; }
    public int OrderIndex { get; set; }
}
