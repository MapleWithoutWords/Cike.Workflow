namespace Cike.Workflow.Core.WorkflowGraphs.Models;

public class ActivityNode
{
    private readonly List<ActivityNode> _parents = new();
    private readonly List<ActivityNode> _children = new();
    private string? _nodeId;

    public ActivityNode(IActivity activity, string port)
    {
        Activity = activity;
        Port = port;
    }

    public string NodeId
    {
        get
        {
            if (_nodeId == null)
            {
                var ancestorIds = Ancestors().Reverse().Select(x => x.Activity.Id).ToList();
                _nodeId = ancestorIds.Any() ? $"{string.Join(":", ancestorIds)}:{Activity.Id}" : Activity.Id;
            }

            return _nodeId;
        }
    }

    public IActivity Activity { get; }

    public string Port { get; }

    public IReadOnlyCollection<ActivityNode> Parents => _parents.AsReadOnly();

    public ICollection<ActivityNode> Children => _children.AsReadOnly();

    public void AddParent(ActivityNode parent)
    {
        _parents.Add(parent);
        _nodeId = null;
    }

    public void AddChild(ActivityNode child)
    {
        _children.Add(child);
    }

    public IEnumerable<ActivityNode> Descendants()
    {
        foreach (var child in Children)
        {
            yield return child;

            var descendants = child.Descendants();

            foreach (var descendant in descendants)
                yield return descendant;
        }
    }

    public IEnumerable<ActivityNode> Ancestors()
    {
        foreach (var parent in Parents)
        {
            yield return parent;

            var ancestors = parent.Ancestors();

            foreach (var ancestor in ancestors)
                yield return ancestor;
        }
    }

    public IEnumerable<ActivityNode> Siblings() => Parents.SelectMany(parent => parent.Children);

    public IEnumerable<ActivityNode> SiblingsAndCousins() => Parents.SelectMany(parent => parent.Descendants());

    public IEnumerable<ActivityNode> Flatten()
    {
        yield return this;

        foreach (var node in this.Children)
        {
            var children = node.Flatten();

            foreach (var child in children)
                yield return child;
        }
    }
}
