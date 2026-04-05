namespace CrudBuilder;
public static class MyPath
{
    public static string AbsoluteBasePath => "F:\\Template Projects\\clean-architecture\\src\\";
    public static string EntityName { get; set; } = "Test";

    public static string ApplicationPath => AbsoluteBasePath + "Application\\";
    public static string PersistancePath => AbsoluteBasePath + "Infrastructure\\Persistence\\Configurations\\";
    public static string MapperPath => ApplicationPath + "Common\\Mappings\\";
    public static string DtoPath => ApplicationPath + "Common\\DTOs\\";
    public static string ControllerPath=> AbsoluteBasePath + "WebApi\\Controllers\\";
}
