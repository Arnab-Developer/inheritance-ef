namespace ConsoleApp1
{
    using ConsoleApp1.Data;
    using ConsoleApp1.Services;
    using Microsoft.EntityFrameworkCore;

    public class Prog
    {
        private static void Main()
        {
            IEmpService service = new EmpService(
                new EmpRepo(
                    new EmpContext(
                        new DbContextOptionsBuilder<EmpContext>()
                            .UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=empdb;Integrated Security=True")
                            .Options)));

            //service.CreateEmp("emp1");
            service.CreateSeniorEmp("emp1");
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

        public void CreateSeniorEmp(string name);
    }

    public class EmpService : IEmpService
    {
        private readonly IEmpRepo _repo;

        public EmpService(IEmpRepo repo)
        {
            _repo = repo;
        }

        void IEmpService.CreateEmp(string name)
        {
            Employee emp = new(name);
            PopulateBaseSal(emp);
            SaveEmp(emp);
        }

        void IEmpService.CreateSeniorEmp(string name)
        {
            SeniorEmployee emp = new(name);
            PopulateBaseSal(emp);
            SaveEmp(emp);
        }

        private static void PopulateBaseSal(Employee emp)
        {
            emp.CalculateBaseSal();
        }

        private void SaveEmp(Employee emp)
        {
            _repo.AddEmp(emp);
            _repo.Save();
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
        public SeniorEmployee(string name)
            : base(name)
        {
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

        public EmpContext(DbContextOptions<EmpContext> options)
            : base(options)
        {
            Employees = Set<Employee>();
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
}