
using ExportOrderWebServer.Areas.ImportDocument.VesselCall.Dto;
using FluentValidation;


namespace ExportOrderWebServer.Areas.ImportDocument.VesselCall.Validator
{
    public class VesselCallDtoValidator : AbstractValidator<VesselCallDto>
    {
        public VesselCallDtoValidator()
        {
            RuleFor(x => x.VesselId)
                .NotEmpty().WithMessage("Vessel is required")
                .NotEqual(Guid.Empty).WithMessage("Valid vessel must be selected");

            RuleFor(x => x.VoyageNo)
                .NotEmpty().WithMessage("Voyage number is required")
                .MaximumLength(50).WithMessage("Voyage number cannot exceed 50 characters")
                .Matches(@"^[A-Za-z0-9\-/]+$").WithMessage("Voyage number can only contain letters, numbers, hyphens and slashes");

            RuleFor(x => x.TerminalVoyageNo)
                .MaximumLength(50).WithMessage("Terminal voyage number cannot exceed 50 characters");

            RuleFor(x => x.TerminalId)
                .NotEmpty().WithMessage("Terminal is required")
                .NotEqual(Guid.Empty).WithMessage("Valid terminal must be selected");

            RuleFor(x => x.PortOfLoadingId)
                .NotEmpty().WithMessage("Port of loading is required")
                .NotEqual(Guid.Empty).WithMessage("Valid port of loading must be selected");

            RuleFor(x => x.ETA)
                .NotEmpty().WithMessage("ETA is required")
                .GreaterThanOrEqualTo(DateTime.UtcNow.AddYears(-1)).WithMessage("ETA cannot be too far in the past")
                .LessThanOrEqualTo(DateTime.UtcNow.AddYears(1)).WithMessage("ETA cannot be too far in the future");

            RuleFor(x => x.ETS)
                .NotEmpty().WithMessage("ETS is required")
                .GreaterThanOrEqualTo(x => x.ETA).WithMessage("ETS must be after or equal to ETA")
                .When(x => x.ETA != default);

            RuleFor(x => x.FeederBlNo)
                .MaximumLength(100).WithMessage("Feeder BL number cannot exceed 100 characters");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid status value");
        }
    }
}