using System.Linq.Expressions;
using Cike.EventBus.Local;
using Cike.UniversalId.ULong;
using Cike.Uow;
using Cike.Workflow.Core.Activities;
using Cike.Workflow.Core.Enums;
using Cike.Workflow.Core.Models;
using Cike.Workflow.Core.Runners;
using Cike.Workflow.Core.Runners.Models;
using Cike.Workflow.Core.WorkflowGraphs.Models;
using Cike.Workflow.Domain.Data;
using Cike.Workflow.Domain.Data.Entities;
using Cike.Workflow.Domain.Managers;
using Cike.Workflow.Domain.Managers.Mappers;
using Cike.Workflow.Runtime;
using Cike.Workflow.Runtime.Exceptions;
using Cike.Workflow.Runtime.Internals;
using Cike.Workflow.Runtime.Models;
using Cike.Workflow.Runtime.WorkflowDefintions;
using Cike.Locks.Abstracts;
using NSubstitute;

namespace Cike.Workflow.Core.Tests.Runtime.Internals;

[TestFixture]
public class WorkflowClientTest
{
    private const long SnowflakeId = 777;

    private IWorkflowDefinitionService _definitionService = null!;
    private IWorkflowInstanceRepository _repository = null!;
    private IWorkflowRunner _runner = null!;
    private ISnowflakeIdGenerator _identityGenerator = null!;
    private ILock _lock = null!;
    private IWorkflowStateExtractor _stateExtractor = null!;
    private WorkflowInstanceManager _manager = null!;

    [SetUp]
    public void SetUp()
    {
        _definitionService = Substitute.For<IWorkflowDefinitionService>();
        _repository = Substitute.For<IWorkflowInstanceRepository>();
        _runner = Substitute.For<IWorkflowRunner>();
        _identityGenerator = Substitute.For<ISnowflakeIdGenerator>();
        _lock = Substitute.For<ILock>();
        var lockHandle = Substitute.For<IAsyncDisposable>();
        _lock.TryGetAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(lockHandle);
        _identityGenerator.NextId().Returns(SnowflakeId);
        _stateExtractor = Substitute.For<IWorkflowStateExtractor>();
        _manager = new WorkflowInstanceManager(
            _repository,
            new WorkflowInstanceFactory(_identityGenerator),
            Substitute.For<IUnitOfWork>(),
            new WorkflowStateMapper());
    }

    private WorkflowClient CreateClient(long? workflowInstanceId = null)
    {
        return new WorkflowClient(
            workflowInstanceId,
            _definitionService,
            _manager,
            _repository,
            _runner,
            _identityGenerator,
            _lock,
            Substitute.For<ILocalEventBus>(),
            Substitute.For<IServiceProvider>());
    }

    private static WorkflowInstance CreateInstance(long id, WorkflowStatus status)
    {
        return new WorkflowInstance
        {
            Id = id,
            DefinitionId = "def-1",
            DefinitionVersionId = 1,
            Version = 1,
            Status = status,
            WorkflowState = new WorkflowState { Id = id, Status = status }
        };
    }

    private static WorkflowGraph CreateTestGraph()
    {
        var workflow = new WorkflowActivity
        {
            DefinitionInfo = new WorkflowDefinitionInfo
            {
                Id = 1,
                DefinitionId = "def-1",
                Version = 1,
            }
        };
        var rootActivity = new WriteLine("root") { Id = "root", NodeId = "root", Code = "root" };
        var rootNode = new ActivityNode(rootActivity, "");
        return new WorkflowGraph(workflow, rootNode, new[] { rootNode });
    }

    [Test]
    public void WorkflowInstanceId_WhenCreatedWithId_UsesGivenId()
    {
        var client = CreateClient(42);

        Assert.That(client.WorkflowInstanceId, Is.EqualTo(42));
    }

    [Test]
    public void WorkflowInstanceId_WhenCreatedWithoutId_PregeneratesSnowflakeId()
    {
        var client = CreateClient(null);

        Assert.That(client.WorkflowInstanceId, Is.EqualTo(SnowflakeId));
    }

