using ExportOrderWebServer.Areas.ImportDocument.Customer.Dto;
using FluentValidation;

namespace ExportOrderWebServer.Areas.ImportDocument.Customer.Validator;

    public class CustomerDtoValidator : AbstractValidator<CustomerDto>
    {
        public CustomerDtoValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required")
                .MaximumLength(500).WithMessage("Full name cannot exceed 500 characters");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Customer code is required")
                .MaximumLength(50).WithMessage("Customer code cannot exceed 50 characters")
                .Matches(@"^[A-Za-z0-9\-_]+$").WithMessage("Customer code can only contain letters, numbers, hyphens and underscores");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required")
                .MaximumLength(1000).WithMessage("Address cannot exceed 1000 characters");

            RuleFor(x => x.NameRu)
                .MaximumLength(200).WithMessage("Russian name cannot exceed 200 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.NameRu));

            RuleFor(x => x.AddressRu)
                .MaximumLength(1000).WithMessage("Russian address cannot exceed 1000 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.AddressRu));

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid status value");
        }
    }

