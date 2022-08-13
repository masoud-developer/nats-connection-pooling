namespace MicroEthos.Common.Contracts;

public interface IPool<TItem>
{
    TItem Acquire();
    void Release(TItem item);
}