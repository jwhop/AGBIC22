using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public enum Tag
{
    None,
    Unhealthy,
    Healthy,
    Expensive,
    Cheap,
    Soggy,
    Crunchy,
    Chewy,
    Mild,
    Sweet,
    Spicy,
    Salty,
    Sour
}

public enum Category
{
    Topping,
    Bun,
    Meat,
    Garnish
}

public enum Size
{
    Small,
    Medium,
    Large,
    Huge
}

public class Ingredient
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public HashSet<Tag> Tags { get; set; } = new HashSet<Tag>();
    public HashSet<Category> Categories { get; set; } = new HashSet<Category>();
}

public class Customer
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public Size Size { get; set; } = Size.Small;
    
    // TODO JSR/HE : Add image resource path as field
    public HashSet<Tag> Likes { get; set; } = new HashSet<Tag>();
    public HashSet<Tag> Dislikes { get; set; } = new HashSet<Tag>();
    
    // 0 = snack
    // 1 = meal
    // 2 = famished
    // 3 = whole hog
    public int Hungriness { get; set; } = 1;
    public override string ToString()
    {
        return $"{Name}[{Id}] - {Description}\n" +
               $"Size: {Size}\n" +
               $"Likes: {Likes.Aggregate("", (s, tag) => s + $"{tag}, ")}\n" +
               $"Dislikes: {Dislikes.Aggregate("", (s, tag) => s + $"{tag}, ")}\n" +
               $"Hungriness: {Hungriness}\n";
    }
}

public class Burger
{
    public List<Ingredient> Ingredients { get; set; } = new List<Ingredient>();

    public override string ToString()
    {
        var retString = "";
        foreach (var ingredient in Ingredients)
        {
            retString += $"{ingredient.Name}\n";
        }

        return retString;
    }
}

public class BurgerSystem : MonoBehaviour
{
    private static BurgerSystem _instance;
    public static BurgerSystem Instance
    {
        get { return _instance; }
    }

    private static Dictionary<string, Ingredient> _ingredients = new Dictionary<string, Ingredient>();
    private static Dictionary<Category, HashSet<Ingredient>> _categoryList = new Dictionary<Category, HashSet<Ingredient>>();
    private static HashSet<Customer> _customers = new HashSet<Customer>();
    
    private static Random rng = new Random();


    /// <summary>
    /// Generates a burger that suits the customer's tastes
    /// </summary>
    /// <param name="customer">Customer requesting the burger</param>
    /// <returns></returns>
    public Burger GenerateBurgerForCustomer(Customer customer)
    {
        // Firstly we need a number of buns. Snack might be light
        var numBuns = customer.Hungriness < 2 ? 1 : 2;
        
        // We must have at least 1 patty
        var numMeat = Math.Max(customer.Hungriness, 1);
        
        // Fair amount of toppings needed
        var numToppings = (int)customer.Hungriness * 1.5f;

        var numGarnish = 0;
        // People who don't like rich things don't want a garnish
        if (!customer.Dislikes.Contains(Tag.Expensive))
        {
            numGarnish = rng.Next(0, 1);
            
            // Rich guy always wants garnish, maybe 2
            if (customer.Likes.Contains(Tag.Expensive))
                numGarnish++;
        }

        Burger burger = new Burger();
        
        // Add the toppings (these should be heterogenous)
        for (int i = 0; i < numToppings; i++)
        {
            Ingredient topping = null;

            // Randomize the list of toppings
            var toppings = _categoryList[Category.Topping].OrderBy(a => rng.Next());
            foreach (var ingredient in toppings)
            {
                if (!ingredient.Tags.Overlaps(customer.Dislikes))
                {
                    topping = ingredient;
                }
            }

            // They're not too picky
            if (topping != null)
            {
                burger.Ingredients.Add(topping);
            }
        }
        
        // Add the meat
        for (int i = 0; i < numMeat; i++)
        {
            Ingredient meat = null;
            var meats = _categoryList[Category.Meat].OrderBy(a => rng.Next());
            foreach (var ingredient in meats)
            {
                if (!ingredient.Tags.Overlaps(customer.Dislikes))
                {
                    meat = ingredient;
                }
            }

            // They are not vegetarian
            if (meat != null)
            {
                burger.Ingredients.Add(meat);
            }
        }

        // The bun will be the same for the top and bottom
        Ingredient bun = null;
        foreach (var ingredient in _categoryList[Category.Bun])
        {
            if (!ingredient.Tags.Overlaps(customer.Dislikes))
            {
                bun = ingredient;
            }
        }

        if (bun != null)
        {
            burger.Ingredients.Add(bun);
            if (numBuns == 2)
            {
                burger.Ingredients.Insert(0, bun);
            }
        }
        
        // Finally, add the garnishes
        for (int i = 0; i < numGarnish; i++)
        {
            Ingredient garnish = null;
            var garnishes = _categoryList[Category.Garnish].OrderBy(a => rng.Next());
            foreach (var ingredient in garnishes)
            {
                if (!ingredient.Tags.Overlaps(customer.Dislikes))
                {
                    garnish = ingredient;
                }
            }

            // Ok guess they don't really like any of our garnishes, so they won't request it
            if (garnish != null)
            {
                burger.Ingredients.Insert(0, garnish);
            }
        }
        
        return burger;
    }

