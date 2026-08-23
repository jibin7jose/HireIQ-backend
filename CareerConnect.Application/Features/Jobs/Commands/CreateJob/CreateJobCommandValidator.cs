using FluentValidation;

namespace CareerConnect.Application.Features.Jobs.Commands.CreateJob;

public sealed class CreateJobCommandValidator : AbstractValidator<CreateJobCommand>
{
    public CreateJobCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Job title is required.")
            .MaximumLength(200).WithMessage("Job title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Job description is required.")
            .MinimumLength(50).WithMessage("Job description must be at least 50 characters.");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Job location is required.");

        RuleFor(x => x.JobType)
            .NotEmpty().WithMessage("Job type is required.");

        RuleFor(x => x.MinSalary)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum salary must be non-negative.");

        RuleFor(x => x.MaxSalary)
            .GreaterThan(x => x.MinSalary).WithMessage("Maximum salary must be greater than minimum salary.");
    }
}
