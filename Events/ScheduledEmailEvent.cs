using MediatR;

namespace EmailPOC.Events
{
    public class ScheduledEmailEvent: INotification
    {
        public string MemberType { get; }
        public DateTime ScheduledTime { get; }

        public ScheduledEmailEvent(string memberType, DateTime scheduledTime)
        {
            MemberType = memberType;
            ScheduledTime = scheduledTime;
        }
    }
}
