using APIDataSyncXMLGenerator;
using APIDataSyncXMLGenerator.Models;
using System.Configuration;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
           .MinimumLevel.Debug()
           .WriteTo.Console()
           .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 3)
           .CreateLogger();

try
{
    Log.Information("Program started.");

    int fetchInterval = Convert.ToInt32(ConfigurationManager.AppSettings["ApiFetchIntervalSeconds"]);
    DatabaseService dbService = new DatabaseService();
    List<Product> products = new List<Product>();

    string inputPath = Path.Combine(Directory.GetCurrentDirectory(), "input");

    if (!Directory.Exists(inputPath))
    {
        Directory.CreateDirectory(inputPath);
        Log.Warning("Created missing directory: {InputPath}", inputPath);
    }

    string[] files = Directory.GetFiles(inputPath, "*.txt");

    if (!files.Any())
    {
        Log.Warning("No files found in /input folder!");
        return;
    }

    foreach (string file in files)
    {
        Log.Information("Processing file: {FileName}", file);
        List<string> codes = FileService.ReadFile(file);

        for (int i = 0; i < codes.Count; i++)
        {
            string code = codes[i].Trim();

            Console.WriteLine("");
            Console.WriteLine("-------------------------------------------------------------------------------------------------------------");
            Console.WriteLine("");
            Log.Information("Processing code: {ProductCode}", code);

            int id = await dbService.GetIdFromName(code);
            if (id > 0)
            {
                var productDetails = await ApiService.FetchProductsData(id);
                if (productDetails != null)
                {
                    products.Add(productDetails);
                    bool updatedSuccessfully = await dbService.InsertOrUpdateProductDetails(id, productDetails);

                    if (updatedSuccessfully)
                    {
                        Log.Information("Successfully updated database for product ID {ProductId}", id);
                    }
                    else
                    {
                        Log.Warning("Partial update failure for product ID {ProductId}", id);
                    }
                }
            }
            else
            {
                Log.Error("ID not found for code {ProductCode}", code);
            }

            if (i < codes.Count - 1)
            {
                Log.Information("Waiting {FetchInterval} seconds before next fetch.", fetchInterval);
                await Task.Delay(fetchInterval * 1000);
            }
        }
        Console.WriteLine("");
        Console.WriteLine("-------------------------------------------------------------------------------------------------------------");
        Console.WriteLine("");
        FileService.MakeXMLFile(file, products);
        Log.Information("XML file created for: {FileName}", file);
    }
    Console.WriteLine("Press Enter to close");
    Console.ReadLine();
}
catch (Exception ex)
{
    Log.Fatal(ex, "A fatal error occurred.");
}
finally
{
    Log.CloseAndFlush();
}