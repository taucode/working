using Serilog.Core;
using Serilog.Events;
using System.Text;
using TauCode.Infrastructure.Logging;

namespace TauCode.Working.Slavery;

public class SlaveObjectTagEnricher : ObjectTagEnricher
{
    public SlaveObjectTagEnricher(Func<ObjectTag> tagGetter)
        : base(tagGetter)
    {
    }

    protected override LogEventProperty BuildProperty(ObjectTag tag, ILogEventPropertyFactory propertyFactory)
    {
        var sb = new StringBuilder();

        sb.Append(" (");
        if (tag.Type != null)
        {
            sb.Append(tag.Type);
            if (tag.Name != null)
            {
                sb.Append(" ");
            }
        }

        if (tag.Name != null)
        {
            sb.Append($"'{tag.Name}'");
        }

        sb.Append(")");

        var text = sb.ToString();

        var property = propertyFactory.CreateProperty(PropertyName, text);
        return property;
    }
}