using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
namespace AA_Serverless_NET;

class Program
{
    static void Main(string[] args)
    {
        var personne = new Personne
        {
            Nom = "Alice",
            Age = 30
        };

        Console.WriteLine(personne.Hello(true));
        Console.WriteLine(personne.Hello(false));

        var personneJson = JsonConvert.SerializeObject(personne, Formatting.Indented);
        Console.WriteLine(personneJson);
        

        string inputFolder  = @"images_input";
        string outputFolder = @"images_output";

        if (!Directory.Exists(inputFolder))
        {
            Console.WriteLine($"Dossier introuvable : {inputFolder}");
            return;
        }

        Directory.CreateDirectory(outputFolder);

        // Récupère les JPG et PNG
        string[] files = Directory.GetFiles(inputFolder, "*.*");
        files = Array.FindAll(files, f =>
            f.EndsWith(".jpg",  StringComparison.OrdinalIgnoreCase) ||
            f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
            f.EndsWith(".png",  StringComparison.OrdinalIgnoreCase)
        );

        if (files.Length == 0)
        {
            Console.WriteLine("Aucune image JPG ou PNG trouvée dans le dossier.");
            return;
        }

        Console.WriteLine($"Traitement parallèle de {files.Length} image(s)...\n");

        var sw = Stopwatch.StartNew();

        Parallel.ForEach(files, file =>
        {
            using Image image = Image.Load(file);
            image.Mutate(x => x.Resize(image.Width / 2, image.Height / 2)
                .Rotate(RotateMode.Rotate180));
            string outputFile = Path.Combine(outputFolder, Path.GetFileName(file));
            image.Save(outputFile); // ImageSharp détecte l'extension et encode en conséquence

            Console.WriteLine($"  Traité : {Path.GetFileName(file)} → {image.Width}x{image.Height}");
        });

        sw.Stop();
        Console.WriteLine($"\nTerminé en {sw.ElapsedMilliseconds} ms");
    }
}