    [Test]
    public async Task InstanceExistsAsync_WhenRepositoryReportsExisting_ReturnsTrue()
    {
        _repository.AnyAsync(Arg.Any<Expression<Func<WorkflowInstance, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(true);
        var client = CreateClient(42);

        var exists = await client.InstanceExistsAsync();

        Assert.That(exists, Is.True);
    }

    [Test]
    public async Task InstanceExistsAsync_WhenRepositoryReportsMissing_ReturnsFalse()
    {
        _repository.AnyAsync(Arg.Any<Expression<Func<WorkflowInstance, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(false);
        var client = CreateClient(42);

        var exists = await client.InstanceExistsAsync();

        Assert.That(exists, Is.False);
    }

    [Test]
    public void RunInstanceAsync_WhenInstanceNotExists_ThrowsWorkflowInstanceNotFoundException()
    {
        _repository.FindAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns((WorkflowInstance?)null);
        var client = CreateClient(42);

        Assert.That(async () => await client.RunInstanceAsync(RunWorkflowInstanceRequest.Empty), Throws.TypeOf<WorkflowInstanceNotFoundException>());
    }

    [Test]
    public async Task RunInstanceAsync_WhenInstanceIsFinished_ReturnsCurrentStatusWithoutRunning()
    {
        var instance = CreateInstance(42, WorkflowStatus.Finished);
        _repository.FindAsync(42, Arg.Any<CancellationToken>()).Returns(instance);
        var client = CreateClient(42);

        var response = await client.RunInstanceAsync(RunWorkflowInstanceRequest.Empty);

        Assert.That(response.WorkflowInstanceId, Is.EqualTo(42));
        Assert.That(response.Status, Is.EqualTo(WorkflowStatus.Finished));
        await _runner.DidNotReceive().RunAsync(Arg.Any<WorkflowGraph>(), Arg.Any<WorkflowState>(), Arg.Any<RunWorkflowOptions>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task RunInstanceAsync_WhenInstanceSuspended_RunsGraphAndPersistsCommittedState()
    {
        var instance = CreateInstance(42, WorkflowStatus.Suspended);
        _repository.FindAsync(42, Arg.Any<CancellationToken>()).Returns(instance);
        _repository.AnyAsync(Arg.Any<Expression<Func<WorkflowInstance, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(true);
        var graph = CreateTestGraph();
        _definitionService.GetWorkflowGraphAsync(Arg.Any<WorkflowDefinitionHandle>(), Arg.Any<CancellationToken>())
            .Returns(graph);
        var postRunState = new WorkflowState
        {
            Id = 42,
            Status = WorkflowStatus.Executing,
            Output = new Dictionary<string, object> { ["Result"] = "done" },
        };
        _runner.RunAsync(Arg.Any<WorkflowGraph>(), Arg.Any<WorkflowState>(), Arg.Any<RunWorkflowOptions>(), Arg.Any<CancellationToken>())
            .Returns(new RunWorkflowResult(null!, postRunState, null!, null, Journal.Empty));
        var client = CreateClient(42);

        var response = await client.RunInstanceAsync(new RunWorkflowInstanceRequest
        {
            BookmarkId = 99,
            IncludeWorkflowOutput = true,
        });

        Assert.That(response.WorkflowInstanceId, Is.EqualTo(42));
        Assert.That(response.Status, Is.EqualTo(WorkflowStatus.Executing));
        Assert.That(response.Output, Is.Not.Null);
        Assert.That(response.Output!["Result"], Is.EqualTo("done"));
        await _runner.Received(1).RunAsync(
            Arg.Any<WorkflowGraph>(),
            Arg.Is<WorkflowState>(s => s.Id == 42),
            Arg.Is<RunWorkflowOptions>(o => o.BookmarkId == 99),
            Arg.Any<CancellationToken>());
        await _repository.Received(1).UpdateAsync(
            Arg.Is<WorkflowInstance>(e => e.WorkflowState == postRunState),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task CreateAndRunInstanceAsync_WhenCreated_CommitsPendingInstanceThenPersistsPostRunState()
    {
        _repository.AnyAsync(Arg.Any<Expression<Func<WorkflowInstance, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(true);
        var graph = CreateTestGraph();
        _definitionService.GetWorkflowGraphAsync(Arg.Any<WorkflowDefinitionHandle>(), Arg.Any<CancellationToken>())
            .Returns(graph);
        var postRunState = new WorkflowState { Id = SnowflakeId, Status = WorkflowStatus.Finished };
        _runner.RunAsync(Arg.Any<WorkflowGraph>(), Arg.Any<WorkflowState>(), Arg.Any<RunWorkflowOptions>(), Arg.Any<CancellationToken>())
            .Returns(new RunWorkflowResult(null!, postRunState, null!, null, Journal.Empty));
        var client = CreateClient(null);

        var response = await client.CreateAndRunInstanceAsync(new CreateAndRunWorkflowInstanceRequest
        {
            WorkflowDefinitionHandle = WorkflowDefinitionHandle.ByDefinitionVersionId(1),
            BookmarkId = 99,
            CorrelationId = "corr-1",
            Name = "My Instance",
            Input = new Dictionary<string, object> { ["A"] = 1 },
            Properties = new Dictionary<string, object> { ["P"] = 2 },
            ParentId = 7,
        });

        Assert.That(response.WorkflowInstanceId, Is.EqualTo(SnowflakeId));
        Assert.That(response.Status, Is.EqualTo(WorkflowStatus.Finished));
        await _repository.Received(1).InsertAsync(
            Arg.Is<WorkflowInstance>(e =>
                e.Id == SnowflakeId &&
                e.Status == WorkflowStatus.Pending &&
                e.CorrelationId == "corr-1" &&
                e.Name == "My Instance" &&
                e.ParentWorkflowInstanceId == 7),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>());
        await _repository.Received(1).UpdateAsync(
            Arg.Is<WorkflowInstance>(e => e.WorkflowState == postRunState),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>());
        await _runner.Received(1).RunAsync(
            Arg.Any<WorkflowGraph>(),
            Arg.Is<WorkflowState>(s => s.Id == SnowflakeId && s.Status == WorkflowStatus.Pending),
            Arg.Is<RunWorkflowOptions>(o => o.BookmarkId == 99),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task CreateAndRunInstanceAsync_WhenIncludeWorkflowOutputSet_ReturnsOutput()
    {
        _repository.AnyAsync(Arg.Any<Expression<Func<WorkflowInstance, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(true);
        var graph = CreateTestGraph();
        _definitionService.GetWorkflowGraphAsync(Arg.Any<WorkflowDefinitionHandle>(), Arg.Any<CancellationToken>())
            .Returns(graph);
        var postRunState = new WorkflowState
        {
            Id = SnowflakeId,
            Status = WorkflowStatus.Finished,
            Output = new Dictionary<string, object> { ["Result"] = "done" },
        };
        _runner.RunAsync(Arg.Any<WorkflowGraph>(), Arg.Any<WorkflowState>(), Arg.Any<RunWorkflowOptions>(), Arg.Any<CancellationToken>())
            .Returns(new RunWorkflowResult(null!, postRunState, null!, null, Journal.Empty));
        var client = CreateClient(null);

        var response = await client.CreateAndRunInstanceAsync(new CreateAndRunWorkflowInstanceRequest
        {
            WorkflowDefinitionHandle = WorkflowDefinitionHandle.ByDefinitionVersionId(1),
            IncludeWorkflowOutput = true,
        });

        Assert.That(response.Output, Is.Not.Null);
        Assert.That(response.Output!["Result"], Is.EqualTo("done"));
    }

    [Test]
    public async Task CreateAndRunInstanceAsync_WhenIncludeWorkflowOutputNotSet_OmitsOutput()
    {
        _repository.AnyAsync(Arg.Any<Expression<Func<WorkflowInstance, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(true);
        var graph = CreateTestGraph();
        _definitionService.GetWorkflowGraphAsync(Arg.Any<WorkflowDefinitionHandle>(), Arg.Any<CancellationToken>())
            .Returns(graph);
        var postRunState = new WorkflowState
        {
            Id = SnowflakeId,
            Status = WorkflowStatus.Finished,
            Output = new Dictionary<string, object> { ["Result"] = "done" },
        };
        _runner.RunAsync(Arg.Any<WorkflowGraph>(), Arg.Any<WorkflowState>(), Arg.Any<RunWorkflowOptions>(), Arg.Any<CancellationToken>())
            .Returns(new RunWorkflowResult(null!, postRunState, null!, null, Journal.Empty));
        var client = CreateClient(null);

        var response = await client.CreateAndRunInstanceAsync(new CreateAndRunWorkflowInstanceRequest
        {
            WorkflowDefinitionHandle = WorkflowDefinitionHandle.ByDefinitionVersionId(1),
        });

        Assert.That(response.Output, Is.Null);
    }

    [Test]
    public async Task RunInstanceAsync_WhenLockHeldByAnotherNode_ReturnsCurrentStateWithoutRunning()
    {
        _lock.TryGetAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns((IAsyncDisposable?)null);
        var instance = CreateInstance(42, WorkflowStatus.Suspended);
        _repository.FindAsync(42, Arg.Any<CancellationToken>()).Returns(instance);
        var client = CreateClient(42);

        var response = await client.RunInstanceAsync(RunWorkflowInstanceRequest.Empty);

        Assert.That(response.WorkflowInstanceId, Is.EqualTo(42));
        Assert.That(response.Status, Is.EqualTo(WorkflowStatus.Suspended));
        _runner.DidNotReceive().RunAsync(Arg.Any<WorkflowGraph>(), Arg.Any<WorkflowState>(), Arg.Any<RunWorkflowOptions>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task CreateAndRunInstanceAsync_WhenLockNotAcquiredAndInstanceMissing_ReturnsEmptyResponseWithoutRunning()
    {
        _lock.TryGetAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns((IAsyncDisposable?)null);
        _repository.FindAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns((WorkflowInstance?)null);
        var client = CreateClient(42);

        var response = await client.CreateAndRunInstanceAsync(new CreateAndRunWorkflowInstanceRequest
        {
            WorkflowDefinitionHandle = WorkflowDefinitionHandle.ByDefinitionVersionId(1),
        });

        Assert.That(response.WorkflowInstanceId, Is.EqualTo(42));
        Assert.That(response.Status, Is.EqualTo(WorkflowStatus.Pending));
        _runner.DidNotReceive().RunAsync(Arg.Any<WorkflowGraph>(), Arg.Any<WorkflowState>(), Arg.Any<RunWorkflowOptions>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task RunInstanceAsync_WhenLockAcquired_UsesSharedKeyWithFiveMinuteTimeoutAndReleasesAfterRun()
    {
        var instance = CreateInstance(42, WorkflowStatus.Suspended);
        _repository.FindAsync(42, Arg.Any<CancellationToken>()).Returns(instance);
        _repository.AnyAsync(Arg.Any<Expression<Func<WorkflowInstance, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(true);
        var graph = CreateTestGraph();
        _definitionService.GetWorkflowGraphAsync(Arg.Any<WorkflowDefinitionHandle>(), Arg.Any<CancellationToken>())
            .Returns(graph);
        var postRunState = new WorkflowState { Id = 42, Status = WorkflowStatus.Executing };
        _runner.RunAsync(Arg.Any<WorkflowGraph>(), Arg.Any<WorkflowState>(), Arg.Any<RunWorkflowOptions>(), Arg.Any<CancellationToken>())
            .Returns(new RunWorkflowResult(null!, postRunState, null!, null, Journal.Empty));
        var handle = Substitute.For<IAsyncDisposable>();
        _lock.TryGetAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(handle);
        var client = CreateClient(42);

        await client.RunInstanceAsync(RunWorkflowInstanceRequest.Empty);

        await _lock.Received(1).TryGetAsync(
            WorkflowInstanceLock.GetKey(42),
            WorkflowInstanceLock.Timeout,
            Arg.Any<CancellationToken>());
        await handle.Received(1).DisposeAsync();
    }
}
