using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Persistence.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        
    }

    //Representacion de trabblas en el modelo 
    //(Cada uno es una tabla en la base de datos)
    public DbSet<User> Users { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<UserEmail> UserEmails { get; set; }
    public DbSet<UserPasswordReset> UserPasswordReset { get; set; }

    
    

//Convierte camel case a snake_case para los nombres de tablas y columnas en la base de datos
    protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    foreach (var entity in modelBuilder.Model.GetEntityTypes())
    {
        var tableName = entity.GetTableName();
        if (!string.IsNullOrEmpty(tableName))
        {
            entity.SetTableName(ToSnakeCase(tableName));
        }
        foreach (var property in entity.GetProperties())
        {
            var columnName = property.GetColumnName();
            if (!string.IsNullOrEmpty(columnName))
            {
                property.SetColumnName(ToSnakeCase(columnName));
            }  
        }
    }


//------------------------------------------------//
//Configuracion especifica de la entidad user
//------------------------------------------------//
modelBuilder.Entity<User>(entity =>
{
    //Llave primaria
    entity.HasKey(e => e.Id);

    entity.HasIndex(e => e.Email).IsUnique();
    entity.HasIndex(e => e.Username).IsUnique();

    //Relacion de 1:1 con UserProfile
    entity.HasOne(e => e.UserProfile)
        .WithOne(p => p.User)
        .HasForeignKey<UserProfile>(p => p.UserId)
        .OnDelete(DeleteBehavior.Cascade);

    //Relacion de 1:N con UserRoles
    entity.HasMany(e => e.UserRoles)
        .WithOne(ur => ur.User)
        .HasForeignKey(ur => ur.UserId)
        .OnDelete(DeleteBehavior.Cascade);

    //Relacion 1:1 con UserEmail
    entity.HasOne(e => e.UserEmail)
        .WithOne(ue => ue.User)
        .HasForeignKey<UserEmail>(ue => ue.UserId)
        .OnDelete(DeleteBehavior.Cascade);

    //Relacion 1:N con UserPasswordReset
    entity.HasOne(e => e.UserPasswordReset)
        .WithOne(upr => upr.User)
        .HasForeignKey<UserPasswordReset>(upr => upr.UserId)
        .OnDelete(DeleteBehavior.Cascade);      
});


//Cnfiguracion especifica de la entidad userRole
modelBuilder.Entity<UserRole>(entity =>
{
    //Llave primario
    entity.HasKey(e => e.Id);
    //El usuario no puede tener el mismo rol mas de una vez
    entity .HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();
});

//------------------------------------------------//
//Configuracion especifica de la entidad Role
//------------------------------------------------//
modelBuilder.Entity<Role>(entity =>
{
    //Llave primaria
    entity.HasKey(e => e.Id);
    //El nombre del rol debe ser unico
    entity.HasIndex(e => e.Name).IsUnique();
});

}
//-----------------------------------------------//


    //Funcion para configrar el nombre de clase a nombre de DB en formato snake_case
    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return string.Concat(
            input.Select((x, i) => i > 0 && char.IsUpper(x) 
                ? "_" + x.ToString().ToLower() 
                : x.ToString().ToLower())
        );
    }
}