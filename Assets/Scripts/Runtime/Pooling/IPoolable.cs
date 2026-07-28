namespace PayeGames.AsalYolculuk.Pooling
{
    public interface IPoolable
    {
        void OnSpawned(GameObjectPool owner);
        void OnDespawned();
    }
}
