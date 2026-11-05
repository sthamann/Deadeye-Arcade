# Verification scope

The library parent previously stopped a worker after 165 seconds, although that worker permits 180 seconds of profile preparation before its renderer-start budget begins. A valid slow preparation could therefore be cancelled before its own limit. This is a deterministic budget mismatch; it has not been attributed to any particular native failure without its report.

The parent now derives its bound from the shared 180-second preparation limit, 90-second renderer budget, requested observation and a finite 60-second cleanup allowance. Individual worker limits and the emergency cancellation remain active. The existing observation checks verify the bounded sum and valid interval range.

The local suite passed 859 meaningful core checks. Windows CI and installed native observations are recorded after execution. The full-library run already in progress uses installed 0.3.78 and remains labelled accordingly.
