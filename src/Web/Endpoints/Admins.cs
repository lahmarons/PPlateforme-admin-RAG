using IdaraTech.Application.Admins.Commands.CreateAdmin;
using IdaraTech.Application.Admins.Commands.DeleteAdmin;
using IdaraTech.Application.Admins.Commands.UpdateAdmin;
using IdaraTech.Application.Admins.Queries.GetAdmins;
using IdaraTech.Application.Admins.Queries.GetAdminsList;
using Microsoft.AspNetCore.Http.HttpResults;

namespace IdaraTech.Web.Endpoints;

public class Admins : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetAdmins);
        groupBuilder.MapGet(GetAdminById, "{id}");
        groupBuilder.MapPost(CreateAdmin);
        groupBuilder.MapPut(UpdateAdmin, "{id}");
        groupBuilder.MapDelete(DeleteAdmin, "{id}");
    }

    public async Task<Ok<List<AdminDto>>> GetAdmins(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAdminsQuery(), cancellationToken);
        return TypedResults.Ok(result);
    }

    public async Task<Ok<AdminDto>> GetAdminById(ISender sender, int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAdminByIdQuery { Id = id }, cancellationToken);
        return TypedResults.Ok(result);
    }

    public async Task<Created<int>> CreateAdmin(ISender sender, CreateAdminCommand command, CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return TypedResults.Created($"/api/admins/{id}", id);
    }

    public async Task<Results<NoContent, BadRequest>> UpdateAdmin(ISender sender, int id, UpdateAdminCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return TypedResults.BadRequest();
        }

        await sender.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    public async Task<NoContent> DeleteAdmin(ISender sender, int id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteAdminCommand(id), cancellationToken);
        return TypedResults.NoContent();
    }
}
