using IdaraTech.Application.Documents.Commands.CreateDocument;
using IdaraTech.Application.Documents.Commands.DeleteDocument;
using IdaraTech.Application.Documents.Commands.UpdateDocument;
using IdaraTech.Application.Documents.Queries.GetDocuments;
using IdaraTech.Application.Documents.Queries.GetDocumentsList;
using Microsoft.AspNetCore.Http.HttpResults;

namespace IdaraTech.Web.Endpoints;

public class Documents : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetDocuments);
        groupBuilder.MapPost(CreateDocument);
        groupBuilder.MapPut(UpdateDocument, "{id}");
        groupBuilder.MapDelete(DeleteDocument, "{id}");
    }

    public async Task<Ok<List<DocumentDto>>> GetDocuments(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetDocumentsQuery(), cancellationToken);
        return TypedResults.Ok(result);
    }

    public async Task<Created<int>> CreateDocument(ISender sender, CreateDocumentCommand command, CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return TypedResults.Created($"/api/documents/{id}", id);
    }

    public async Task<Results<NoContent, BadRequest>> UpdateDocument(ISender sender, int id, UpdateDocumentCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return TypedResults.BadRequest();
        }

        await sender.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    public async Task<NoContent> DeleteDocument(ISender sender, int id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteDocumentCommand(id), cancellationToken);
        return TypedResults.NoContent();
    }
}
