using FluentValidation;

namespace JobMarket.Application.Features.CVs.Commands.UploadCv;

public class UploadCvValidator : AbstractValidator<UploadCvCommand>
{
    private static readonly string[] AllowedExtensions = { ".pdf", ".docx" };
    private const long MaxFileSize = 5 * 1024 * 1024;

    public UploadCvValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty()
            .Must(f => AllowedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
            .WithMessage("Only PDF and DOCX files are allowed.");

        RuleFor(x => x.FileStream)
            .NotNull()
            .Must(s => s.Length <= MaxFileSize)
            .WithMessage("File size must not exceed 5MB.");

        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}
