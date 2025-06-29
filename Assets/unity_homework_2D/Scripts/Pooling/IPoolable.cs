namespace Pooling
{
    public interface IPoolable
    {
        void OnGetFromPool();
        void OnReturnToPool();
        void OnCreatedInPool();
        bool CanReturnToPool() => true;
    }
}