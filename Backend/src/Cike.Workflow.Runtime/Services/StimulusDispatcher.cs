using Cike.Core.DependencyInjection;
using Cike.EventBus.Local;
using Cike.Workflow.Runtime.Internals.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Runtime.Internals;

internal class StimulusDispatcher(ILocalEventBus localEventBus, IStimulusSender stimulusSender) : IStimulusDispatcher, IScopedDependency
{
    public async Task SendAsync(DispatchStimulusRequest request, CancellationToken cancellationToken = default)
    {
        await localEventBus.PublishAsync(new DispatchStimulusCommand(request), cancellationToken);
    }

    [LocalEventHandler]
    public async Task HandlerDispatchStimulusAsync(DispatchStimulusCommand command, CancellationToken cancellationToken = default)
    {
        var request = command.Request;
        if (request.ActivityTypeName != null)
        {
            await stimulusSender.SendAsync(request.ActivityTypeName, request.Stimulus!, request.Metadata, cancellationToken);
            return;
        }

        if (request.StimulusHash != null)
        {
            await stimulusSender.SendAsync(request.StimulusHash!, request.Metadata, cancellationToken);
            return;
        }
    }
}
