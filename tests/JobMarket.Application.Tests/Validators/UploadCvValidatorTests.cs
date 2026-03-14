using FluentAssertions;
using FluentValidation.Results;
using JobMarket.Application.Features.CVs.Commands.UploadCv;

namespace JobMarket.Application.Tests.Validators;

public class UploadCvValidatorTests
{
    private readonly UploadCvValidator _validator = new();

    private static System.IO.Stream CreateStream(int bytes = 100)
    {
        var ms = new System.IO.MemoryStream(new byte[bytes]);
        return ms;
    }

    [Fact]
    public void Validate_WithValidPdf_IsValid()
    {
        var command = new UploadCvCommand(Guid.NewGuid(), CreateStream(), "resume.pdf");

        ValidationResult result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithValidDocx_IsValid()
    {
        var command = new UploadCvCommand(Guid.NewGuid(), CreateStream(), "resume.docx");

        ValidationResult result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithInvalidExtension_HasFileNameError()
    {
        var command = new UploadCvCommand(Guid.NewGuid(), CreateStream(), "resume.txt");

        ValidationResult result = _validator.Validate(command);

        result.Errors.Should().Contain(e => e.PropertyName == "FileName");
    }

    [Fact]
    public void Validate_WithOversizedFile_HasFileStreamError()
    {
        int bigSize = 6 * 1024 * 1024;
        var command = new UploadCvCommand(Guid.NewGuid(), CreateStream(bigSize), "resume.pdf");

        ValidationResult result = _validator.Validate(command);

        result.Errors.Should().Contain(e => e.PropertyName == "FileStream");
    }

    [Fact]
    public void Validate_WithEmptyUserId_HasUserIdError()
    {
        var command = new UploadCvCommand(Guid.Empty, CreateStream(), "resume.pdf");

        ValidationResult result = _validator.Validate(command);

        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }
}
