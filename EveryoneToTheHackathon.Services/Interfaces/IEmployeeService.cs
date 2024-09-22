using EveryoneToTheHackathon.Entities;

namespace EveryoneToTheHackathon.Services;

public interface IEmployeeService
{
    public Employee? GetEmployeeById(int employeeId);
    public IEnumerable<Employee> GetEmployees();
    public void AddEmployee(Employee employee);
    public void AddEmployees(IEnumerable<Employee> employees);
    public void UpdateEmployee(Employee employee);
    public void UpdateEmployees(IEnumerable<Employee> employees);
}