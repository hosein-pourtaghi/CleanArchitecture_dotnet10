using Domain.Carts;

namespace CrudBuilder;
public class EntityReader
{
    public void ReadEntityFile()
    {

        #region Fetch class
        var filePath = $"{MyPath.AbsoluteBasePath}Domain\\Carts\\Cart.cs";

        #endregion

        try
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                var searchString = "{";

                string fileContent = reader.ReadToEnd();
                int indexStart = fileContent.IndexOf(searchString, StringComparison.InvariantCulture);

                //var aa = new { MyPath.EntityName }();

                if (indexStart != -1)
                {
                    Console.WriteLine($"The string '{searchString}' was found at index: {indexStart}");
                }
                else
                {
                    Console.WriteLine($"The string '{searchString}' was not found in the file.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    public void CreateClassFromType()
    {
        string className = "Domain.Carts.Cart";  //MyPath.EntityName; // نام کلاس به صورت رشته‌ای


        // فرض می‌کنیم کلاس Order در فضای نام (namespace) Domain وجود دارد.
        // اگر کلاس در فضای نام دیگری قرار دارد، باید آن را مشخص کنید.
        Type type = Type.GetType(className);


        if (type != null)
        {
            // ایجاد یک نمونه از کلاس
            object instance = Activator.CreateInstance(type);

            var instanceaa = instance;
        }
        else
        {
            Console.WriteLine($"Class '{className}' not found.");
        }
    }



}
