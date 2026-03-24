using FluentValidation;

namespace IdaraTech.Application.Demandes.Commands.CreateDemande;

public class CreateDemandeCommandValidator : AbstractValidator<CreateDemandeCommand>
{
    public CreateDemandeCommandValidator()
    {
        RuleFor(v => v.CitoyenId)
            .GreaterThan(0)
            .WithMessage("CitoyenId must be greater than 0.");

        RuleFor(v => v.Description)
            .MaximumLength(1000)
            .NotEmpty()
            .WithMessage("Description is required.");
    }
}
