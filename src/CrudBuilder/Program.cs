// See https://aka.ms/new-console-template for more information
//using System.Reflection;

using CrudBuilder;
using CrudBuilder.FileBuilders.Application;


Console.WriteLine($"this is your base path : {BuilderPath.AbsoluteBasePath}");
Console.WriteLine($"to continue press ENTRT");
var enter = Console.ReadLine();

if (!string.IsNullOrWhiteSpace(enter))
{
    return;
}


Console.WriteLine($"Enter EntityName");
var EntityName = Console.ReadLine()  ;
EntityName = String.IsNullOrEmpty(EntityName) ? "MyEntity" : EntityName;
FileApplicationBuilder.ApplicationBuilder(EntityName);


//Console.WriteLine("Hello, World!");

//var path = Directory.GetCurrentDirectory();
//Console.WriteLine($"GetCurrentDirectory {path}");

//var appContext = AppContext.BaseDirectory;
//var aa = Assembly.GetExecutingAssembly().GetName().Name;

//Console.WriteLine($"appContext : {appContext} , aa:{aa}");



