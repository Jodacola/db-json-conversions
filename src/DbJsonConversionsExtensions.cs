using System;
using System.Linq.Expressions;
using System.Reflection;
using Jodacola.DbJsonConversions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

public static class DbJsonConversionsExtensions
{
    /// <summary>
    ///     <para>
    ///         Automatically adds JSON serialization and deserialization ValueConverters to any model properties
    ///         which have the JsonConversion attribute applied.
    /// 
    ///     </para>
    ///     <para>
    ///         If using in conjuction with UnderscoreDatabase, ensure this method is called before UnderscoreDatabase,
    ///         in order to ensure convertible fields are properly included in the underscoring process.
    ///     </para>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="builder"></param>
    public static void AddJsonConversions(this DbContext context, ModelBuilder builder)
    {
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            foreach (var prop in entity.ClrType.GetTypeInfo().GetProperties())
            {
                if (prop.GetCustomAttribute<JsonConversionAttribute>() != null)
                {
                    builder.Entity(entity.ClrType).Property(prop.Name).HasConversion(CreateJsonValueConverter(prop.PropertyType));
                }
            }
        }
    }

    /// <summary>
    ///     <para>
    ///         Constructs a ValueConverter which serializes and deserializes to and from JSON for the provided type.
    /// 
    ///     </para>
    ///     <para>
    ///         Uses Newtonsoft.Json for serialization and deserialization.
    ///     </para>
    /// </summary>
    /// <param name="deserializedType">The target type to which the ValueConverter deserializes.</param>
    /// <returns></returns>
    public static ValueConverter CreateJsonValueConverter(Type deserializedType)
    {
        ParameterExpression serializingParameter = Expression.Parameter(deserializedType);
        ParameterExpression deserializingParameter = Expression.Parameter(typeof(string));
        LambdaExpression serializingExpression = Expression.Lambda(
            Expression.Call(typeof(JsonConvert).GetMethod("SerializeObject", new Type[] { typeof(object) }), serializingParameter), serializingParameter);
        LambdaExpression deserializingExpression = Expression.Lambda(
            Expression.Call(typeof(JsonConvert), "DeserializeObject", new Type[] { deserializedType }, deserializingParameter), deserializingParameter);

        var constructedConverterType = typeof(ValueConverter<,>).MakeGenericType(deserializedType, typeof(string));
        return Activator.CreateInstance(constructedConverterType, new object[] { serializingExpression, deserializingExpression, null }) as ValueConverter;
    }
}
