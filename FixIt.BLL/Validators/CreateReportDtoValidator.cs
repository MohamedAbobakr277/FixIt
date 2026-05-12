using FixIt.BLL.DTOs;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace FixIt.BLL.Validators;

public class CreateReportDtoValidator : AbstractValidator<CreateReportDto>
{
    public CreateReportDtoValidator()
    {
        RuleFor(x => x.IssueId)
            .GreaterThan(0)
            .WithMessage("Issue ID is required.");

        RuleFor(x => x.Summary)
            .NotEmpty()
            .WithMessage("Summary is required.")
            .MinimumLength(10)
            .WithMessage("Summary must be at least 10 characters long.");

        RuleFor(x => x.BeforeImage)
            .Must(BeValidImage)
            .When(x => x.BeforeImage != null)
            .WithMessage("Before image must be a valid image file (jpg, jpeg, png, gif) and maximum 5 MB.");

        RuleFor(x => x.AfterImage)
            .Must(BeValidImage)
            .When(x => x.AfterImage != null)
            .WithMessage("After image must be a valid image file (jpg, jpeg, png, gif) and maximum 5 MB.");
    }

    private bool BeValidImage(IFormFile? file)
    {
        if (file == null) return true;

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        return allowedExtensions.Contains(extension) && file.Length <= 5 * 1024 * 1024; // 5 MB
    }
}
