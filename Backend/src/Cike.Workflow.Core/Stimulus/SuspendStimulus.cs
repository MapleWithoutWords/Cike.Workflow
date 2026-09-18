using Cike.Workflow.Core.Stimulus;

namespace Beaver.Infrastructure.Domain.Shared.Stimulus;

public record SuspendStimulus(object Payload) : BaseStimulus
{
}
