using Zenith.Workers.Asistencia;
using Zenith.Workers.Asistencia.Abstractions;
using Zenith.Workers.Asistencia.Clients;
using Zenith.Workers.Asistencia.Options;
using Zenith.Workers.Asistencia.Providers;
using Zenith.Workers.Asistencia.Readers;
using Zenith.Workers.Asistencia.Services;
using Microsoft.Extensions.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<AsistenciaWorkerOptions>(builder.Configuration.GetSection(AsistenciaWorkerOptions.SectionName));

builder.Services.AddHttpClient<IChecadorConfigProvider, RemoteChecadorConfigProvider>();
builder.Services.AddSingleton<IMarcacionReader, ZkTecoMarcacionReader>();
builder.Services.AddHttpClient<IMarcacionSyncClient, HttpMarcacionSyncClient>();
builder.Services.AddHttpClient<IHeartbeatClient, HttpHeartbeatClient>();
builder.Services.AddSingleton<IAsistenciaSyncService, AsistenciaSyncService>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
