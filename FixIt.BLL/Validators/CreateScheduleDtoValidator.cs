using FixIt.BLL.DTOs;
using FluentValidation;

namespace FixIt.BLL.Validators;

public class CreateScheduleDtoValidator : AbstractValidator<CreateScheduleDto>
{
    public CreateScheduleDtoValidator()
    {
        RuleFor(x => x.IssueId)
            .GreaterThan(0)
            .WithMessage("Issue ID is required.");

        RuleFor(x => x.VisitDate)
            .GreaterThan(DateTime.Now)
            .WithMessage("Visit date must be in the future.");

        RuleFor(x => x.EstimatedCost)
            .GreaterThan(0)
            .WithMessage("Estimated cost must be greater than 0.");
    }
}
