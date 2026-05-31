using System.Threading.Channels;

namespace CADSFINANCE.Services.Application.ReportInfrastructure;

public sealed class InMemoryReportExportQueue : IReportExportQueue
{
    private readonly Channel<ReportExportWorkItem> _channel = Channel.CreateUnbounded<ReportExportWorkItem>();

    public ValueTask EnqueueAsync(ReportExportWorkItem workItem, CancellationToken cancellationToken = default)
    {
        return _channel.Writer.WriteAsync(workItem, cancellationToken);
    }

    public ValueTask<ReportExportWorkItem> DequeueAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAsync(cancellationToken);
    }
}
