using CommandLine;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VeracodeMessageQueue.MessagingService;
using VeracodeMessageQueue.Models;
using VeracodeMessageQueue.Profiles;
using VeracodeMessageQueue.Storage;
using VeracodeService;
using AutoMapper;

namespace VeracodeMessageQueue
{
    class Program
    {
        private static IServiceProvider _serviceProvider;
        private static string[] _appIds;
        static void Main(string[] args)
        {
            IConfiguration Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
#if DEBUG
                .AddJsonFile($"appsettings.Development.json", false)
#else
                .AddJsonFile("appsettings.json", false)
#endif
                .Build();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient(options => Options.Create(
                VeracodeFileHelper.GetConfiguration(Configuration.GetValue<string>("VeracodeFileLocation"))));
            serviceCollection.Configure<EventGridConfiguration>(options => Configuration.GetSection("ServiceBusConfiguration").Bind(options));
            serviceCollection.AddScoped<IVeracodeRepository, VeracodeRepository>();
            serviceCollection.AddScoped<IMessageService, MessageService>();
            serviceCollection.AddScoped<IGenericRepository<App>, GenericRepository<App>>();
            serviceCollection.AddScoped<IGenericRepository<Build>, GenericRepository<Build>>();
            serviceCollection.AddScoped<IGenericRepository<Flaw>, GenericRepository<Flaw>>();
            serviceCollection.AddScoped<IMappingService, MappingService>();
            serviceCollection.AddAutoMapper(typeof(Program));

            _appIds = Configuration.GetValue<string[]>("Apps");
            _serviceProvider = serviceCollection.BuildServiceProvider();
            Parser.Default.ParseArguments<RunOptions>(args)
                .MapResult((
                    RunOptions options) => Run(options),
                    errs => HandleParseError(errs));
        }

        static int Run(RunOptions options)
        {
            var messageService = _serviceProvider.GetService<IMessageService>();
            var veracodeRepository = _serviceProvider.GetService<IVeracodeRepository>();
            var myAppRepo = _serviceProvider.GetService<IGenericRepository<App>>();
            var myBuildRepo = _serviceProvider.GetService<IGenericRepository<Build>>();
            var myFlawRepo = _serviceProvider.GetService<IGenericRepository<Flaw>>();


            if (options.All)
                _appIds = veracodeRepository.GetAllApps()
                    .Select(x => x.app_id.ToString()).ToArray();

            // check for app events
            AppEvents();

            foreach (var appId in _appIds)
            {
                // check for build events
                BuildEvents(appId);

            }



            // check for new events

            // check for mitigation events

            return 1;
        }

        private static void AppEvents()
        {
            var veracodeRepository = _serviceProvider.GetService<IVeracodeRepository>();
            var myAppRepo = _serviceProvider.GetService<IGenericRepository<App>>();
            var messageService = _serviceProvider.GetService<IMessageService>();

            var currentAppsInDb = myAppRepo
                .GetAll()
                .Select(x => x.Id)
                .ToArray();

            var removedAppsIds = currentAppsInDb.Except(_appIds);
            foreach (var appId in removedAppsIds)
            {
                var app = myAppRepo.GetAll().SingleOrDefault(x => x.Id == appId);
                messageService.SendMessage(MessageTypes.AppEvent, $"The application {app.Name} with ID {appId} was deleted from Veracode.");
                myAppRepo.Delete(app);
            }

            var addedAppsIds = _appIds.Except(currentAppsInDb);
            foreach (var appId in addedAppsIds)
            {
                var app = veracodeRepository.GetAppDetail(appId);
                messageService.SendMessage(MessageTypes.AppEvent, $"The application {app.application[0].app_name} with ID {appId} was created in Veracode.");
                myAppRepo.Create(new App { Id = appId, Name = app.application[0].app_name });
            }
        }

        private static string[] BuildEvents(string appId)
        {
            var veracodeRepository = _serviceProvider.GetService<IVeracodeRepository>();
            var myBuildRepo = _serviceProvider.GetService<IGenericRepository<Build>>();
            var messageService = _serviceProvider.GetService<IMessageService>();

            var currentBuildsInDb = myBuildRepo
                .GetAll()
                .Where(x => x.AppId == appId)
                .Select(x => x.Id)
                .ToArray();

            var buildIds = veracodeRepository
                .GetAllBuildsForApp(appId).Select(x => $"{x.build_id}")
                .ToArray();

            var removedBuildIds = currentBuildsInDb.Except(buildIds);
            foreach (var buildId in removedBuildIds)
            {
                var build = myBuildRepo.GetAll().SingleOrDefault(x => x.Id == buildId);
                messageService.SendMessage(MessageTypes.BuildEvent, $"The build {build.Name} with ID {build.Id} was deleted from Veracode.");
                myBuildRepo.Delete(build);
            }

            var addedBuildIds = buildIds.Except(currentBuildsInDb);
            foreach (var buildId in addedBuildIds)
            {
                var build = veracodeRepository.GetBuildDetail(appId, buildId);
                messageService.SendMessage(MessageTypes.BuildEvent, $"The build {build.build.version} with ID {build.build_id} was created from Veracode.");
                myBuildRepo.Create(new Build { Id = $"{build.build_id}", AppId = appId, Name = build.build.version, Status = VeracodeEnumConverter.Convert(build.build.analysis_unit[0].status) });
            }

            var buildsToUpdate = buildIds.Intersect(currentBuildsInDb);
            foreach (var buildId in buildsToUpdate)
            {
                var myBuild = myBuildRepo.GetAll().SingleOrDefault(x => x.Id == buildId);
                var build = veracodeRepository.GetBuildDetail(appId, buildId);
                if(VeracodeEnumConverter.Convert(build.build.analysis_unit[0].status) != myBuild.Status)
                {
                    myBuild.Status = VeracodeEnumConverter.Convert(build.build.analysis_unit[0].status);
                    messageService.SendMessage(MessageTypes.BuildEvent, $"The build {build.build.version} with ID {build.build_id} status has been updated to {myBuild.Status}.");
                    myBuildRepo.Update(myBuild);
                }
            }

            return buildsToUpdate.ToArray();
        }

        static int HandleParseError(IEnumerable<Error> errs)
        {
            return 1;
        }
    }
}
