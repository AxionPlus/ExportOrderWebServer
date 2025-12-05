using ExportOrderWebServer.Areas.ImportDocument.Terminal.Dto;
using FluentValidation;

namespace ExportOrderWebServer.Areas.ImportDocument.Terminal.Validator
{
    public class TerminalDtoValidator : AbstractValidator<TerminalDto>
    {
        public TerminalDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Terminal name is required")
                .MaximumLength(200).WithMessage("Terminal name cannot exceed 200 characters");

            RuleFor(x => x.Email)
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters")
                .EmailAddress().WithMessage("Invalid email format")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.CustomsPost)
                .MaximumLength(50).WithMessage("Customs post code cannot exceed 50 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.CustomsPost));

            RuleFor(x => x.CustomsPostName)
                .MaximumLength(200).WithMessage("Customs post name cannot exceed 200 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.CustomsPostName));

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid status value");
        }
    }
}