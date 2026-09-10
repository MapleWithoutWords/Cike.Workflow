using Cike.AspNetCore.MinimalAPIs.EndpointFilters;
using Cike.EventBus.Local;
using Cike.Contracts.EntityDtos;
using Cike.Workflow.Application.Contracts.Workspaces;
using Cike.Workflow.Application.Workspaces.Commands;
using Cike.Workflow.Application.Workspaces.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Cike.Workflow.Service.Open.Services;

[AutoValidation]
public class WorkspaceService : MinimalApiServiceBase
{
    public async Task<Results<Ok<PagedResultDto<WorkspaceItemDto>>, BadRequest>> GetPagedListAsync(
        [FromServices] ILocalEventBus localEventBus,
        string? keyword,
        [AsParameters] PagedAndSortedResultRequest pageDto,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPagedWorkspaceListQuery(keyword, pageDto);
        await localEventBus.PublishAsync(query, cancellationToken);
        return TypedResults.Ok(query.Result);
    }

    public async Task<Results<Ok<WorkspaceItemDto>, BadRequest>> GetAsync(
        [FromServices] ILocalEventBus localEventBus,
        long workspaceId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetWorkspaceQuery(workspaceId);
        await localEventBus.PublishAsync(query, cancellationToken);
        return TypedResults.Ok(query.Result);
    }

    public async Task<Results<Ok<long>, BadRequest>> AddAsync(
        [FromServices] ILocalEventBus localEventBus,
        AddWorkspaceDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new AddWorkspaceCommand(dto);
        await localEventBus.PublishAsync(command, cancellationToken);
        return TypedResults.Ok(command.Id);
    }

    public async Task<Results<Ok, BadRequest>> UpdateAsync(
        [FromServices] ILocalEventBus localEventBus,
        long workspaceId,
        UpdateWorkspaceDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateWorkspaceCommand(workspaceId, dto);
        await localEventBus.PublishAsync(command, cancellationToken);
        return TypedResults.Ok();
    }

    public async Task<Results<Ok, BadRequest>> DeleteAsync(
        [FromServices] ILocalEventBus localEventBus,
        long workspaceId,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteWorkspaceCommand(workspaceId);
        await localEventBus.PublishAsync(command, cancellationToken);
        return TypedResults.Ok();
    }
}
