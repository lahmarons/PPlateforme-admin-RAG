using IdaraTech.Application.Citoyens.Commands.CreateCitoyen;
using IdaraTech.Application.Citoyens.Commands.DeleteCitoyen;
using IdaraTech.Application.Citoyens.Commands.UpdateCitoyen;
using IdaraTech.Application.Citoyens.Queries.GetCitoyens;
using IdaraTech.Application.Citoyens.Queries.GetCitoyenById;
using Microsoft.AspNetCore.Http.HttpResults;

namespace IdaraTech.Web.Endpoints;

public class Citoyens : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetCitoyens);
        groupBuilder.MapPost(CreateCitoyen);
        groupBuilder.MapPut(UpdateCitoyen, "{id}");
        groupBuilder.MapDelete(DeleteCitoyen, "{id}");
    }

    public async Task<Ok<List<CitoyenDto>>> GetCitoyens(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCitoyensQuery(), cancellationToken);
        return TypedResults.Ok(result);
    }

    public async Task<Created<int>> CreateCitoyen(ISender sender, CreateCitoyenCommand command, CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return TypedResults.Created($"/api/citoyens/{id}", id);
    }

    public async Task<Results<NoContent, BadRequest>> UpdateCitoyen(ISender sender, int id, UpdateCitoyenCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return TypedResults.BadRequest();
        }

        await sender.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    public async Task<NoContent> DeleteCitoyen(ISender sender, int id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteCitoyenCommand(id), cancellationToken);
        return TypedResults.NoContent();
    }
}
