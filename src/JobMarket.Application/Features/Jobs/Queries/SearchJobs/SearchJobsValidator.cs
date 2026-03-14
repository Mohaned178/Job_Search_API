using FluentValidation;

namespace JobMarket.Application.Features.Jobs.Queries.SearchJobs;

public class SearchJobsValidator : AbstractValidator<SearchJobsQuery>
{
    public SearchJobsValidator()
    {
        RuleFor(x => x.Filter.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.Filter.PageSize)
            .InclusiveBetween(1, 100);
    }
}
