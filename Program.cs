using System.Globalization;

// ============================================================================
// TP2 C# : Délégués, lambdas et LINQ
// Règle respectée : à partir de l'exercice 2, aucune boucle ne sert à calculer
// un résultat (les boucles foreach servent uniquement à l'affichage).
// ============================================================================

// ============================================================================
// EXERCICE 0 : Mise en place et lecture des fichiers CSV en LINQ
// ============================================================================
Console.WriteLine("===== EXERCICE 0 : Lecture CSV en LINQ =====");

// Transforme une ligne CSV en Note, ou renvoie null si la ligne est invalide
static Note? Parser(string ligne)
{
    var c = ligne.Split(',', StringSplitOptions.TrimEntries);
    if (c.Length != 5) return null;
    if (!double.TryParse(c[3], CultureInfo.InvariantCulture, out var v)) return null;
    if (v < 0 || v > 20) return null;
    if (!int.TryParse(c[4], out var coef)) return null;
    return new Note(c[0], c[1], c[2], v, coef);
}

// Questions 1 & 2 : Lecture sans boucle de notes.csv avec OfType<Note>()
// OfType<Note>() écarte les null ET renvoie directement un IEnumerable<Note> (non nullable).
var notes = File.ReadLines("notes.csv")
    .Skip(1)
    .Select(Parser)
    .OfType<Note>()
    .ToList();

// Question 3 : Lecture de etudiants.csv dans une List<Inscrit>
var inscrits = File.ReadLines("etudiants.csv")
    .Skip(1)
    .Select(ligne =>
    {
        var elements = ligne.Split(',', StringSplitOptions.TrimEntries);
        return new Inscrit(elements[0], elements[1], elements[2]);
    })
    .ToList();

Console.WriteLine($"Notes valides chargées : {notes.Count} | Inscrits chargés : {inscrits.Count}");

// ============================================================================
// EXERCICE 1 : Délégués et lambdas
// ============================================================================
Console.WriteLine("\n===== EXERCICE 1 : Délégués et lambdas =====");

// --- 1.1 Un filtre générique ---

// Question 1 : Implémentation manuelle de Where (seule boucle de calcul autorisée dans le TP)
static List<Note> Filtrer(List<Note> source, Func<Note, bool> critere)
{
    var res = new List<Note>();
    foreach (var n in source)
    {
        if (critere(n)) res.Add(n);
    }
    return res;
}

// Question 2 : Appels avec une méthode nommée et des expressions lambdas
static bool EstEnAlgo(Note n) => n.Matiere == "Algo";

var notesAlgoFiltrees = Filtrer(notes, EstEnAlgo);
var notesSous10Filtrees = Filtrer(notes, n => n.Valeur < 10);
var notesAda15Plus = Filtrer(notes, n => n.Valeur >= 15 && n.Prenom == "Ada");
Console.WriteLine($"Filtres (1.1) -> Algo : {notesAlgoFiltrees.Count}, < 10 : {notesSous10Filtrees.Count}, Ada >= 15 : {notesAda15Plus.Count}");

// Question 3 : Stockage explicite dans une variable de type Func<Note, bool>
// (var c = n => ... ne compile pas car le type de 'n' ne peut pas être inféré sans type cible).
Func<Note, bool> critere15Plus = n => n.Valeur >= 15;
Console.WriteLine($"Notes >= 15 (via variable Func) : {Filtrer(notes, critere15Plus).Count}");

// --- 1.2 Func, Action, Predicate ---

// Question 1 : Déclaration d'un Func<Note, double> et d'un Action<Note>
Func<Note, double> pointsPonderes = n => n.Valeur * n.Coef;
Action<Note> afficherNote = n => Console.WriteLine($"  {n.Prenom}. {n.Matiere} {n.Valeur} (pts pondérés : {pointsPonderes(n)})");

Console.WriteLine("Exemple d'affichage via Action<Note> :");
afficherNote(notes[0]);

// Question 2 : Méthode fabrique renvoyant une lambda
static Func<Note, bool> AuDessusDe(double seuil) => n => n.Valeur >= seuil;

var notesAuDessusDe14 = Filtrer(notes, AuDessusDe(14));
Console.WriteLine($"Notes >= 14 (AuDessusDe) : {notesAuDessusDe14.Count}");

// Question 3 : Tri et suppression sur une COPIE de la liste (pour ne pas altérer 'notes' pour la suite)
// La comparaison dépassant 3 lignes, elle est extraite dans une méthode nommée conformément aux règles du TP.
static int ComparerParMatierePuisValeurDesc(Note a, Note b)
{
    int cmpMatiere = a.Matiere.CompareTo(b.Matiere);
    return cmpMatiere != 0 ? cmpMatiere : b.Valeur.CompareTo(a.Valeur);
}

var copieNotes = new List<Note>(notes);
copieNotes.Sort(ComparerParMatierePuisValeurDesc);
int nbSupprimees = copieNotes.RemoveAll(n => n.Matiere == "Reseau");
Console.WriteLine($"Copie triée et nettoyée : {nbSupprimees} notes de Réseau retirées ({copieNotes.Count} restantes dans la copie).");

