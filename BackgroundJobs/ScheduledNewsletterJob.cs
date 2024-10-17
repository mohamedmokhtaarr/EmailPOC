using EmailPOC.Events;
using MediatR;
using Umbraco.Cms.Infrastructure.BackgroundJobs;

namespace EmailPOC.BackgroundJobs
{
    public class ScheduledNewsletterJob : IRecurringBackgroundJob
    {
        private readonly IMediator _mediator;

        public event EventHandler PeriodChanged;

        public DateTime ScheduledTime { get; private set; }
        public string MemberTypeFilter { get; private set; }

        public TimeSpan Period => throw new NotImplementedException();

        public ScheduledNewsletterJob(IMediator mediator, DateTime scheduledTime, string memberTypeFilter)
        {
            _mediator = mediator;
            ScheduledTime = scheduledTime;
            MemberTypeFilter = memberTypeFilter;
        }

        public async Task RunJobAsync()
        {
            await _mediator.Publish(new ScheduledEmailEvent(MemberTypeFilter, ScheduledTime)); // Trigger scheduled email event
        }
    }

}
