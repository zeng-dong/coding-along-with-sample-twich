using System;

namespace Sample.Contracts
{
    public interface OrderSubmissionAccepted
    {
        Guid OrderId { get; }
        DateTime Timestamp { get; }
        
        string CustomerNumber { get; }
    }
    
    public interface OrderSubmissionRejected
    {
        Guid OrderId { get; }
        DateTime Timestamp { get; }
        
        string CustomerNumber { get; }
        string Reason { get; }
    }
}