using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Data.Errors;
using NetBlocks.Models;

namespace API.Errors;

/// <summary>
/// Registers <see cref="StructuredResultMessage"/> as a polymorphic derived type of
/// <see cref="ResultMessage"/> with <c>System.Text.Json</c>. Without this hook the
/// derived <c>Code</c> / <c>Field</c> / <c>Category</c> properties never make it
/// onto the wire because the declared type on <c>ResultDtoBase.FailureReasons</c>
/// is <c>ResultMessage[]</c>.
///
/// The polymorphism is configured by mutating the resolved <c>JsonTypeInfo</c> at
/// runtime rather than by editing NetBlocks source. No type discriminator is
/// emitted — the derived properties simply appear alongside <c>Message</c> for
/// structured instances and are absent for plain ones, which is backward
/// compatible for existing consumers.
/// </summary>
public static class StructuredResultMessageJsonPolymorphism
{
    public static void AddResultMessagePolymorphism(this DefaultJsonTypeInfoResolver resolver)
    {
        resolver.Modifiers.Add(static typeInfo =>
        {
            if (typeInfo.Type != typeof(ResultMessage)) return;

            typeInfo.PolymorphismOptions = new JsonPolymorphismOptions
            {
                DerivedTypes = { new JsonDerivedType(typeof(StructuredResultMessage)) },
            };
        });
    }
}
