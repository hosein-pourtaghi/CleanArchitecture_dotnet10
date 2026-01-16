// See https://aka.ms/new-console-template for more information
//using System.Reflection;

using CrudBuilder;
using CrudBuilder.FileBuilders.Application;
using CrudBuilder.FileBuilders.Persistance;
using CrudBuilder.FileBuilders.Web;

//var aa =new EntityReader();
//    aa.CreateClassFromType();

Console.WriteLine($"this is your base path : {MyPath.AbsoluteBasePath}");

Console.WriteLine($"Enter EntityName");
var EntityName = Console.ReadLine();
MyPath.EntityName = String.IsNullOrEmpty(EntityName) ? "MyEntity" : EntityName;

FileApplicationBuilder.ApplicationBuilder();
FileConfigurationBuilder.CreateConfiguration();
FileControllerBuilder.CreateController();


//Console.WriteLine("Hello, World!");

//var path = Directory.GetCurrentDirectory();
//Console.WriteLine($"GetCurrentDirectory {path}");

//var appContext = AppContext.BaseDirectory;
//var aa = Assembly.GetExecutingAssembly().GetName().Name;

//Console.WriteLine($"appContext : {appContext} , aa:{aa}");



