using FixIt.BLL.DTOs;
using FluentValidation;

namespace FixIt.BLL.Validators;

public class CreateIssueDtoValidator : AbstractValidator<CreateIssueDto>
{
    public CreateIssueDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required.")
            .MaximumLength(300).WithMessage("Location cannot exceed 300 characters.");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Please select a valid category.");

        RuleFor(x => x.Image)
            .Must(image => image == null || image.Length <= 5 * 1024 * 1024)
            .WithMessage("Image must be under 5MB.");
    }
}
