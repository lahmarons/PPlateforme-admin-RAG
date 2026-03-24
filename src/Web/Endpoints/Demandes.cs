using IdaraTech.Application.Demandes.Commands.ChangeDemandeStatus;
using IdaraTech.Application.Demandes.Commands.CreateDemande;
using IdaraTech.Application.Demandes.Queries.GetDemandById;
using Microsoft.AspNetCore.Http.HttpResults;

namespace IdaraTech.Web.Endpoints;

public class Demandes : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetDemandeById, "{id}");
        groupBuilder.MapPost(CreateDemande);
        groupBuilder.MapPut(ChangeDemandeStatus, "{id}/status");
    }

    public async Task<Ok<DemandeDto>> GetDemandeById(ISender sender, int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetDemandeByIdQuery { Id = id }, cancellationToken);

        if (result == null)
            return TypedResults.Ok(new DemandeDto());

        return TypedResults.Ok(result);
    }

    public async Task<Created<int>> CreateDemande(ISender sender, CreateDemandeCommand command, CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return TypedResults.Created($"/api/demandes/{id}", id);
    }

    public async Task<NoContent> ChangeDemandeStatus(ISender sender, int id, ChangeDemandeStatusCommand command, CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }
}
