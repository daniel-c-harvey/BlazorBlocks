namespace Models.Shared.Entities;

public interface ICompositeEntity<TRoot, TDiscriminator, TInfo> : IKeyed
where TRoot : ICompositeEntityRoot<TDiscriminator>
where TInfo : IEntity
{

    TRoot Root { get; set; }
    TInfo Info { get; set; }
}