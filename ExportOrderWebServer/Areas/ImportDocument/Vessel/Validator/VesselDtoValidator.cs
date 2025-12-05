using ExportOrderWebServer.Areas.ImportDocument.Vessel.Dto;
using FluentValidation;
// VesselValidator.cs


namespace ExportOrderWebServer.Areas.ImportDocument.Vessel.Validator
{

    public class VesselDtoValidator : AbstractValidator<VesselDto>
    {
        public VesselDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Vessel name is required")
                .MaximumLength(200).WithMessage("Vessel name cannot exceed 200 characters");

            RuleFor(x => x.FlagRu)
                .NotEmpty().WithMessage("Flag (RU) is required")
                .MaximumLength(100).WithMessage("Flag (RU) cannot exceed 100 characters");

            RuleFor(x => x.FlagEn)
                .MaximumLength(100).WithMessage("Flag (EN) cannot exceed 100 characters");

            RuleFor(x => x.ShortName)
                .MaximumLength(50).WithMessage("Short name cannot exceed 50 characters");

            RuleFor(x => x.RolisCode)
                .MaximumLength(20).WithMessage("Rolis code cannot exceed 20 characters");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid status value");
        }
    }

}