// --- 1.3 La capture de variables (Closure) ---
// Test commenté : la lambda capture la référence de 'seuil' (affiche 10 puis 6).
/*
double seuil = 12;
Func<Note, bool> admis = n => n.Valeur >= seuil;
Console.WriteLine($"Admis (seuil 12) : {Filtrer(notes, admis).Count}");
seuil = 15;
Console.WriteLine($"Admis (seuil 15) : {Filtrer(notes, admis).Count}");
*/

// ============================================================================
// EXERCICE 2 : Premières requêtes (filtrer, projeter, trier, agréger)
// Note : Les blocs ci-dessous ont été testés et commentés pour garder une
// sortie console lisible centrée sur la classe Promo (Exercice 5).
// ============================================================================

/*
// --- 2.1 Filtrer, projeter, trier ---

// 1. Notes d'Algo triées par valeur décroissante
var noteAlgo = notes
    .Where(n => n.Matiere == "Algo")
    .OrderByDescending(n => n.Valeur)
    .ToList();

// 2. Liste des matières sans doublon et triée
var matieresSansDoublon = notes
    .Select(n => n.Matiere)
    .Distinct()
    .Order()
    .ToList();

// 3. Prénom et valeur des notes < 10 (type anonyme puis tuple)
var etudiantEnDessousDe10 = notes
    .Where(n => n.Valeur < 10)
    .Select(n => new { n.Prenom, n.Valeur })
    .ToList();

var etudiantEnDessousDe10Tuple = notes
    .Where(n => n.Valeur < 10)
    .Select(n => (n.Prenom, n.Valeur))
    .ToList();

// 4. Classement complet : deux OrderBy successifs écrasent le 1er tri -> correction avec ThenBy
var classementCompletAvecThen = notes
    .OrderBy(n => n.Matiere)
    .ThenByDescending(n => n.Valeur)
    .ThenBy(n => n.Prenom)
    .ToList();

// 5. Top 3 puis places 4 à 6 avec Skip et Take
var troisMeilleuresNotes = notes.OrderByDescending(n => n.Valeur).Take(3).ToList();
var quatreASixiemeNote = notes.OrderByDescending(n => n.Valeur).Skip(3).Take(3).ToList();

// --- 2.2 Agréger ---

// 1. Nombre total, nombre < 10, moyenne simple (13,77)
var nbNotes = notes.Count();
var nbNotesInferDix = notes.Count(n => n.Valeur < 10);
var moyenneNotes = notes.Average(n => n.Valeur);

// 2. Max (valeur double) vs MaxBy (objet Note complet)
double meilleureNote = notes.Max(n => n.Valeur);
Note noteComplete = notes.MaxBy(n => n.Valeur)!;

// 3. Moyenne pondérée globale (13,72)
double ponderee = notes.Sum(n => n.Valeur * n.Coef) / notes.Sum(n => n.Coef);

// 4. Chaîne des prénoms distincts : Aggregate vs string.Join
string listeAggregate = notes.Select(n => n.Prenom).Distinct().Aggregate((acc, nom) => acc + ", " + nom);
string listeAvecJoin = string.Join(", ", notes.Select(n => n.Prenom).Distinct());

// --- 2.3 Chercher et tester ---

// 1. FirstOrDefault (renvoie null pour Linus en BD) vs First (lève InvalidOperationException)
var noteLinusBd = notes.FirstOrDefault(n => n.Prenom == "Linus" && n.Matiere == "BD");

// 2. Single sur une note unique (Ada en Algo) vs plusieurs notes (Ada toutes matières -> exception)
var noteAdaAlgo = notes.Single(n => n.Prenom == "Ada" && n.Matiere == "Algo");

// 3. Any, All et All sur une séquence vide (renvoie true par vacuité)
bool existeSous10 = notes.Any(n => n.Valeur < 10);
bool tousAuMoins12EnBd = notes.Where(n => n.Matiere == "BD").All(n => n.Valeur >= 12);
bool allSurVide = Enumerable.Empty<Note>().All(n => n.Valeur >= 12);
*/

// ============================================================================
// EXERCICE 3 : Regrouper et joindre (GroupBy, Join, GroupJoin, ToLookup, SelectMany)
// ============================================================================

