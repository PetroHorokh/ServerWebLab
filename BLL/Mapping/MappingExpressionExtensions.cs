using AutoMapper;
using DAL.Entities;

namespace BLL.Mapping;

internal static class MappingExpressionExtensions
{
    public static IMappingExpression<TSource, TDestination> IgnoreAuditFields<TSource, TDestination>(
        this IMappingExpression<TSource, TDestination> expression)
        where TDestination : AuditableEntity =>
        expression
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.CreatedDateTime, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedDateTime, o => o.Ignore());
}
