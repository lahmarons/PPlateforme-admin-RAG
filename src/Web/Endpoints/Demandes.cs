using IdaraTech.Application.Demandes.Commands.ChangeDemandeStatus;
using IdaraTech.Application.Demandes.Commands.CreateDemande;
using IdaraTech.Application.Demandes.Commands.DeleteDemande;
using IdaraTech.Application.Demandes.Commands.UpdateDemande;
using IdaraTech.Application.Demandes.Queries.GetDemandById;
using IdaraTech.Application.Demandes.Queries.GetDemandesWithPagination;
using Microsoft.AspNetCore.Http.HttpResults;

namespace IdaraTech.Web.Endpoints;

public class Demandes : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetDemandes);
        groupBuilder.MapGet(GetDemandeById, "{id}");
        groupBuilder.MapPost(CreateDemande);
        groupBuilder.MapPut(UpdateDemande, "{id}");
        groupBuilder.MapPut(ChangeDemandeStatus, "{id}/status");
        groupBuilder.MapDelete(DeleteDemande, "{id}");
    }

    public async Task<Ok<IdaraTech.Application.Common.Models.PaginatedList<DemandeDto>>> GetDemandes(
        ISender sender,
        [AsParameters] GetDemandesWithPaginationQuery query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return TypedResults.Ok(result);
    }

    public async Task<Ok<DemandeDto>> GetDemandeById(ISender sender, int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetDemandeByIdQuery { Id = id }, cancellationToken);
        return TypedResults.Ok(result);
    }

    public async Task<Created<int>> CreateDemande(ISender sender, CreateDemandeCommand command, CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return TypedResults.Created($"/api/demandes/{id}", id);
    }

    public async Task<Results<NoContent, BadRequest>> UpdateDemande(ISender sender, int id, UpdateDemandeCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return TypedResults.BadRequest();
        }

        await sender.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    public async Task<NoContent> ChangeDemandeStatus(ISender sender, int id, ChangeDemandeStatusCommand command, CancellationToken cancellationToken)
    {
        await sender.Send(command with { DemandeId = id }, cancellationToken);
        return TypedResults.NoContent();
    }

    public async Task<NoContent> DeleteDemande(ISender sender, int id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteDemandeCommand(id), cancellationToken);
        return TypedResults.NoContent();
    }
}
