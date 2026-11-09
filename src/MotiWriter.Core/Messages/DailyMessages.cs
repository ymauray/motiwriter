namespace MotiWriter.Core.Messages;

/// <summary>
/// Le message du jour : le même pour tout le monde un jour donné, calculé à partir de la date
/// seule (rien n'est stocké). On parcourt la liste avec un pas premier avec sa longueur :
/// chaque message revient une fois par cycle, et deux jours voisins tombent loin l'un de l'autre.
/// </summary>
public static class DailyMessages
{
    /// <summary>
    /// Ton : bienveillant, jamais culpabilisant (voir docs/objectif.md). Pas de « ? », « ! »,
    /// « ; » ni « : », qui demanderaient une espace insécable.
    /// </summary>
    public static IReadOnlyList<string> All { get; } =
    [
        "Une phrase écrite vaut mieux qu'un chapitre rêvé.",
        "Le texte avance comme un sentier, un pas après l'autre.",
        "Écrire peu, mais souvent, c'est déjà écrire beaucoup.",
        "Une page imparfaite se corrige. Une page blanche, non.",
        "Il n'y a pas de petite séance d'écriture.",
        "Les mots d'hier attendent ceux d'aujourd'hui.",
        "Le premier jet n'a pas besoin d'être beau, seulement d'exister.",
        "Dix minutes suffisent pour retrouver le fil.",
        "Chaque jour écrit est une pierre de plus à l'édifice.",
        "On n'attend pas l'inspiration, on lui donne rendez-vous.",
        "Votre histoire n'existe que si vous l'écrivez.",
        "Écrire, c'est d'abord s'asseoir et commencer.",
        "Un paragraphe aujourd'hui, un livre un jour.",
        "La régularité fait plus que le talent seul.",
        "Même une ligne compte. Surtout une ligne.",
        "Le silence d'un jour n'efface rien de ce qui est écrit.",
        "Reprendre est un geste aussi précieux que commencer.",
        "Laissez le correcteur dormir, l'auteur a la main.",
        "Les brouillons sont le chantier des beaux livres.",
        "Votre voix mérite de laisser une trace sur la page.",
        "Ce que vous écrivez aujourd'hui, personne d'autre ne peut l'écrire.",
        "Un mot en appelle un autre. Il suffit du premier.",
        "Écrire un peu, c'est garder la porte ouverte.",
        "Le carnet ne juge pas, il garde.",
        "Une scène, un dialogue, une image, tout est bon à prendre.",
        "Les personnages vous attendent là où vous les avez laissés.",
        "Le temps passé à écrire n'est jamais perdu.",
        "Une idée notée est une idée sauvée.",
        "Ce n'est pas la longueur qui compte, c'est le geste.",
        "Écrire chaque jour, c'est apprendre chaque jour.",
        "La page d'aujourd'hui n'a pas à égaler celle d'hier.",
        "Un texte grandit à force de petites visites.",
        "Écrivez pour vous d'abord, le reste viendra.",
        "Il y a toujours une phrase qui veut sortir.",
        "Le rythme compte plus que la vitesse.",
        "Quelques mots tranquilles valent mieux qu'aucun.",
        "Chaque séance rend la suivante plus facile.",
        "Écrire, c'est se donner rendez-vous avec soi-même.",
        "Le doute fait partie du chemin, pas de l'arrivée.",
        "Une page de plus, et l'histoire respire.",
        "Ce que vous avez déjà écrit est bien réel.",
        "Les grands textes sont faits de petites journées.",
        "Écrire mal aujourd'hui, c'est pouvoir réécrire demain.",
        "Le plaisir d'écrire se cultive comme un jardin.",
        "Commencez par la phrase la plus simple.",
        "Le monde de votre histoire n'attend que vous.",
        "Une minute d'écriture vaut une heure d'hésitation.",
        "Les mots viennent en marchant, et en écrivant.",
        "Votre carnet garde la mémoire de chaque effort.",
        "Aucune journée d'écriture n'est trop modeste.",
        "Écrire, c'est choisir de croire à son histoire.",
        "La persévérance est la plus discrète des inspirations.",
        "Un texte n'est jamais fini, mais il peut toujours avancer.",
        "Faites confiance au fil, il se déroule en écrivant.",
        "Ce qui est écrit reste. C'est votre force.",
        "Le meilleur moment pour écrire est souvent maintenant.",
        "Une phrase sincère vaut mieux qu'une phrase parfaite.",
        "Chaque jour écrit rapproche le mot fin.",
        "Votre imagination ne se repose jamais longtemps.",
        "Écrire est un muscle doux, qui aime la régularité.",
    ];

    /// <summary>Le message du jour <paramref name="day"/>.</summary>
    public static string For(DateOnly day)
    {
        var count = All.Count;
        var index = (int)((long)day.DayNumber * Step(count) % count);
        return All[index];
    }

    /// <summary>
    /// Le plus petit entier au-delà du nombre d'or fois <paramref name="count"/> qui soit premier
    /// avec lui : multiplier par ce pas permute la liste.
    /// </summary>
    private static int Step(int count)
    {
        var step = (int)(count * 0.618) + 1;
        while (Gcd(step, count) != 1)
        {
            step++;
        }
        return step;
    }

    private static int Gcd(int a, int b) => b == 0 ? a : Gcd(b, a % b);
}
