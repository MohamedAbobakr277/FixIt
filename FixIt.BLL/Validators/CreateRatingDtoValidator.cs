using FluentValidation;
using FixIt.BLL.DTOs;

namespace FixIt.BLL.Validators;

public class CreateRatingDtoValidator : AbstractValidator<CreateRatingDto>
{
    public CreateRatingDtoValidator()
    {
        RuleFor(x => x.IssueId)
            .GreaterThan(0).WithMessage("Valid Issue is required.");

        RuleFor(x => x.Stars)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5 stars.");

        RuleFor(x => x.Comment)
            .MaximumLength(1000).WithMessage("Comment cannot exceed 1000 characters.");
    }
}
