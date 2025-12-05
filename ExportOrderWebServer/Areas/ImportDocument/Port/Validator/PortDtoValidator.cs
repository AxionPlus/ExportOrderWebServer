using ExportOrderWebServer.Areas.ImportDocument.Port.Dto;
using FluentValidation;

namespace ExportOrderWebServer.Areas.ImportDocument.Port.Validator
{
    public class PortDtoValidator : AbstractValidator<PortDto>
    {
        public PortDtoValidator()
        {
            RuleFor(x => x.NameRu)
                .NotEmpty().WithMessage("Russian port name is required")
                .MaximumLength(200).WithMessage("Russian port name cannot exceed 200 characters");

            RuleFor(x => x.NameEn)
                .MaximumLength(200).WithMessage("English port name cannot exceed 200 characters");

            RuleFor(x => x.CountryRu)
                .NotEmpty().WithMessage("Russian country name is required")
                .MaximumLength(100).WithMessage("Russian country name cannot exceed 100 characters");

            RuleFor(x => x.CountryEn)
                .MaximumLength(100).WithMessage("English country name cannot exceed 100 characters");

            RuleFor(x => x.IsoCode)
                .NotEmpty().WithMessage("ISO code is required")
                .MaximumLength(10).WithMessage("ISO code cannot exceed 10 characters")
                .Matches(@"^[A-Z]{2,10}$").WithMessage("ISO code must contain only uppercase letters");

            RuleFor(x => x.AuxIsoCode)
                .MaximumLength(10).WithMessage("Auxiliary ISO code cannot exceed 10 characters")
                .Matches(@"^[A-Z]{0,10}$").WithMessage("Auxiliary ISO code must contain only uppercase letters");

            RuleFor(x => x.PikYugIsoCode)
                .MaximumLength(10).WithMessage("PikYug ISO code cannot exceed 10 characters")
                .Matches(@"^[A-Z]{0,10}$").WithMessage("PikYug ISO code must contain only uppercase letters");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid status value");
        }
    }
}
