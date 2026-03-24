using IdaraTech.Application.Citoyens.Commands.CreateCitoyen;
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
}
