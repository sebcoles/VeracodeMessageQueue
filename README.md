# VeracodeMessageQueue

This is another experiment with notifications from Veracode. This project is a console application that will compare the latest Veracode apps, scans and flaws against a local storage copy (MSSQL). If there are any changes it will publish an event to a Azure Event Grid topic, and then update it's local storage. This can be done on a schedule to get events posted to Azure.

Using Event Grids in Azure opens up building simple no/low workflows using LogicApps or more complex event based solutions by subscribing to the event grid topic.

The events that are published are:
- Apps
  - New Applications added to Veracode
  - Removed Applications from Veracode
- Scans
  - New/Removed scans from Veracode
  - Status changes to a scan in Veracode
- Flaws
  - New/Removed Flaws
  - Flaw Remediation Status Change
  - Flaw Mitigation Status Change

# Setup
1. Create an Azure Event Grid Topic
2. Have a MSSQL instance available to host the storage
3. Update the console configuration with the address of the topic and the shared access key
4. Console application reads from .Veracode credentials file, update the location in the config

The console application can be run in 2 ways.
- To scan all all available apps, run the application with the `-a` flag
- To only look at a subset of applications, include the App Ids in the console configuration

`
  "VeracodeFileLocation": "_YOUR_CREDENTIALS_FILE_",
  "Apps": [], // OPTIONAL IF ONLY LOOKING AT SUBSET OF APPS
  "ServiceBusConfiguration": {
    "SharedAccessKey": "_YOUR_EVENT_GRID_SHARED_aCCESS_KEY_",
    "VeracodeTopicEndpoint": "_YOUR_EVENT_GRID_TOPIC_ENDPOINT_"
  },
  "ConnectionStrings": {
    "DefaultConnection": "_MSSQL_CONNECTION_STRING_"
  }
`

This will look to supersede the Mitgations Webhook project.