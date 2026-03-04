using FluentValidation;
using Dto = ExportOrderEntites.BillofLading.Dto;

namespace ExportOrderWebServer.Areas.ImportDocument.BillOfLading.Validator;

public class BillOfLadingDtoValidator : AbstractValidator<Dto.BillOfLadingBaseDto>
{
    public BillOfLadingDtoValidator()
    {
        RuleFor(x => x.Num)
            .NotEmpty().WithMessage("BL number is required")
            .MaximumLength(50).WithMessage("BL number cannot exceed 50 characters")
            .Matches(@"^[A-Za-z0-9\-/]+$").WithMessage("BL number can only contain letters, numbers, hyphens and slashes");

        //RuleFor(x => x.Date)
        //    .NotEmpty().WithMessage("BL date is required")
        //    .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("BL date cannot be in the future")
        //    .GreaterThanOrEqualTo(DateTime.UtcNow.AddYears(-5)).WithMessage("BL date cannot be too far in the past");

        RuleFor(x => x.VesselCallId)
            .NotEmpty().WithMessage("Vessel call is required")
            .NotEqual(Guid.Empty).WithMessage("Valid vessel call must be selected");

        RuleFor(x => x.ShipperName)
            .NotEmpty().WithMessage("Shipper name is required")
            .MaximumLength(200).WithMessage("Shipper name cannot exceed 200 characters");

        RuleFor(x => x.ConsigneeName)
            .NotEmpty().WithMessage("Consignee name is required")
            .MaximumLength(200).WithMessage("Consignee name cannot exceed 200 characters");

        RuleFor(x => x.CargoDescription)
            .MaximumLength(1000).WithMessage("Cargo description cannot exceed 1000 characters");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid status value");

        RuleForEach(x => x.ContainerRecords)
            .SetValidator(new BillOfLadingContainerRecordDtoValidator());
    }
}

public class BillOfLadingContainerRecordDtoValidator : AbstractValidator<Dto.BillOfLadingContainerRecordBaseDto>
{
    public BillOfLadingContainerRecordDtoValidator()
    {
        RuleFor(x => x.ContainerNo)
            .NotEmpty().WithMessage("Container number is required")
            .MaximumLength(20).WithMessage("Container number cannot exceed 20 characters")
            .Matches(@"^[A-Za-z]{3}[UJZ]\d{6}\d?$").WithMessage("Invalid container number format");

        RuleFor(x => x.IsoCode)
            .MaximumLength(10).WithMessage("ISO code cannot exceed 10 characters");

        RuleFor(x => x.GrossWeight)
            .GreaterThan(0).WithMessage("Gross weight must be greater than 0")
            .LessThanOrEqualTo(100000).WithMessage("Gross weight cannot exceed 100,000");

        RuleFor(x => x.NoOfPackage)
            .GreaterThan(0).WithMessage("Number of packages must be greater than 0");

        RuleFor(x => x.Volume)
            .GreaterThanOrEqualTo(0).WithMessage("Volume cannot be negative");

        When(x => !string.IsNullOrEmpty(x.ReeferTemp), () =>
        {
            RuleFor(x => x.ReeferTempUOM)
                .NotEmpty().WithMessage("Reefer temperature UOM is required when temperature is specified")
                .Must(uom => uom == "C" || uom == "F")
                .WithMessage("Reefer temperature UOM must be 'C' or 'F'");
        });
    }
}