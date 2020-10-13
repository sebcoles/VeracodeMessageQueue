using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VeracodeMessageQueue.MessagingService;
using VeracodeMessageQueue.Models;
using VeracodeMessageQueue.Profiles;
using VeracodeMessageQueue.Storage;
using VeracodeService;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

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
            var connection = Configuration.GetConnectionString("DefaultConnection");
            serviceCollection.AddDbContext<ApplicationDbContext>
                (options => options.UseSqlServer(connection));

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
            Console.WriteLine($"Starting...");
            var messageService = _serviceProvider.GetService<IMessageService>();
            var veracodeRepository = _serviceProvider.GetService<IVeracodeRepository>();
            var myAppRepo = _serviceProvider.GetService<IGenericRepository<App>>();
            var myBuildRepo = _serviceProvider.GetService<IGenericRepository<Build>>();
            var myFlawRepo = _serviceProvider.GetService<IGenericRepository<Flaw>>();

            if (options.All)
            {
                Console.WriteLine($"Running against all apps available.");
                _appIds = veracodeRepository.GetAllApps()
                    .Select(x => x.app_id.ToString()).ToArray();
            } else {
                Console.WriteLine($"Running against apps in configuration.");
            }

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
            Console.WriteLine($"Checking for App Events.");

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
                Console.WriteLine($"The application {app.Name} with ID {appId} was deleted from Veracode.");
                messageService.SendMessage(MessageTypes.AppEvent, $"The application {app.Name} with ID {appId} was deleted from Veracode.", app);
                myAppRepo.Delete(app);
            }

            var addedAppsIds = _appIds.Except(currentAppsInDb);
            foreach (var appId in addedAppsIds)
            {
                var app = veracodeRepository.GetAppDetail(appId);
                Console.WriteLine($"The application {app.application[0].app_name} with ID {appId} was created in Veracode.");
                messageService.SendMessage(MessageTypes.AppEvent, $"The application {app.application[0].app_name} with ID {appId} was created in Veracode.", app);
                myAppRepo.Create(new App { Id = appId, Name = app.application[0].app_name });
            }

            Console.WriteLine($"Finished App Events.");
        }

        private static string[] BuildEvents(string appId)
        {
            Console.WriteLine($"Checking for Build Events.");
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
                Console.WriteLine($"The build {build.Name} with ID {build.Id} was deleted from Veracode.");
                messageService.SendMessage(MessageTypes.BuildEvent, $"The build {build.Name} with ID {build.Id} was deleted from Veracode.", build);
                myBuildRepo.Delete(build);
            }

            var addedBuildIds = buildIds.Except(currentBuildsInDb);
            foreach (var buildId in addedBuildIds)
            {
                var build = veracodeRepository.GetBuildDetail(appId, buildId);
                Console.WriteLine($"The build {build.build.version} with ID {build.build_id} was created from Veracode.");
                messageService.SendMessage(MessageTypes.BuildEvent, $"The build {build.build.version} with ID {build.build_id} was created from Veracode.", build);
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
                    Console.WriteLine($"The build {build.build.version} with ID {build.build_id} status has been updated to {myBuild.Status}.");
                    messageService.SendMessage(MessageTypes.BuildEvent, $"The build {build.build.version} with ID {build.build_id} status has been updated to {myBuild.Status}.", myBuild);
                    myBuildRepo.Update(myBuild);
                }
            }
            Console.WriteLine($"Finished Build Events.");
            return buildsToUpdate.ToArray();
        }

        static int HandleParseError(IEnumerable<Error> errs)
        {
            return 1;
        }
    }
}
