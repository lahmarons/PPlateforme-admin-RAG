using IdaraTech.Application.Agents.Commands.CreateAgent;
using IdaraTech.Application.Agents.Queries.GetAgentsList;
using IdaraTech.Application.Agents.Queries.GetAgents;
using Microsoft.AspNetCore.Http.HttpResults;

namespace IdaraTech.Web.Endpoints;

public class Agents : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetAgents);
        groupBuilder.MapPost(CreateAgent);
    }

    public async Task<Ok<List<AgentDto>>> GetAgents(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAgentsQuery(), cancellationToken);
        return TypedResults.Ok(result);
    }

    public async Task<Created<int>> CreateAgent(ISender sender, CreateAgentCommand command, CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return TypedResults.Created($"/api/agents/{id}", id);
    }
}
