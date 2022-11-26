using Unity.Entities;

namespace DotsEffect
{
    public struct DotSpawner : IComponentData
    {
        public int maxDotsCount;
        public Entity dot;
    }
}
