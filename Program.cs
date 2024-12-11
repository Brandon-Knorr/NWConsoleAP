using NLog;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using NorthwindConsole.Model;
using System.ComponentModel.DataAnnotations;
string path = Directory.GetCurrentDirectory() + "//nlog.config";

// create instance of Logger
var logger = LogManager.Setup().LoadConfigurationFromFile(path).GetCurrentClassLogger();

logger.Info("Program started");

do
{
  Console.WriteLine("1) Display Categories");
  Console.WriteLine("2) Add Category");
  Console.WriteLine("3) Display Category and related products");
  Console.WriteLine("4) Display all Categories and their related products");
  Console.WriteLine("\t");
  Console.WriteLine("5) Display Products");
  Console.WriteLine("6) Display all Active Products (in blue)");
  Console.WriteLine("7) Display all Discontinued Products (in red)");
  Console.WriteLine("8) Display a specific Product");
  Console.WriteLine("9) Edit a Product");
  Console.WriteLine("10) Add a Product");
  Console.WriteLine("Enter to quit");
  string? choice = Console.ReadLine();
  Console.Clear();
  logger.Info("Option {choice} selected", choice);

  if (choice == "1")
  {
    // display categories
    var configuration = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json");

    var config = configuration.Build();

    var db = new DataContext();
    var query = db.Categories.OrderBy(p => p.CategoryName);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"{query.Count()} records returned");
    Console.ForegroundColor = ConsoleColor.Magenta;
    foreach (var item in query)
    {
      Console.WriteLine($"{item.CategoryName} - {item.Description}");
    }
    Console.ForegroundColor = ConsoleColor.White;
  }
  else if (choice == "2")
  {
    // Add category
    Category category = new();
    Console.WriteLine("Enter Category Name:");
    category.CategoryName = Console.ReadLine()!;
    Console.WriteLine("Enter the Category Description:");
    category.Description = Console.ReadLine();
    ValidationContext context = new ValidationContext(category, null, null);
    List<ValidationResult> results = new List<ValidationResult>();

    var isValid = Validator.TryValidateObject(category, context, results, true);
    if (isValid)
    {
      var db = new DataContext();
      // check for unique name
      if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
      {
        // generate validation error
        isValid = false;
        results.Add(new ValidationResult("Name exists", ["CategoryName"]));
      }
      else
      {
        logger.Info("Validation passed");
        // save new category to db
        db.Categories.Add(category);
        db.SaveChanges();
        logger.Info($"Category: {category.CategoryName} added successfully");
      }
    }
    if (!isValid)
    {
      foreach (var result in results)
      {
        logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
      }
    }
  }
  else if (choice == "3")
  {
    var db = new DataContext();
    var query = db.Categories.OrderBy(p => p.CategoryId);

    Console.WriteLine("Select the category whose products you want to display:");
    Console.ForegroundColor = ConsoleColor.DarkRed;
    foreach (var item in query)
    {
      Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
    }
    Console.ForegroundColor = ConsoleColor.White;
    int id = int.Parse(Console.ReadLine()!);
    Console.Clear();
    logger.Info($"CategoryId {id} selected");
    Category category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id)!;
    Console.WriteLine($"{category.CategoryName} - {category.Description}");
    foreach (Product p in category.Products)
    {
      Console.WriteLine($"\t{p.ProductName}");
    }
  }
  else if (choice == "4")
  {
    var db = new DataContext();
    var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
    foreach (var item in query)
    {
      Console.WriteLine($"{item.CategoryName}");
      foreach (Product p in item.Products)
      {
        Console.WriteLine($"\t{p.ProductName}");
      }
    }
  }
  else if (choice == "5") 
  {
    // Display all products in DB table
    var db = new DataContext();
    var query = db.Products.OrderBy(p => p.ProductName);
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"{query.Count()} records returned");
    Console.ForegroundColor = ConsoleColor.Magenta;
    foreach (var item in query)
    {
      Console.WriteLine($"{item.ProductName}");
    }
    Console.ForegroundColor = ConsoleColor.White;
  }
  else if (choice == "6") 
  {
    // Display all active products in the DB table
    var db = new DataContext();
    var query = db.Products.Where(p => p.Discontinued == false).OrderBy(p => p.ProductName);
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"{query.Count()} records returned");
    Console.ForegroundColor = ConsoleColor.DarkCyan;
    foreach (var item in query)
    {
      Console.WriteLine($"{item.ProductName}");
    }
    Console.ForegroundColor = ConsoleColor.White;
  }
  else if (choice == "7")
  {
    // Display all the discontinued products in the DB table in Red
    var db = new DataContext();
    var query = db.Products.Where(p => p.Discontinued == true).OrderBy(p => p.ProductName);
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"{query.Count()} records returned");
    Console.ForegroundColor = ConsoleColor.Red;
    foreach (var item in query)
    {
      Console.WriteLine($"{item.ProductName}");
    }
    Console.ForegroundColor = ConsoleColor.White;
  }
  else if (choice == "8")
  {
    // Display a specific product
    var db = new DataContext();
    var query = db.Products.OrderBy(p => p.ProductName);
    Console.WriteLine("Select the product you want to display:");
    Console.ForegroundColor = ConsoleColor.DarkRed;
    foreach (var item in query)
    {
      Console.WriteLine($"{item.ProductId}) {item.ProductName}");
    }
    Console.ForegroundColor = ConsoleColor.White;
    int id = int.Parse(Console.ReadLine()!);
    Console.Clear();
    logger.Info($"ProductId {id} selected");
    Product product = db.Products.FirstOrDefault(p => p.ProductId == id)!;
    Console.WriteLine($"{product.ProductName} - Unit Prince: {product.UnitPrice}\nQuantity Per Unit: {product.QuantityPerUnit}\n Units in Stock: {product.UnitsInStock}\nUnits on Order: {product.UnitsOnOrder}\nReorder Level: {product.ReorderLevel}");
  }
  else if (choice == "9")
  {
    // Edit a product
    var db = new DataContext();
    var query = db.Products.OrderBy(p => p.ProductName);
    Console.WriteLine("Select the product you want to edit:");
    Console.ForegroundColor = ConsoleColor.DarkRed;
    foreach (var item in query)
    {
      Console.WriteLine($"{item.ProductId}) {item.ProductName}");
    }
    Console.ForegroundColor = ConsoleColor.White;
    int id = int.Parse(Console.ReadLine()!);
    Console.Clear();
    logger.Info($"ProductId {id} selected");
    Product product = db.Products.FirstOrDefault(p => p.ProductId == id)!;
    Console.WriteLine("Enter Product Name:");
    product.ProductName = Console.ReadLine()!;
    Console.WriteLine("Enter the Quantity Per Unit:");
    product.QuantityPerUnit = Console.ReadLine();
    Console.WriteLine("Enter the Unit Price:");
    product.UnitPrice = decimal.Parse(Console.ReadLine()!);
    Console.WriteLine("Enter the Units in Stock:");
    product.UnitsInStock = short.Parse(Console.ReadLine()!);
    Console.WriteLine("Enter the Units on Order:");
    product.UnitsOnOrder = short.Parse(Console.ReadLine()!);
    Console.WriteLine("Enter the Reorder Level:");
    product.ReorderLevel = short.Parse(Console.ReadLine()!);
    Console.WriteLine("Is the product discontinued? (y/n)");
    string? input = Console.ReadLine();
    if (input == "y")
    {
      product.Discontinued = true;
    }
    else
    {
      product.Discontinued = false;
    }
    db.SaveChanges();
    logger.Info($"Product: {product.ProductName} updated successfully");
  }
  else if (choice == "10")
  {
    // Add a product
    Product product = new();
    Console.WriteLine("Enter Product Name:");
    product.ProductName = Console.ReadLine()!;
    Console.WriteLine("Enter the Quantity Per Unit:");
    product.QuantityPerUnit = Console.ReadLine();
    Console.WriteLine("Enter the Unit Price:");
    product.UnitPrice = decimal.Parse(Console.ReadLine()!);
    Console.WriteLine("Enter the Units in Stock:");
    product.UnitsInStock = short.Parse(Console.ReadLine()!);
    Console.WriteLine("Enter the Units on Order:");
    product.UnitsOnOrder = short.Parse(Console.ReadLine()!);
    Console.WriteLine("Enter the Reorder Level:");
    product.ReorderLevel = short.Parse(Console.ReadLine()!);
    Console.WriteLine("Is the product discontinued? (y/n)");
    string? input = Console.ReadLine();
    if (input == "y")
    {
      product.Discontinued = true;
    }
    else
    {
      product.Discontinued = false;
    }
    var db = new DataContext();
    db.Products.Add(product);
    db.SaveChanges();
    logger.Info($"Product: {product.ProductName} added successfully");
  }
  else if (String.IsNullOrEmpty(choice))
  {
    break;
  }
  Console.WriteLine();
} while (true);

logger.Info("Program ended");