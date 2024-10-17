using EmailPOC.Events;
using MediatR;
using Umbraco.Cms.Infrastructure.BackgroundJobs;

namespace EmailPOC.BackgroundJobs
{
    public class DailyNewsletterJob : IRecurringBackgroundJob
    {
        private readonly IMediator _mediator;

        public event EventHandler PeriodChanged;

        public TimeSpan Period => TimeSpan.FromDays(1); // Run daily at 10 AM
        public DateTime ScheduledTime => new DateTime().AddHours(10);

        public DailyNewsletterJob(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task RunJobAsync()
        {
            await _mediator.Publish(new DailyEmailEvent()); // Trigger daily email event
        }
    }

}
