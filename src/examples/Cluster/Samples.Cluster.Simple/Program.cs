//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2015 Typesafe Inc. <http://www.typesafe.com>
//     Copyright (C) 2013-2015 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Akka.Configuration.Hocon;
using Akka.Contrib.Pattern;

namespace Samples.Cluster.Simple
{
    class Program
    {
        private static void Main(string[] args)
        {
            StartUp(args.Length == 0 ? new String[] {"2551", "2552", "0"} : args);
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        public static void StartUp(string[] ports)
        {
            var section = (AkkaConfigurationSection)ConfigurationManager.GetSection("akka");
            var systems = new ActorSystem[3];
            var count = 0;
            foreach (var port in ports)
            {
                //Override the configuration of the port
                var config =
                    ConfigurationFactory.ParseString("akka.remote.helios.tcp.port=" + port)
                        .WithFallback(ConfigurationFactory.ParseString("akka.cluster.roles = [compute]"))
                        .WithFallback(section.AkkaConfig);

                //create an Akka system
                var system = ActorSystem.Create("ClusterSystem", config);
                systems[count] = system;

                count++;


                /**
                
                var proxy = system.ActorOf(
                    ClusterSingletonProxy.Props(
                        "/user/singleton/greet",
                        null,
                        TimeSpan.FromSeconds(1.0)),
                        "greetProxy"
                        ); 
                proxy.Tell(new Greet(port));
                **/
            }

            Thread.Sleep(1000);

            foreach (var system in systems)
            {
                system.ActorOf(
                  ClusterSingletonManager.Props(
                      Props.Create<GreetingActor>(),
                      "greet",
                      PoisonPill.Instance,
                      "compute",
                      10,
                      5,
                      TimeSpan.FromSeconds(1.0)),
                  "singleton");
            }

        }
    }

}

