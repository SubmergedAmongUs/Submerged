using System;

namespace InnerNet
{
	public enum ReportOutcome
	{
		NotReportedUnknown,
		NotReportedNoAccount,
		NotReportedNotFound,
		NotReportedRateLimit,
		Reported
	}
}
