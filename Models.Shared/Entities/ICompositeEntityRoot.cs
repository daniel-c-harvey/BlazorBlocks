namespace Models.Shared.Entities;

public interface ICompositeEntityRoot<TDiscriminator> : IEntity
{
    TDiscriminator Discriminator { get; set; }
}