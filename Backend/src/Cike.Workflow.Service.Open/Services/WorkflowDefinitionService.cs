using Cike.AspNetCore.MinimalAPIs.EndpointFilters;
using Cike.EventBus.Local;
using Cike.Workflow.Application.Contracts.WorkflowDefinitions;
using Cike.Workflow.Application.WorkflowDefinitions.Commands;
using Cike.Workflow.Application.WorkflowDefinitions.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Cike.Workflow.Service.Open.Services;

[AutoValidation]
public class WorkflowDefinitionService : MinimalApiServiceBase
{
    public async Task<Results<Ok<List<WorkflowDefinitionFolderItemDto>>, BadRequest>> GetListAsync(
        [FromServices] ILocalEventBus localEventBus,
        long workspaceId,
        long folderId,
        string? keyword,
        string sorting = "CreatedAt desc",
        CancellationToken cancellationToken = default)
    {
        var query = new GetWorkflowDefinitionFolderListQuery(workspaceId, folderId, keyword, sorting);
        await localEventBus.PublishAsync(query, cancellationToken);
        return TypedResults.Ok(query.Result);
    }

    public async Task<Results<Ok<WorkflowDefinitionDetailDto>, BadRequest>> GetAsync(
        [FromServices] ILocalEventBus localEventBus,
        long id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetWorkflowDefinitionQuery(id);
        await localEventBus.PublishAsync(query, cancellationToken);
        return TypedResults.Ok(query.Result);
    }

    public async Task<Results<Ok<List<WorkflowDefinitionVersionItemDto>>, BadRequest>> GetVersionListAsync(
        [FromServices] ILocalEventBus localEventBus,
        string definitionId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetWorkflowDefinitionVersionListQuery(definitionId);
        await localEventBus.PublishAsync(query, cancellationToken);
        return TypedResults.Ok(query.Result);
    }

    public async Task<Results<Ok<long>, BadRequest>> AddAsync(
        [FromServices] ILocalEventBus localEventBus,
        AddWorkflowDefinitionDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new AddWorkflowDefinitionCommand(dto);
        await localEventBus.PublishAsync(command, cancellationToken);
        return TypedResults.Ok(command.Id);
    }

    public async Task<Results<Ok<long>, BadRequest>> SaveAsync(
        [FromServices] ILocalEventBus localEventBus,
        long id,
        SaveWorkflowDefinitionDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new SaveWorkflowDefinitionCommand(id, dto);
        await localEventBus.PublishAsync(command, cancellationToken);
        return TypedResults.Ok(command.DraftId);
    }

    public async Task<Results<Ok, BadRequest>> PublishAsync(
        [FromServices] ILocalEventBus localEventBus,
        long id,
        PublishWorkflowDefinitionDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new PublishWorkflowDefinitionCommand(id, dto.PublishedNote);
        await localEventBus.PublishAsync(command, cancellationToken);
        return TypedResults.Ok();
    }

    public async Task<Results<Ok, BadRequest>> MoveAsync(
        [FromServices] ILocalEventBus localEventBus,
        long id,
        MoveWorkflowDefinitionDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new MoveWorkflowDefinitionCommand(id, dto.FolderId);
        await localEventBus.PublishAsync(command, cancellationToken);
        return TypedResults.Ok();
    }

    public async Task<Results<Ok, BadRequest>> RollbackAsync(
        [FromServices] ILocalEventBus localEventBus,
        RollbackWorkflowDefinitionDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new RollbackWorkflowDefinitionCommand(dto.DefinitionId, dto.DefinitionVersionId);
        await localEventBus.PublishAsync(command, cancellationToken);
        return TypedResults.Ok();
    }

    public async Task<Results<Ok, BadRequest>> UpdateAsync(
        [FromServices] ILocalEventBus localEventBus,
        long id,
        UpdateWorkflowDefinitionDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateWorkflowDefinitionCommand(id, dto);
        await localEventBus.PublishAsync(command, cancellationToken);
        return TypedResults.Ok();
    }

    public async Task<Results<Ok, BadRequest>> DeleteAsync(
        [FromServices] ILocalEventBus localEventBus,
        long id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteWorkflowDefinitionCommand(id);
        await localEventBus.PublishAsync(command, cancellationToken);
        return TypedResults.Ok();
    }
}
