using Microsoft.EntityFrameworkCore;

namespace E_Comm.Models
{
    public static class DataSeeder
    {
        public static async Task SeedDataAsync(EntertainmentGuildContext context)
        {
            await context.Database.EnsureCreatedAsync();

            // Check if data already exists
            if (await context.Genres.AnyAsync())
                return; // Data already seeded

            // Seed Genres
            var genres = new List<Genre>
            {
                new Genre { Name = "Books" },
                new Genre { Name = "Movies" },
                new Genre { Name = "Games" },
                new Genre { Name = "Fiction" },
                new Genre { Name = "Non-Fiction" },
                new Genre { Name = "Sci-Fi" },
                new Genre { Name = "Fantasy" },
                new Genre { Name = "Action" },
                new Genre { Name = "Adventure" },
                new Genre { Name = "RPG" }
            };

            context.Genres.AddRange(genres);
            await context.SaveChangesAsync();

            // Seed Sources
            var sources = new List<Source>
            {
                new Source { Source_name = "Amazon Books", ExternalLink = "https://amazon.com", GenreID = 1 },
                new Source { Source_name = "Netflix", ExternalLink = "https://netflix.com", GenreID = 2 },
                new Source { Source_name = "Steam Games", ExternalLink = "https://steam.com", GenreID = 3 },
                new Source { Source_name = "Barnes & Noble", ExternalLink = "https://barnesandnoble.com", GenreID = 1 },
                new Source { Source_name = "GameStop", ExternalLink = "https://gamestop.com", GenreID = 3 }
            };

            context.Sources.AddRange(sources);
            await context.SaveChangesAsync();

            // Seed Products
            var products = new List<Product>
            {
                // Books
                new Product
                {
                    Name = "The Lord of the Rings",
                    Author = "J.R.R. Tolkien",
                    Description = "An epic high fantasy novel that follows the quest to destroy the One Ring and save Middle-earth from the Dark Lord Sauron.",
                    GenreID = 7, // Fantasy
                    Published = new DateTime(1954, 7, 29),
                    LastUpdatedBy = "System",
                    LastUpdated = DateTime.Now
                },
                new Product
                {
                    Name = "Dune",
                    Author = "Frank Herbert",
                    Description = "Set in the distant future amidst a feudal interstellar society, this novel tells the story of young Paul Atreides.",
                    GenreID = 6, // Sci-Fi
                    Published = new DateTime(1965, 8, 1),
                    LastUpdatedBy = "System",
                    LastUpdated = DateTime.Now
                },
                new Product
                {
                    Name = "1984",
                    Author = "George Orwell",
                    Description = "A dystopian social science fiction novel about totalitarian control and the struggle for individual freedom.",
                    GenreID = 4, // Fiction
                    Published = new DateTime(1949, 6, 8),
                    LastUpdatedBy = "System",
                    LastUpdated = DateTime.Now
                },
                new Product
                {
                    Name = "The Hobbit",
                    Author = "J.R.R. Tolkien",
                    Description = "The enchanting prelude to The Lord of the Rings, following Bilbo Baggins on his unexpected journey.",
                    GenreID = 7, // Fantasy
                    Published = new DateTime(1937, 9, 21),
                    LastUpdatedBy = "System",
                    LastUpdated = DateTime.Now
                },

                // Movies
                new Product
                {
                    Name = "Avatar: The Way of Water",
                    Author = "James Cameron",
                    Description = "Jake Sully and Ney'tiri have formed a family and are doing everything to stay together, but they must leave their home to explore the regions of Pandora.",
                    GenreID = 6, // Sci-Fi
                    Published = new DateTime(2022, 12, 16),
                    LastUpdatedBy = "System",
                    LastUpdated = DateTime.Now
                },
                new Product
                {
                    Name = "Top Gun: Maverick",
                    Author = "Joseph Kosinski",
                    Description = "After thirty years, Maverick is still pushing the envelope as a top naval aviator, training a new generation for a specialized mission.",
                    GenreID = 8, // Action
                    Published = new DateTime(2022, 5, 27),
                    LastUpdatedBy = "System",
                    LastUpdated = DateTime.Now
                },
                new Product
                {
                    Name = "The Batman",
                    Author = "Matt Reeves",
                    Description = "Batman ventures into Gotham City's underworld when a sadistic killer leaves behind a trail of cryptic clues.",
                    GenreID = 8, // Action
                    Published = new DateTime(2022, 3, 4),
                    LastUpdatedBy = "System",
                    LastUpdated = DateTime.Now
                },
                new Product
                {
                    Name = "Spider-Man: No Way Home",
                    Author = "Jon Watts",
                    Description = "Peter Parker seeks help from Doctor Strange to make everyone forget he is Spider-Man, but the spell goes wrong.",
                    GenreID = 8, // Action
                    Published = new DateTime(2021, 12, 17),
                    LastUpdatedBy = "System",
                    LastUpdated = DateTime.Now
                },

                // Games
                new Product
                {
                    Name = "Elden Ring",
                    Author = "FromSoftware",
                    Description = "A fantasy action-RPG adventure set within a world full of mystery and peril, ready to be fully explored and discovered.",
                    GenreID = 10, // RPG
                    Published = new DateTime(2022, 2, 25),
                    LastUpdatedBy = "System",
                    LastUpdated = DateTime.Now
                },
                new Product
                {
                    Name = "God of War Ragnarök",
                    Author = "Santa Monica Studio",
                    Description = "Kratos and Atreus embark on a mythic journey for answers before Ragnarök arrives and the realms are destroyed.",
                    GenreID = 9, // Adventure
                    Published = new DateTime(2022, 11, 9),
                    LastUpdatedBy = "System",
                    LastUpdated = DateTime.Now
                },
                new Product
                {
                    Name = "Cyberpunk 2077",
                    Author = "CD Projekt RED",
                    Description = "An open-world, action-adventure story set in Night City, a megalopolis obsessed with power, glamour and body modification.",
                    GenreID = 10, // RPG
                    Published = new DateTime(2020, 12, 10),
                    LastUpdatedBy = "System",
                    LastUpdated = DateTime.Now
                },
                new Product
                {
                    Name = "The Witcher 3: Wild Hunt",
                    Author = "CD Projekt RED",
                    Description = "A story-driven open world RPG set in a visually stunning fantasy universe full of meaningful choices and impactful consequences.",
                    GenreID = 10, // RPG
                    Published = new DateTime(2015, 5, 19),
                    LastUpdatedBy = "System",
                    LastUpdated = DateTime.Now
                }
            };

            context.Products.AddRange(products);
            await context.SaveChangesAsync();

            // Seed Stocktakes (Inventory)
            var random = new Random();
            var stocktakes = new List<Stocktake>();

            for (int i = 0; i < products.Count; i++)
            {
                var product = products[i];
                var sourceId = random.Next(1, 6); // Random source
                var price = product.GenreID switch
                {
                    1 or 4 or 5 or 6 or 7 => random.Next(15, 35), // Books: $15-35
                    2 or 8 => random.Next(20, 45), // Movies: $20-45
                    3 or 9 or 10 => random.Next(30, 80), // Games: $30-80
                    _ => random.Next(20, 50)
                };

                stocktakes.Add(new Stocktake
                {
                    ProductId = product.ID,
                    SourceId = sourceId,
                    Quantity = random.Next(5, 100),
                    Price = price + (random.NextDouble() * 10) // Add random cents
                });
            }

            context.Stocktakes.AddRange(stocktakes);
            await context.SaveChangesAsync();

            // Seed Sample Customer
            var customer = new Customer
            {
                Email = "john.doe@example.com",
                PhoneNumber = "+1-555-0123",
                StreetAddress = "123 Main Street",
                PostCode = 12345,
                Suburb = "Downtown",
                State = "NSW",
                CardNumber = "****-****-****-1234",
                CardOwner = "John Doe",
                Expiry = "12/26",
                CVV = 123
            };

            context.Customers.Add(customer);
            await context.SaveChangesAsync();
        }
    }
} 