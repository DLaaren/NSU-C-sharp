using EveryoneToTheHackathon.Entities;

namespace EveryoneToTheHackathon.Tests;

public class HackathonTests
{
    [Fact]
    public void CheckHackathonResultWithDefinedData()
    {
        // Arrange
        var teamLeads = new List<Employee>
        {
            new Employee(1, EmployeeTitle.TeamLead, "John Doe"),
            new Employee(2, EmployeeTitle.TeamLead, "Jane Black"),
            new Employee(3, EmployeeTitle.TeamLead, "Bob Richman"),
            new Employee(4, EmployeeTitle.TeamLead, "Aboba Abobovich"),
            new Employee(5, EmployeeTitle.TeamLead, "Chuck Norris")
        };
        var juniors = new List<Employee>
        {
            new Employee(1, EmployeeTitle.Junior, "Walter White"),
            new Employee(2, EmployeeTitle.Junior, "Arnold Kindman"),
            new Employee(3, EmployeeTitle.Junior, "Jack Jones"),
            new Employee(4, EmployeeTitle.Junior, "Jane Jordan"),
            new Employee(5, EmployeeTitle.Junior, "Ken Kennedy")
        };

        var teamLeadsWishlists = new List<Wishlist>(5);
        var juniorsWishlists = new List<Wishlist>(5);
        for (var i = 1; i <= teamLeads.Count; i++)
        {
            Random seed = new Random(i);
            teamLeadsWishlists.Add(new Wishlist(i, EmployeeTitle.TeamLead, Enumerable.Range(1, 5).OrderBy(_ => seed.Next()).ToArray()));
        }
        for (var i = 1; i <= juniors.Count; i++)
        {
            Random seed = new Random(i * 100);
            juniorsWishlists.Add(new Wishlist(i, EmployeeTitle.Junior, Enumerable.Range(1, 5).OrderBy(_ => seed.Next()).ToArray()));
        }

        Hackathon hackathon = new Hackathon(
            teamLeads, 
            juniors,
            new HRManager(new ProposeAndRejectAlgorithm()),
            new HRDirector()
            );
        
        // Act = perform test
        hackathon.HoldEvent(teamLeadsWishlists, juniorsWishlists);

        // Assert = validate test's results
        Assert.Equal(4.2, hackathon.MeanSatisfactionIndex);
    }
}