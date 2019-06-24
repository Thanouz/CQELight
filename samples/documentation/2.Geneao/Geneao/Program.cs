﻿using CQELight;
using CQELight.Dispatcher;
using CQELight.IoC;
using CQELight.EventStore.EFCore.Common;
using Geneao.Common.Commands;
using Geneao.Data;
using Geneao.Queries;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;
using CQELight.Abstractions.DDD;
using Geneao.Domain;
using System.Linq;
using Geneao.Common.Identity;
using Microsoft.Extensions.DependencyInjection;
using Geneao.Queries.Models.Out;

namespace Geneao
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Bienvenue dans la gestion de votre arbre généalogique");
            if (!File.Exists("./familles.json"))
            {
                File.WriteAllText("./familles.json", "[]");
            }
            new Bootstrapper()
                .OnlyIncludeDLLsForTypeSearching("Geneao")
                .UseInMemoryEventBus()
                .UseInMemoryCommandBus()
                .UseAutofacAsIoC(_ => { })
                .UseEFCoreAsEventStore(
                new CQELight.EventStore.EFCore.EFEventStoreOptions(
                    c => c.UseSqlite("FileName=events.db", opts => opts.MigrationsAssembly(typeof(Program).Assembly.GetName().Name)),
                    archiveBehavior: CQELight.EventStore.SnapshotEventsArchiveBehavior.Delete))
                .Bootstrapp();

            await DisplayMainMenuAsync();
        }

        private static async Task DisplayMainMenuAsync()
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("Choisissez votre commande");
                    Console.WriteLine("1. Lister les familles du logiciel");
                    Console.WriteLine("2. Créer une nouvelle famille");
                    Console.WriteLine("3. Ajouter une personne à une famille");
                    Console.WriteLine("Ou tapez q pour quitter");
                    Console.WriteLine();
                    var result = Console.ReadKey();
                    Console.WriteLine();
                    switch (result.Key)
                    {
                        case ConsoleKey.D1:
                        case ConsoleKey.NumPad1:
                            await ListerFamillesAsync();
                            break;
                        case ConsoleKey.D2:
                        case ConsoleKey.NumPad2:
                            await CreerFamilleAsync();
                            break;
                        case ConsoleKey.D3:
                        case ConsoleKey.NumPad3:
                            await AjouterPersonneAsync();
                            break;
                        case ConsoleKey.Q:
                            Environment.Exit(0);
                            break;
                        default:
                            Console.WriteLine("Choix incorrect, merci de faire un choix dans la liste");
                            break;
                    }
                }
                catch (Exception e)
                {
                    var color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.DarkRed;

                    Console.WriteLine("Aie, pas bien, ça a planté ... :(");
                    Console.WriteLine(e.ToString());

                    Console.ForegroundColor = color;
                }
            }
        }

        private static async Task AjouterPersonneAsync()
        {
            Console.WriteLine("Veuillez saisir la famille concernée");
            var familleConcernee = Console.ReadLine();
            using (var scope = DIManager.BeginScope())
            {
                var query = scope.Resolve<IRecupererListeFamille>();
                var famille = (await query.ExecuteQueryAsync()).FirstOrDefault(f => f.Nom == familleConcernee);
                if (famille != null)
                {
                    await CreerPersonneAsync(familleConcernee);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"La famille {familleConcernee} n'existe pas dans le système. Voulez-vous la créer ? (y/n)");
                    Console.ResetColor();
                    var response = Console.ReadLine();
                    if (response.Trim().Equals("y", StringComparison.OrdinalIgnoreCase))
                    {
                        await CreerFamilleCommandAsync(familleConcernee);
                    }
                }
            }
        }

        private static async Task CreerPersonneAsync(NomFamille nomFamille)
        {
            Console.WriteLine("Veuillez entrer le nom de la personne à créer");
            var prenom = Console.ReadLine();
            Console.WriteLine("Veuillez entrer le lieu de naissance de la personne à créer");
            var lieu = Console.ReadLine();
            Console.WriteLine("Veuillez entrer la date de naissance (dd/MM/yyyy)");
            DateTime date = DateTime.MinValue;
            DateTime.TryParse(Console.ReadLine(), out date);
            if (!string.IsNullOrWhiteSpace(prenom)
                && !string.IsNullOrWhiteSpace(lieu)
                && date != DateTime.MinValue)
            {
                var result = await CoreDispatcher.DispatchCommandAsync(
                    new AjouterPersonneCommand(nomFamille, prenom, lieu, date));
                if (!result)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    var message = $"La personne n'a pas pu être ajoutée à la famille {nomFamille.Value}";
                    if (result is Result<PersonneNonAjouteeCar> resultRaison)
                    {
                        switch (resultRaison.Value)
                        {
                            case PersonneNonAjouteeCar.InformationsDeNaissanceInvalides:
                                message += " car les informations de naissance sont invalides";
                                break;
                            case PersonneNonAjouteeCar.PersonneExistante:
                                message += " car cette personne existe déjà dans cette famille";
                                break;
                            case PersonneNonAjouteeCar.PrenomInvalide:
                                message += " car son prénom n'est pas reconnu valide";
                                break;
                        }
                    }
                    Console.WriteLine(message);
                }
            }
        }

        private static async Task ListerFamillesAsync()
        {
            using (var scope = DIManager.BeginScope())
            {
                var query = scope.Resolve<IRecupererListeFamille>();
                var familles = await query.ExecuteQueryAsync();
                Console.WriteLine("---- Liste des familles du système ----");
                foreach (var item in familles)
                {
                    Console.WriteLine(item.Nom);
                }
                Console.WriteLine();
            }
        }

        private static async Task CreerFamilleAsync()
        {
            string familleName = string.Empty;
            do
            {
                Console.WriteLine("Choisissez un nom de famille pour la créer");
                familleName = Console.ReadLine();
            }
            while (string.IsNullOrWhiteSpace(familleName));
            await CreerFamilleCommandAsync(familleName);
        }

        private static async Task CreerFamilleCommandAsync(string familleName)
        {
            var result = await CoreDispatcher.DispatchCommandAsync(new CreerFamilleCommand(familleName));
            if (!result)
            {
                var color = Console.ForegroundColor;

                Console.ForegroundColor = ConsoleColor.DarkRed;
                string raisonText = string.Empty;
                if (result is Result<FamilleNonCreeeCar> resultErreurFamille)
                {
                    raisonText =
                        resultErreurFamille.Value == FamilleNonCreeeCar.FamilleDejaExistante
                        ? "cette famille existe déjà."
                        : "le nom de la famille est incorrect.";
                }
                if (!string.IsNullOrWhiteSpace(raisonText))
                {
                    Console.WriteLine($"La famille {familleName} n'a pas pu être créée car {raisonText}");
                }

                Console.ForegroundColor = color;
            }
        }
    }
}
