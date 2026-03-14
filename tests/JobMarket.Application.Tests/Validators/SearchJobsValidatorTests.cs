using FluentAssertions;
using FluentValidation.Results;
using JobMarket.Application.Features.Jobs.DTOs;
using JobMarket.Application.Features.Jobs.Queries.SearchJobs;

namespace JobMarket.Application.Tests.Validators;

public class SearchJobsValidatorTests
{
    private readonly SearchJobsValidator _validator = new();

    [Fact]
    public void Validate_WithValidFilter_IsValid()
    {
        var query = new SearchJobsQuery(new JobSearchFilter { Page = 1, PageSize = 20 });

        ValidationResult result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithPageZero_IsInvalid()
    {
        var query = new SearchJobsQuery(new JobSearchFilter { Page = 0, PageSize = 20 });

        ValidationResult result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Page"));
    }

    [Fact]
    public void Validate_WithPageSizeOver100_IsInvalid()
    {
        var query = new SearchJobsQuery(new JobSearchFilter { Page = 1, PageSize = 101 });

        ValidationResult result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("PageSize"));
    }

    [Theory]
    [InlineData(1, 1, true)]
    [InlineData(1, 100, true)]
    [InlineData(5, 50, true)]
    [InlineData(0, 20, false)]
    [InlineData(1, 0, false)]
    [InlineData(1, 101, false)]
    public void Validate_AllCombinations_ReturnsExpectedValidity(int page, int pageSize, bool expected)
    {
        var query = new SearchJobsQuery(new JobSearchFilter { Page = page, PageSize = pageSize });

        ValidationResult result = _validator.Validate(query);

        result.IsValid.Should().Be(expected);
    }
}
