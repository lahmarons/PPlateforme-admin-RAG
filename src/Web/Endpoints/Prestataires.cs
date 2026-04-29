using IdaraTech.Application.Prestataires.Commands.CreatePrestataire;
using IdaraTech.Application.Prestataires.Commands.DeletePrestataire;
using IdaraTech.Application.Prestataires.Commands.UpdatePrestataire;
using IdaraTech.Application.Prestataires.Queries.GetPrestataires;
using IdaraTech.Domain.Constants;
using Microsoft.AspNetCore.Http.HttpResults;

namespace IdaraTech.Web.Endpoints;

public class Prestataires : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetPrestataires);
        groupBuilder.MapGet(GetPrestataireById, "{id}");
        groupBuilder.MapPost(CreatePrestataire);
        groupBuilder.MapPut(UpdatePrestataire, "{id}");
        groupBuilder.MapDelete(DeletePrestataire, "{id}");
    }

    public async Task<Ok<List<PrestataireDto>>> GetPrestataires(
        ISender sender,
        [AsParameters] GetPrestatairesQuery query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return TypedResults.Ok(result);
    }

    public async Task<Ok<PrestataireDto>> GetPrestataireById(ISender sender, int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPrestataireByIdQuery { Id = id }, cancellationToken);
        return TypedResults.Ok(result);
    }

    public async Task<Created<int>> CreatePrestataire(ISender sender, CreatePrestataireCommand command, CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return TypedResults.Created($"/api/prestataires/{id}", id);
    }

    public async Task<Results<NoContent, BadRequest>> UpdatePrestataire(ISender sender, int id, UpdatePrestataireCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return TypedResults.BadRequest();
        }

        await sender.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    public async Task<NoContent> DeletePrestataire(ISender sender, int id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeletePrestataireCommand(id), cancellationToken);
        return TypedResults.NoContent();
    }
}
