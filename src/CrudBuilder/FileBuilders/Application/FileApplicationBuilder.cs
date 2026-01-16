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

        GetAllBuilder.GetAllCommandBuilder();
        GetAllBuilder.GetAllCommandHandlerBuilder();

        GetByIdBuilder.GetByIdCommandBuilder();
        GetByIdBuilder.GetByIdCommandHandlerBuilder();

        // Mapper Profile 
        MapperBuilder.BuildMapper();
        DtoBuilder.BuildDto();

        Console.WriteLine($"{nameof(ApplicationBuilder)} Done");
    }







}
