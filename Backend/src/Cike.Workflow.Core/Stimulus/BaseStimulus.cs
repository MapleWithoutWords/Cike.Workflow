using Beaver.Infrastructure.Domain.Shared.Stimulus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Core.Stimulus;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(SuspendStimulus), nameof(SuspendStimulus))]
public abstract record BaseStimulus
{
}
