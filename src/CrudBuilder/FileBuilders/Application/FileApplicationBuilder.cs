namespace CrudBuilder.FileBuilders.Application;
public static class FileApplicationBuilder
{
    public static void ApplicationBuilder(string EntityName)
    {
        Console.WriteLine($"Starting {nameof(ApplicationBuilder)}");

        CreateBuilder.CreateCommandBuilder(EntityName);
        CreateBuilder.CreateCommandHandlerBuilder(EntityName);

        UpdateBuilder.UpdateCommandBuilder(EntityName);
        UpdateBuilder.UpdateCommandHandlerBuilder(EntityName);

        DeleteBuilder.DeleteCommandBuilder(EntityName);
        DeleteBuilder.DeleteCommandHandlerBuilder(EntityName);

        Console.WriteLine($"{nameof(ApplicationBuilder)} Done");
    }







}
