using BLL.Models.AiTriage;

namespace BLL.Interfaces;

public interface IAiTriageService
{
    Task<AiTriageResultDto> AnalyzeAsync(AiTriageRequestDto request);
}
