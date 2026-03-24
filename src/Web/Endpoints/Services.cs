using IdaraTech.Application.Services.Commands.CreateService;
using IdaraTech.Application.Services.Queries.GetServicesList;
using IdaraTech.Application.Services.Queries.GetServices;
using Microsoft.AspNetCore.Http.HttpResults;

namespace IdaraTech.Web.Endpoints;

public class Services : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetServices);
        groupBuilder.MapPost(CreateService);
    }

    public async Task<Ok<List<ServiceDto>>> GetServices(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetServicesQuery(), cancellationToken);
        return TypedResults.Ok(result);
    }

    public async Task<Created<int>> CreateService(ISender sender, CreateServiceCommand command, CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return TypedResults.Created($"/api/services/{id}", id);
    }
}
