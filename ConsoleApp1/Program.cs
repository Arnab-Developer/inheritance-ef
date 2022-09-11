namespace ConsoleApp1
{
    using ConsoleApp1.Data;
    using ConsoleApp1.Services;
    using Microsoft.EntityFrameworkCore;

    public class Prog
    {
        private static void Main()
        {
            EmpContext context = new(new DbContextOptionsBuilder<EmpContext>()
                .UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=empdb;Integrated Security=True")
                .Options);

            IEmpService service = new EmpService(
                new EmpRepo(context),
                new SeniorEmpRepo(context));

            service.CreateEmp("emp1");
            service.CreateSeniorEmp("emp1", "mumbai");
        }
    }
}

namespace ConsoleApp1.Services
{
    using ConsoleApp1.Data;
    using ConsoleApp1.Models;

    public interface IEmpService
    {
        public void CreateEmp(string name);

        public void CreateSeniorEmp(string name, string flatAddress);
    }

    public class EmpService : IEmpService
    {
        private readonly IEmpRepo _repo;
        private readonly ISeniorEmpRepo _seniorEmpRepo;

        public EmpService(IEmpRepo empRepo, ISeniorEmpRepo seniorEmpRepo)
        {
            _repo = empRepo;
            _seniorEmpRepo = seniorEmpRepo;
        }

        void IEmpService.CreateEmp(string name)
        {
            Employee emp = new(name);
            PopulateBaseSal(emp);

            _repo.AddEmp(emp);
            //_repo.Save();
        }

        void IEmpService.CreateSeniorEmp(string name, string flatAddress)
        {
            SeniorEmployee emp = new(name, flatAddress);
            PopulateBaseSal(emp);

            _seniorEmpRepo.AddEmp(emp);
            _seniorEmpRepo.Save();
        }

        private static void PopulateBaseSal(Employee emp)
        {
            emp.CalculateBaseSal();
        }
    }
}

namespace ConsoleApp1.Models
{
    public class Employee
    {
        public int Id { get; private set; }

        public string Name { get; set; }

        public decimal BaseSal { get; protected set; }

        public Employee(string name)
        {
            Name = name;
        }

        public virtual void CalculateBaseSal()
        {
            BaseSal = 100;
        }
    }

    public class SeniorEmployee : Employee
    {
        public string FlatAddress { get; set; }

        public SeniorEmployee(string name, string flatAddress)
            : base(name)
        {
            FlatAddress = flatAddress;
        }

        public override void CalculateBaseSal()
        {
            BaseSal = 500;
        }
    }
}

namespace ConsoleApp1.Data
{
    using ConsoleApp1.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;

    public class EmpContext : DbContext
    {
        public DbSet<Employee> Employees { get; set; }

        public DbSet<SeniorEmployee> SeniorEmployees { get; set; }

        public EmpContext(DbContextOptions<EmpContext> options)
            : base(options)
        {
            Employees = Set<Employee>();
            SeniorEmployees = Set<SeniorEmployee>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Employee>()
            //    .Property("Discriminator")
            //    .HasMaxLength(200);

            modelBuilder.Entity<Employee>()
                .HasDiscriminator<string>("emp_type")
                .HasValue<Employee>("emp_normal")
                .HasValue<SeniorEmployee>("emp_senior");
        }
    }

    public class EmpContextFactory : IDesignTimeDbContextFactory<EmpContext>
    {
        public EmpContext CreateDbContext(string[] args)
        {
            DbContextOptionsBuilder<EmpContext> optionsBuilder = new();
            optionsBuilder.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=empdb;Integrated Security=True");

            return new EmpContext(optionsBuilder.Options);
        }
    }

    public interface IEmpRepo
    {
        public void AddEmp(Employee emp);

        public void Save();
    }

    public class EmpRepo : IEmpRepo
    {
        private readonly EmpContext _context;

        public EmpRepo(EmpContext context)
        {
            _context = context;
        }

        void IEmpRepo.AddEmp(Employee emp)
        {
            _context.Employees.Add(emp);
        }

        void IEmpRepo.Save()
        {
            _context.SaveChanges();
        }
    }

    public interface ISeniorEmpRepo
    {
        public void AddEmp(SeniorEmployee emp);

        public void Save();
    }

    public class SeniorEmpRepo : ISeniorEmpRepo
    {
        private readonly EmpContext _context;

        public SeniorEmpRepo(EmpContext context)
        {
            _context = context;
        }

        void ISeniorEmpRepo.AddEmp(SeniorEmployee emp)
        {
            _context.SeniorEmployees.Add(emp);
        }

        void ISeniorEmpRepo.Save()
        {
            _context.SaveChanges();
        }
    }
}