namespace CrudBuilder.FileBuilders.Application;
public static class FileApplicationBuilder
{
    public static void ApplicationBuilder()
    {
        Console.WriteLine($"Starting {nameof(ApplicationBuilder)}");

        CreateBuilder.CreateCommandBuilder();
        CreateBuilder.CreateCommandHandlerBuilder();

        UpdateBuilder.UpdateCommandBuilder();
        UpdateBuilder.UpdateCommandHandlerBuilder();

        DeleteBuilder.DeleteCommandBuilder();
        DeleteBuilder.DeleteCommandHandlerBuilder();

        GetAllBuilder.GetAllQueryBuilder();
        GetAllBuilder.GetAllQueryHandlerBuilder();

        GetByIdBuilder.GetByIdQueryBuilder();
        GetByIdBuilder.GetByIdQueryHandlerBuilder();

        // Mapper Profile 
        MapperBuilder.BuildMapper();
        DtoBuilder.BuildDto();

        Console.WriteLine($"{nameof(ApplicationBuilder)} Done");
    }







}
