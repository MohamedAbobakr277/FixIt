using FixIt.BLL.DTOs;
using FluentValidation;

namespace FixIt.BLL.Validators;

public class IssueFilterDtoValidator : AbstractValidator<IssueFilterDto>
{
    public IssueFilterDtoValidator()
    {
        RuleFor(x => x.PageIndex)
            .GreaterThanOrEqualTo(1).WithMessage("Page index must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50).WithMessage("Page size must be between 1 and 50.");

        RuleFor(x => x.SortOrder)
            .Must(x => string.IsNullOrEmpty(x) || x.ToLower() == "asc" || x.ToLower() == "desc")
            .WithMessage("Sort order must be 'asc' or 'desc'.");
            
        RuleFor(x => x.SortBy)
            .Must(x => string.IsNullOrEmpty(x) || x.ToLower() == "date" || x.ToLower() == "status")
            .WithMessage("Sort by must be 'date' or 'status'.");
    }
}
