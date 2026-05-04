# CronWJb

Minimal cron-based scheduler using WJb.

## Overview
CronWJb loads actions from *actions.json*, registers them with WJb, and executes them according to cron expressions.

## Features
- Cron-based scheduled job execution (advanced cron available in Commercial edition)
- Simple action model (`IAction`).
- JSON-based configuration.
- UTF-8 console output.

## Run
```
dotnet run
```

## actions.json
Contains a dictionary of actions with type and cron metadata.

Example:
```json
{
  "HelloEveryMinute": {
    "Type": "DummyAction, CronWJb",
    "More": {
      "cron": "*/1 * * * *",
      "priority": "ASAP",
      "message": "Minute tick ✅"
    }
  },
  "Hello9to5Weekdays": {
    "Type": "DummyAction, CronWJb",
    "More": {
      "cron": "*/3 9-21 * * 1-5",
      "priority": "High",
      "message": "Working hours ping (every minute, Mon–Fri)"
    }
  }
}
```

## Program.cs
- Reads `actions.json`.
- Registers WJb actions via `AddWJbActions`.
- Enables scheduler via `AddWJbBase(jobScheduler: true)`.
- Runs host.

## DummyAction
A simple action that prints timestamp + message.
