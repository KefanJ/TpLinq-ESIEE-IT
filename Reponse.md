# TP — C# séance 2 : LINQ (`REPONSES.md`)

---

## 0. Mise en place

### 0.3 Lire le fichier en LINQ
* **Résultat obtenu :** 13 notes valides et 6 inscrits (la ligne invalide de Barbara en Algo avec la valeur `abc` est écartée).
* **`Where(n => n is not null)` vs `OfType<Note>()` :**
  * Avec `Where(n => n is not null)`, le compilateur conserve le type `IEnumerable<Note?>` (nullable), ce qui oblige à gérer des avertissements de nullabilité par la suite.
  * Avec `OfType<Note>()`, le résultat est directement typé **`IEnumerable<Note>`** (non nullable). C'est plus pratique et plus propre car on peut accéder directement aux propriétés de `Note` sans avertissement ni opérateur `!`.

---

## 1. Délégués et lambdas

### 1.1 Un filtre générique
* **Type de la lambda :** `Func<Note, bool>`.
* **Peut-on écrire `var c = n => n.Valeur >= 15;` ?**
  * **Non.** Le compilateur renvoie une erreur (*impossible d'assigner une expression lambda à une variable implicitement typée* / type de `n` inconnu).
  * **Pourquoi :** sans type explicite à gauche (`Func<Note, bool>`), le compilateur ne sait pas de quel type est le paramètre `n` (sauf si on le précise dans la lambda : `var c = (Note n) => n.Valeur >= 15;`, autorisé depuis C# 10).

### 1.3 La capture de variables
* **Valeurs affichées :**
  * Premier affichage (`seuil = 12`) : **`10`**
  * Second affichage (`seuil = 15`) : **`6`**
* **Explication :** en C#, une lambda capture la **variable elle-même** (par référence) et non sa valeur au moment de la déclaration. Quand `seuil` passe à `15`, la lambda `admis` utilise la nouvelle valeur lors du second appel.
* **Différence avec Java :** en Java, ce code **ne compile pas**. Les lambdas Java ne peuvent capturer que des variables `final` ou *effectively final* (qui ne sont jamais réaffectées).
* **Comment figer le seuil à 12 ?** En copiant `seuil` dans une variable locale dédiée avant la lambda (par exemple `double seuilFige = seuil;`) et en utilisant `seuilFige` dans la lambda (ou en passant par une méthode fabrique comme `AuDessusDe(seuil)`).

---

## 2. Premières requêtes

### 2.1 Filtrer, projeter, trier
* **Deux `OrderBy` successifs vs `ThenBy` :**
  * Avec deux `OrderBy` successifs, le second **écrase** complètement le tri du premier : au final, les notes ne sont triées que selon le dernier critère.
  * Pour trier selon plusieurs critères hiérarchiques, il faut utiliser **`OrderBy`** pour le critère principal, puis **`ThenBy`** / **`ThenByDescending`** pour les critères secondaires.

### 2.2 Agréger
* **Valeurs obtenues :**
  * Nombre de notes : **13** (dont **1** inférieure à 10 : Alan en Algo avec 9)
  * Moyenne simple : **13,77**
  * Moyenne pondérée : **13,72**
  * Meilleure note : **18** (Grace en Algo)
* **Différence entre `Max` et `MaxBy` :**
  * `Max(n => n.Valeur)` renvoie uniquement la **valeur numérique** (`double`, ici `18`).
  * `MaxBy(n => n.Valeur)` renvoie l'**objet complet** (`Note?`, ici la note de Grace en Algo avec 18 et coef 3).
* **`Aggregate` vs `string.Join` :**
  * **`string.Join`** est préférable : c'est plus court, plus lisible, plus performant en mémoire (pas de concaténations intermédiaires), et surtout **sûr sur une séquence vide** (renvoie `""`, alors qu'`Aggregate` sans valeur initiale lève une `InvalidOperationException`).

### 2.3 Chercher et tester
* **`FirstOrDefault` vs `First` (Linus en BD) :**
  * `FirstOrDefault` renvoie **`null`** (Linus n'a pas de note en BD).
  * `First` lève une exception **`InvalidOperationException`** (*Sequence contains no matching element*).
* **`Single` :**
  * Sur Ada en Algo : renvoie son unique note (15).
  * Sur toutes les notes d'Ada : lève une **`InvalidOperationException`** (*Sequence contains more than one matching element*), car `Single` exige **strictement un seul** élément correspondant (Ada en a 3).
* **`Any` et `All` :**
  * Note < 10 : **`true`** (Alan, 9 en Algo).
  * Tous au moins 12 en BD : **`true`** (Ada 16, Alan 12, Grace 17, Barbara 13).
  * **`All` sur une séquence vide :** renvoie **`true`** (vérité par vacuité : aucun élément ne contredit la condition).

---

## 3. Regrouper et joindre

### 3.1 `GroupBy`
* **Type d'un groupe :** `IGrouping<string, Note>` (une collection `IEnumerable<Note>` dotée d'une propriété `Key` de type `string`).
* **Valeurs obtenues :**
  * Algo : 4 notes, moyenne **13,25**, major **Grace**
  * BD : 4 notes, moyenne **14,50**, major **Grace**
  * Réseau : 5 notes, moyenne **13,60**, major **Linus**
  * Moyennes pondérées : Grace **16,67**, Ada **15,17**, Barbara **13,67**, Linus **12,25**, Alan **10,33**.
* **`CountBy` (.NET 9) vs `GroupBy` + `Count` :**
  * `CountBy` est plus concis et **plus performant en mémoire** : il incrémente directement un compteur par clé sans allouer de listes intermédiaires pour stocker tous les objets `Note` de chaque groupe.

### 3.2 `Join`
* **Moyennes par promo :**
  * **M1 :** `13,00`
  * **M2 :** `15,00`
* **Pourquoi Margaret Hamilton n'apparaît-elle pas dans le relevé ?**
  * `Join` réalise une **jointure interne (*INNER JOIN*)** : un inscrit sans correspondance dans `notes` est exclu du résultat.
* **`ToLookup` avec un prénom absent :**
  * Contrairement à un `Dictionary` (qui lève une `KeyNotFoundException`), un `ILookup` renvoie une **séquence vide** (`IEnumerable<Note>` de taille 0), sans lever d'exception.

### 3.3 `SelectMany`
* **`SelectMany` vs `Select` :**
  * `Select(b => b.Valeurs)` renvoie un **`IEnumerable<List<double>>`** (une collection de listes).
  * `SelectMany(b => b.Valeurs)` aplatit les sous-listes et renvoie directement un **`IEnumerable<double>`**.

---

## 4. L'évaluation différée

1. **Valeurs affichées et explication :**
   * Ligne 1 : **`6`** (6 notes $\ge 15$ initialement).
   * Ligne 2 : **`7`** (après l'ajout du 19 de Margaret, `bonnes.Count()` ré-exécute la requête sur la liste modifiée).
   * Ligne 3 : **`8 / 7`** (`bonnes` ré-exécute encore la requête et inclut le 20 de Margaret, tandis que `figees` a été matérialisée par `.ToList()` avant ce second ajout et reste figée à 7).
2. **Effet de bord dans `Where` :**
   * Sans `ToList()` et avec deux appels à `Count()`, chaque note est testée **2 fois** (la requête est rejouée entièrement à chaque appel).
   * Avec `ToList()` d'abord, chaque note n'est testée qu'**1 seule fois** (au moment de la matérialisation).
3. **`Count() > 0` vs `Any()` :**
   * `Count() > 0` parcourt et teste **toutes** les notes de la liste (15 notes après les ajouts).
   * `Any()` s'arrête dès la première correspondance : ici, la toute première note du fichier (Ada, Algo, 15) vérifie déjà `>= 15`, donc **1 seule note** est testée.
4. **`Where` avant vs après `OrderBy` :**
   * Le **résultat est identique**.
   * Avec `Where` **avant** `OrderBy`, seules les notes filtrées ($\ge 15$) passent par le tri. Avec `Where` **après** `OrderBy`, **toutes** les notes sont triées inutilement avant d'être filtrées. Il faut toujours filtrer avant de trier.

---

## 5. Réécrire la classe `Promo`

1. **Division de deux `Sum` pour un étudiant sans note :**
   * Sur une séquence vide, `Sum` renvoie `0`. La division de `double` (`0.0 / 0.0`) ne lève pas d'exception mais renvoie **`double.NaN`**.
   * **Solution :** vérifier au préalable que l'étudiant possède au moins une note avec `.Any()` (ou que la somme des coefficients est non nulle) et renvoyer `0` sinon.
3. **Comparaison avec la séance 1 :**
   * **Nombre de lignes :** fortement réduit ; chaque méthode tient en une seule expression (`=>`) sans boucles ni variables d'accumulation.
   * **Lisibilité :** le style déclaratif de LINQ décrit *ce qu'on veut obtenir* plutôt que *comment parcourir les listes*, ce qui rend l'intention immédiate.
   * **Facilité à modifier :** ajouter un filtre, un critère de tri (`ThenBy`) ou changer un formatage se fait en ajoutant ou modifiant un opérateur dans la chaîne.
4. **Recalcul de `Classement()` et mise en cache :**
   * **Oui**, `Classement()` est recalculée intégralement à chaque appel (y compris via `Major()` et `Releve()`).
   * **Est-ce un problème ici ?** Non, avec une dizaine de notes le coût est négligeable.
   * **Quand mettre en cache ?** Si le volume de données devient important (milliers/millions de notes) et que `Classement()` est appelée fréquemment alors que `_notes` ne change pas. Comme `_notes` est en lecture seule après la construction, un cache via `Lazy<T>` ou un champ calculé dans le constructeur serait adapté.
