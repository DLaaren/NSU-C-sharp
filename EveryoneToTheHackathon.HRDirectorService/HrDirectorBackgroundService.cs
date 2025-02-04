using System.Diagnostics;
using EveryoneToTheHackathon.Messages;
using MassTransit;

namespace EveryoneToTheHackathon.HRDirectorService;

public class HrDirectorBackgroundService(
    IBusControl busControl,
    ILogger<HrDirectorBackgroundService> logger,
    HrDirectorService hrDirectorService)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        for (var i = 0; i < hrDirectorService.HackathonsNumber; i++)
        {
            await StartHackathon(stoppingToken);
            
            Debug.Assert(hrDirectorService.HackathonFinished != null);
            await hrDirectorService.HackathonFinished.Task;
        }
        await Task.CompletedTask;
    }

    private async Task StartHackathon(CancellationToken stoppingToken)
    {
        hrDirectorService.CurrHackathonId = hrDirectorService.StartHackathon();
        logger.LogInformation("Starting hackathon with id = {id}", hrDirectorService.CurrHackathonId);
        
        hrDirectorService.HackathonFinished = new TaskCompletionSource<bool>();
        
        await busControl.Publish(new HackathonStarted(hrDirectorService.CurrHackathonId), stoppingToken);
        logger.LogInformation("HRDirector has announced start of the hackathon");
    }
}