using IdaraTech.Application.Agents.Commands.CreateAgent;
using IdaraTech.Application.Agents.Commands.DeleteAgent;
using IdaraTech.Application.Agents.Commands.UpdateAgent;
using IdaraTech.Application.Agents.Queries.GetAgentsList;
using IdaraTech.Application.Agents.Queries.GetAgents;
using Microsoft.AspNetCore.Http.HttpResults;

namespace IdaraTech.Web.Endpoints;

public class Agents : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetAgents);
        groupBuilder.MapGet(GetAgentById, "{id}");
        groupBuilder.MapPost(CreateAgent);
        groupBuilder.MapPut(UpdateAgent, "{id}");
        groupBuilder.MapDelete(DeleteAgent, "{id}");
    }

    public async Task<Ok<List<AgentDto>>> GetAgents(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAgentsQuery(), cancellationToken);
        return TypedResults.Ok(result);
    }

    public async Task<Ok<AgentDto>> GetAgentById(ISender sender, int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAgentByIdQuery { Id = id }, cancellationToken);
        return TypedResults.Ok(result);
    }

    public async Task<Created<int>> CreateAgent(ISender sender, CreateAgentCommand command, CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return TypedResults.Created($"/api/agents/{id}", id);
    }

    public async Task<Results<NoContent, BadRequest>> UpdateAgent(ISender sender, int id, UpdateAgentCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return TypedResults.BadRequest();
        }

        await sender.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    public async Task<NoContent> DeleteAgent(ISender sender, int id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteAgentCommand(id), cancellationToken);
        return TypedResults.NoContent();
    }
}
