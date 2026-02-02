namespace ExportOrderWebServer.Areas.ExportOrder.Mapper;

public static class ExportOrderMapper
{
    public static ExportOrderEntity ToEntity(this ExportOrderDto dto)
    {
        return new ExportOrderEntity
        {
            Id = dto.Id,
            Num = dto.Num,
            Dated = dto.Dated,

        };
    }
}
