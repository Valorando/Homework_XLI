using Microsoft.EntityFrameworkCore;

internal class Program
{

    class Company
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public List<User> Users { get; set; } = new();
    }

    class User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Age { get; set; }
        public int CompanyId { get; set; }
        public Company? Company { get; set; }
    }


    class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null;
        public DbSet<Company> Companies { get; set; } = null;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=helloap.db;Trusted_Connection=True;");
        }
    }

    private static void Main(string[] args)
    {
        // Пересоздание базы данных и заполнение её таблиц записями.
        using (ApplicationContext db = new ApplicationContext())
        {
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            Company microsoft = new Company { Name = "Microsoft" };
            Company google = new Company { Name = "Google" };
            db.Companies.AddRange(microsoft, google);

            User tan = new User { Name = "Tan", Age = 36, Company = microsoft };
            User vasya = new User { Name = "Vasya", Age = 39, Company = google };
            User alice = new User { Name = "Alice", Age = 28, Company = microsoft };
            User kate = new User { Name = "Kate", Age = 25, Company = google };

            db.Users.AddRange(tan, vasya, alice, kate);
            db.SaveChanges();
        }


        // Получение данных из таблицы.
        using (ApplicationContext db = new ApplicationContext())
        {
            var users = (from user in db.Users.Include(p => p.Company)
                         where user.CompanyId == 1
                         select user).ToList();
            foreach (var user in users)
            {
                Console.WriteLine($"{user.Name} ({user.Age}) - {user.Company?.Name}");
            }
        }


        // Получение данных с помощью LINQ.
        using (ApplicationContext db = new ApplicationContext())
        {
            var users = db.Users.Include(p => p.Company).Where(p => p.CompanyId == 1);
        }


        // Фильтрация данных с помощью метода Where.
        using (ApplicationContext db = new ApplicationContext())
        {
            var users = db.Users.Where(p => p.Company!.Name == "Google");
            foreach (User user in users)
            {
                Console.WriteLine($"{user.Name} ({user.Age})");
            }
        }


        // Фильтрация данных с помощью LINQ.
        using (ApplicationContext db = new ApplicationContext())
        {
            var users = (from user in db.Users
                         where user.Company!.Name == "Google"
                         select user).ToList();

            foreach (User user in users)
            {
                Console.WriteLine($"{user.Name} ({user.Age})");
            }
        }


        // Установка условия запроса с помощью EF.Functions.Like.
        using (ApplicationContext db = new ApplicationContext())
        {
            var users = db.Users.Where(p => EF.Functions.Like(p.Name!, "%Tom%"));
            foreach (User user in users)
            {
                Console.WriteLine($"{user.Name} ({user.Age})");
            }
        }


        // Объединение таблиц с помощью Join.
        using (ApplicationContext db = new ApplicationContext())
        {
            var users = db.Users.Join(db.Companies,
                u => u.CompanyId,
                c => c.Id,
                (u, c) => new
                {
                    u.Name,
                    Company = c.Name,
                    u.Age
                });

            foreach (var u in users)
            {
                Console.WriteLine($"{u.Name} ({u.Company}) - {u.Age}");
            }
        }
    }
}
