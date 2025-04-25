using Microsoft.ApplicationInsights.Channel;
using Serilog.Events;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;
using System.Reflection;

namespace PreciousMetalsTradingSystem.WebApi.Infrastructure
{
    public class TelemetryConverter : TraceTelemetryConverter
    {
        //TODO: is there anything else we would like to pass into Application Insights?
        public override IEnumerable<ITelemetry> Convert(LogEvent logEvent, IFormatProvider formatProvider)
        {
            // first create a default TraceTelemetry using the sink's default logic
            // .. but without the log level, and (rendered) message (template) included in the Properties
            foreach (ITelemetry telemetry in base.Convert(logEvent, formatProvider))
            {
                telemetry.Context.Cloud.RoleName = Assembly.GetExecutingAssembly().GetName().Name;

                /*
                // then go ahead and post-process the telemetry's context to contain the user id as desired
                if (logEvent.Properties.ContainsKey("UserId"))
                {
                    telemetry.Context.User.Id = logEvent.Properties["UserId"].ToString();
                }
                // post-process the telemetry's context to contain the operation id
                if (logEvent.Properties.ContainsKey("operation_Id"))
                {
                    telemetry.Context.Operation.Id = logEvent.Properties["operation_Id"].ToString();
                }
                // post-process the telemetry's context to contain the operation parent id
                if (logEvent.Properties.ContainsKey("operation_parentId"))
                {
                    telemetry.Context.Operation.ParentId = logEvent.Properties["operation_parentId"].ToString();
                }
                // typecast to ISupportProperties so you can manipulate the properties as desired
                ISupportProperties propTelematry = (ISupportProperties)telemetry;

                // find redundent properties
                var removeProps = new[] { "UserId", "operation_parentId", "operation_Id" };
                removeProps = removeProps.Where(prop => propTelematry.Properties.ContainsKey(prop)).ToArray();

                foreach (var prop in removeProps)
                {
                    // remove redundent properties
                    propTelematry.Properties.Remove(prop);
                }
                */

                yield return telemetry;
            }
        }
    }
}
