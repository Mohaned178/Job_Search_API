using JobMarket.Application.Common.Interfaces;
using JobMarket.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace JobMarket.Infrastructure.AI;

public class AtsService : IAtsService
{
    private readonly ILogger<AtsService> _logger;

    public AtsService(ILogger<AtsService> logger)
    {
        _logger = logger;
    }

    public Task<(int Score, AtsScoreBreakdown Breakdown)> ScoreAsync(
        CvParsedData cvData,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Calculating ATS score for CV with {SkillCount} skills", cvData.Skills.Count);

        int keywordScore = CalculateKeywordScore(cvData);
        int formattingScore = CalculateFormattingScore(cvData);
        int completenessScore = CalculateCompletenessScore(cvData);
        int skillRelevanceScore = CalculateSkillRelevanceScore(cvData);

        int totalScore = keywordScore + formattingScore + completenessScore + skillRelevanceScore;

        AtsScoreBreakdown breakdown = new()
        {
            KeywordScore = keywordScore,
            FormattingScore = formattingScore,
            CompletenessScore = completenessScore,
            SkillRelevanceScore = skillRelevanceScore,
            Suggestions = BuildSuggestions(cvData, keywordScore, formattingScore, completenessScore, skillRelevanceScore)
        };

        return Task.FromResult((totalScore, breakdown));
    }

    private static int CalculateKeywordScore(CvParsedData cv)
    {
        int score = 0;
        if (cv.Skills.Count >= 5) score += 15;
        else score += cv.Skills.Count * 3;
        if (!string.IsNullOrWhiteSpace(cv.Summary)) score += 10;
        if (cv.TotalYearsExperience > 0) score += 5;
        return Math.Min(score, 30);
    }

    private static int CalculateFormattingScore(CvParsedData cv)
    {
        int score = 0;
        if (!string.IsNullOrWhiteSpace(cv.FullName)) score += 5;
        if (!string.IsNullOrWhiteSpace(cv.Email)) score += 5;
        if (!string.IsNullOrWhiteSpace(cv.Phone)) score += 5;
        if (cv.Experience.Count > 0) score += 5;
        return Math.Min(score, 20);
    }

    private static int CalculateCompletenessScore(CvParsedData cv)
    {
        int score = 0;
        if (!string.IsNullOrWhiteSpace(cv.Summary)) score += 5;
        if (cv.Experience.Count > 0) score += 8;
        if (cv.Education.Count > 0) score += 6;
        if (cv.Languages.Count > 0) score += 3;
        if (cv.Skills.Count > 0) score += 3;
        return Math.Min(score, 25);
    }

    private static int CalculateSkillRelevanceScore(CvParsedData cv)
    {
        int score = cv.Skills.Count >= 10 ? 25 : cv.Skills.Count * 2;
        return Math.Min(score, 25);
    }

    private static List<string> BuildSuggestions(
        CvParsedData cv,
        int keywordScore,
        int formattingScore,
        int completenessScore,
        int skillRelevanceScore)
    {
        List<string> suggestions = [];

        if (keywordScore < 20)
            suggestions.Add("Add more industry-specific keywords and technical skills to improve ATS keyword detection.");
        if (cv.Skills.Count < 5)
            suggestions.Add("List at least 8-10 specific technical skills relevant to your target role.");
        if (string.IsNullOrWhiteSpace(cv.Summary))
            suggestions.Add("Add a professional summary section at the top of your CV.");
        if (formattingScore < 15)
            suggestions.Add("Ensure your CV includes full name, professional email, and phone number.");
        if (cv.Experience.Count == 0)
            suggestions.Add("Add work experience entries with clear job titles, companies, and dates.");
        if (cv.Education.Count == 0)
            suggestions.Add("Add your educational background including degree and institution.");
        if (skillRelevanceScore < 15)
            suggestions.Add("Include more role-specific skills with tools, frameworks, and technologies.");

        return suggestions;
    }
}
