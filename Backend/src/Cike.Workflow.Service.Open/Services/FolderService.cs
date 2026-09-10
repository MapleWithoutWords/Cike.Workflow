using Cike.AspNetCore.MinimalAPIs.EndpointFilters;
using Cike.EventBus.Local;
using Cike.Workflow.Application.Contracts.Folders;
using Cike.Workflow.Application.Folders.Commands;
using Cike.Workflow.Application.Folders.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Cike.Workflow.Service.Open.Services;

[AutoValidation]
public class FolderService : MinimalApiServiceBase
{
    public async Task<Results<Ok<FolderDetailDto>, BadRequest>> GetAsync(
        [FromServices] ILocalEventBus localEventBus,
        long folderId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetFolderQuery(folderId);
        await localEventBus.PublishAsync(query, cancellationToken);
        return TypedResults.Ok(query.Result);
    }

    public async Task<Results<Ok<long>, BadRequest>> AddAsync(
        [FromServices] ILocalEventBus localEventBus,
        AddFolderDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new AddFolderCommand(dto);
        await localEventBus.PublishAsync(command, cancellationToken);
        return TypedResults.Ok(command.Id);
    }

    public async Task<Results<Ok, BadRequest>> UpdateAsync(
        [FromServices] ILocalEventBus localEventBus,
        long folderId,
        UpdateFolderDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateFolderCommand(folderId, dto);
        await localEventBus.PublishAsync(command, cancellationToken);
        return TypedResults.Ok();
    }

    public async Task<Results<Ok, BadRequest>> DeleteAsync(
        [FromServices] ILocalEventBus localEventBus,
        long folderId,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteFolderCommand(folderId);
        await localEventBus.PublishAsync(command, cancellationToken);
        return TypedResults.Ok();
    }
}
