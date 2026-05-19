namespace BLL.Models.AiTriage;

public class AiTriageResultDto
{
    public bool IsRecognized { get; set; }
    public string? PredictedSpecialty { get; set; }
    public string? PredictedUrgency { get; set; }
    public float Confidence { get; set; }
    public string HumanMessage { get; set; } = string.Empty;
}
