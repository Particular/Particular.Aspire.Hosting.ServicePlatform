

var builder = new DistributedApplicationBuilder(args);

builder.AddParticularPlatform("particular")
    .AddDefaultComponents();

builder.Build().Run();
