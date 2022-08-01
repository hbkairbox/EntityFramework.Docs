using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

#region DiagnosticObserver
public class DiagnosticObserver : IObserver<DiagnosticListener>
{
    public void OnCompleted()
        => throw new NotImplementedException();

    public void OnError(Exception error)
        => throw new NotImplementedException();

    public void OnNext(DiagnosticListener value)
    {
        if (value.Name == DbLoggerCategory.Name) // "Microsoft.EntityFrameworkCore"
        {
            value.Subscribe(new KeyValueObserver());
        }
    }
}
#endregion

#region KeyValueObserver
public class KeyValueObserver : IObserver<KeyValuePair<string, object>>
{
    public void OnCompleted()
        => throw new NotImplementedException();

    public void OnError(Exception error)
        => throw new NotImplementedException();

    public void OnNext(KeyValuePair<string, object> value)
    {
        //Helps to track the properties that are changed
        if(value.Key == CoreEventId.PropertyChangeDetected.ToString())
        {
            Console.WriteLine($"{value.Value}");  
        }

        //Helps to track the state of each entity
        if(value.Key == CoreEventId.StateChanged.ToString())
        {
            Console.WriteLine($"{value.Value}");
        }
    }
}
#endregion

public class Program
{
    #region Program
    public static void Main()
    {
        #region RegisterDiagnosticListener
        DiagnosticListener.AllListeners.Subscribe(new DiagnosticObserver());
        #endregion

        //Testing new added entities
        using (var context = new BlogsContext())
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.Add(
                        new Blog { Name = "EF Blog", Posts = { new Post { Title = "EF Core 3.1!" }, new Post { Title = "EF Core 5.0!" } } });

            context.Add(
                        new Blog { Name = "EF6 Blog", Posts = { new Post { Title = "EF Core 6.1!" }, new Post { Title = "EF Core 6.0!" } } });

            context.SaveChanges();
        }

        //Testing existing updated Entities
        using (var context = new BlogsContext())
        {
            var blog = context.Blogs.Include(e => e.Posts).First();

            blog.Name = "EF Core Blog";
            context.Remove(blog.Posts.First());
            blog.Posts.Add(new Post { Title = "EF Core 6.0!" });

            var blog2 = context.Blogs.Skip(1).Take(1).First();
            blog2.Name = "EF6 Core Blog";


            context.SaveChanges();
        }
        #endregion
    }
}

public class BlogsContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("DataSource=blogs.db");
        
        //Enable this below to see and track entity data
        optionsBuilder.EnableSensitiveDataLogging();
        //optionsBuilder.LogTo(Console.WriteLine);
    }

    public DbSet<Blog> Blogs { get; set; }
}

public class Blog
{
    public int Id { get; set; }
    public string Name { get; set; }

    public ICollection<Post> Posts { get; } = new List<Post>();
}

public class Post
{
    public int Id { get; set; }
    public string Title { get; set; }

    public Blog Blog { get; set; }
}