    // TODO JSR : Move this somewhere
    private void TestBurger()
    {
        // Print all customers
        foreach (var customer in _customers)
        {
            Debug.Log(customer);
            
            Burger burger = BurgerSystem.Instance.GenerateBurgerForCustomer(customer);
            if (burger != null)
            {
                Debug.Log(burger);
            }
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    private void Start()
    {
        LoadIngredientsFromCSV();
        LoadCustomersFromCSV();
        TestBurger();
    }

    private void LoadIngredientsFromCSV()
    {
        string path = Path.Combine(Application.dataPath,"ingredients.csv");
        using (var reader = new StreamReader(path))
        {
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                if (values.Length != 4)
                    continue;

                var id = values[0];
                var name = values[1];
                var tags = values[2];
                var categories = values[3];
                var ingredient = new Ingredient
                {
                    Id = id,
                    Name = name,
                    Tags = new HashSet<Tag>(tags.Split(';').Select(tagString => (Tag) Enum.Parse(typeof(Tag), tagString)).ToList()),
                    Categories = new HashSet<Category>(categories.Split(';').Select(categoryString => (Category) Enum.Parse(typeof(Category), categoryString)).ToList())
                };
                _ingredients[id] = ingredient;
                foreach (var category in ingredient.Categories)
                {
                    if (!_categoryList.ContainsKey(category))
                    {
                        _categoryList[category] = new HashSet<Ingredient>();
                    }

                    _categoryList[category].Add(ingredient);
                }
            }
        }
    }

    private void LoadCustomersFromCSV()
    {
        string path = Path.Combine(Application.dataPath,"characters.csv");
        using (var reader = new StreamReader(path))
        {
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                if (values.Length != 6)
                    continue;

                var id = values[0];
                var name = values[1];
                var description = values[2];
                var size = (Size) Enum.Parse(typeof(Size), values[3]);
                var likes = values[4];
                var dislikes = values[5];
                
                // Generate hungriness based on size
                var sizeNum = (int)size + 1;
                var hungriness = rng.Next(sizeNum / 2, sizeNum);
                var customer = new Customer
                {
                    Id = id,
                    Name = name,
                    Description = description,
                    Size = size,
                    Likes = new HashSet<Tag>(likes.Split(';').Select(tagString => (Tag) Enum.Parse(typeof(Tag), tagString)).ToList()),
                    Dislikes = new HashSet<Tag>(dislikes.Split(';').Select(tagString => (Tag) Enum.Parse(typeof(Tag), tagString)).ToList()),
                    Hungriness = hungriness
                };
                _customers.Add(customer);
            }
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
