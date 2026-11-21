using FolderHub.Infrastructure.Ipc;

namespace FolderHub.Infrastructure.Tests;

public sealed class CardHostPendingClosesFileTests
{
    [Fact]
    public void EnqueuePeekRemove_RoundTrip()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();

        // 隔离：先清掉可能残留。
        foreach (var id in CardHostPendingClosesFile.Peek().ToList())
            CardHostPendingClosesFile.Remove(id);

        CardHostPendingClosesFile.Enqueue(a);
        CardHostPendingClosesFile.Enqueue(b);
        CardHostPendingClosesFile.Enqueue(a); // 幂等

        var peek = CardHostPendingClosesFile.Peek();
        Assert.Contains(a, peek);
        Assert.Contains(b, peek);
        Assert.Equal(2, peek.Count);

        CardHostPendingClosesFile.Remove(a);
        Assert.DoesNotContain(a, CardHostPendingClosesFile.Peek());
        Assert.Contains(b, CardHostPendingClosesFile.Peek());

        var consumed = CardHostPendingClosesFile.ConsumeAll();
        Assert.Single(consumed);
        Assert.Equal(b, consumed[0]);
        Assert.Empty(CardHostPendingClosesFile.Peek());
    }
}