/*
// --- 3.1 GroupBy ---

// 1 & 2. Regroupement et statistiques par matière triées par moyenne décroissante
var statsParMatiere = notes
    .GroupBy(n => n.Matiere)
    .Select(g => new
    {
        Matiere = g.Key,
        NbNotes = g.Count(),
        Moyenne = g.Average(n => n.Valeur),
        Min = g.Min(n => n.Valeur),
        Max = g.Max(n => n.Valeur),
        Major = g.MaxBy(n => n.Valeur)!.Prenom
    })
    .OrderByDescending(s => s.Moyenne);

// 3. Dictionnaire Prénom -> Moyenne pondérée via ToDictionary
Dictionary<string, double> moyennesEtudiants = notes
    .GroupBy(n => n.Prenom)
    .ToDictionary(
        g => g.Key,
        g => g.Sum(n => n.Valeur * n.Coef) / g.Sum(n => n.Coef)
    );

// 4. Comptage par étudiant : CountBy vs GroupBy + Count
var nbNotesCountBy = notes.CountBy(n => n.Prenom);
var nbNotesGroupBy = notes.GroupBy(n => n.Prenom).Select(g => (Prenom: g.Key, Nb: g.Count()));

// --- 3.2 Join ---

// 1 & 2. Jointure interne sur clé composite (Nom, Prenom) et moyenne par promo
var releve = notes.Join(
    inscrits,
    n => (n.Nom, n.Prenom),
    i => (i.Nom, i.Prenom),
    (n, i) => new { i.Prenom, i.Promo, n.Matiere, n.Valeur }
).ToList();

var moyennesParPromo = releve
    .GroupBy(r => r.Promo)
    .Select(g => new { Promo = g.Key, Moyenne = g.Average(r => r.Valeur) });

// 3. Inscrits sans note (Margaret Hamilton) via Where+Any puis GroupJoin
var sansNoteWhere = inscrits.Where(i => !notes.Any(n => n.Nom == i.Nom && n.Prenom == i.Prenom));
var sansNoteGroupJoin = inscrits
    .GroupJoin(notes, i => (i.Nom, i.Prenom), n => (n.Nom, n.Prenom), (i, ns) => new { Inscrit = i, Notes = ns })
    .Where(x => !x.Notes.Any())
    .Select(x => x.Inscrit);

// 4. Indexation avec ToLookup (une clé absente renvoie une séquence vide sans exception)
ILookup<string, Note> indexNotes = notes.ToLookup(n => n.Prenom);

// --- 3.3 SelectMany ---

// 1, 2 & 3. Construction des bulletins, mise à plat avec SelectMany et paires (Prenom, Valeur)
List<Bulletin> bulletins = notes
    .GroupBy(n => n.Prenom)
    .Select(g => new Bulletin(g.Key, g.Select(n => n.Valeur).ToList()))
    .ToList();

IEnumerable<double> toutesLesValeurs = bulletins.SelectMany(b => b.Valeurs);
var paires = bulletins.SelectMany(b => b.Valeurs, (bulletin, valeur) => (bulletin.Prenom, valeur));
*/

// ============================================================================
// EXERCICE 4 : L'évaluation différée
// Note : Les effets de bord (Console.WriteLine dans Where) ont été commentés
// après vérification afin de respecter la grille de rendu du TP.
// ============================================================================

/*
// 1. Comparaison entre requête différée (bonnes) et résultat figé en mémoire (ToList)
var bonnes = notes.Where(n => n.Valeur >= 15);
Console.WriteLine(bonnes.Count()); // 6
notes.Add(new Note("Hamilton", "Margaret", "Algo", 19, 3));
Console.WriteLine(bonnes.Count()); // 7 (réévalué dynamiquement)

var figees = notes.Where(n => n.Valeur >= 15).ToList();
notes.Add(new Note("Hamilton", "Margaret", "BD", 20, 2));
Console.WriteLine(bonnes.Count() + " / " + figees.Count); // 8 / 7

// 2 & 3. Effet de bord pour observer la réexécution et le court-circuit de Any()
var bonnesTrace = notes.Where(n =>
{
    Console.WriteLine($"  test de {n.Prenom} ({n.Matiere}) : {n.Valeur}");
    return n.Valeur >= 15;
});
bool avecAny = bonnesTrace.Any(); // Ne teste qu'1 seul élément puis s'arrête

// 4. Where avant OrderBy (trie 6 notes) vs Where après OrderBy (trie les 13 notes)
var resAvant = notes.Where(n => n.Valeur >= 15).OrderBy(n => n.Valeur).ToList();
var resApres = notes.OrderBy(n => n.Valeur).Where(n => n.Valeur >= 15).ToList();
*/

// ============================================================================
// EXERCICE 5 : Réécrire la classe Promo en expressions LINQ.
// ============================================================================
Console.WriteLine("\n===== EXERCICE 5 : Classe Promo en LINQ =====");

var promo = new Promo("M1 Info", notes);

Console.WriteLine($"=== Promo : {promo.Nom} ===");
Console.WriteLine($"Étudiants : {string.Join(", ", promo.Etudiants())}");
Console.WriteLine($"Moyenne de Grace : {promo.Moyenne("Grace"):F2}");
Console.WriteLine($"Moyenne d'un absent : {promo.Moyenne("Inconnu"):F2}");
Console.WriteLine($"Moyenne générale : {promo.MoyenneGenerale():F2}");
Console.WriteLine($"Major : {promo.Major()}");

Console.WriteLine("\n--- Admis par matière ---");
foreach (var (matiere, nb) in promo.NbAdmisParMatiere())
{
    Console.WriteLine($"{matiere} : {nb} admis");
}

Console.WriteLine("\n--- Relevé ----");
foreach (var ligne in promo.Releve())
{
    Console.WriteLine(ligne);
}