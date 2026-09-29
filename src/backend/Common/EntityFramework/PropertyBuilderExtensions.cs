/*
 * Not necessary since we are now registring the converters in the DbContext ConfigureConventions method.
 */

//using Microsoft.EntityFrameworkCore.Metadata.Builders;
//using PAS.Domain;
//using PAS.EntityFramework.Converters;

//namespace PAS.EntityFramework;

//public static class PropertyBuilderExtensions {

//    public static PropertyBuilder<TId> HasStrongTypedIdConversion<TId, TValue>(this PropertyBuilder<TId> propertyBuilder)
//            where TId : struct, IStronglyTypedId<TValue>
//            where TValue : notnull {

//        return propertyBuilder.HasConversion<StronglyTypedIdValueConverter<TId, TValue>>();
//    }
//}
