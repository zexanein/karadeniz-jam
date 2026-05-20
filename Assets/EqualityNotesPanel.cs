using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EqualityNotesPanel : BehaviourSingleton<EqualityNotesPanel>
{
    public Transform equalityNotesParent;
    public EqualityItem equalityNotePrefab;
    public List<EqualityNote> equalityNotes;

    private bool state;
    private Sequence _sequence;
    
    public void ToggleState()
    {
        state = !state;

        Soundmanager.TurnPaperSound = true;
        
        var targetY = state ? -5 : -385;
        var angle = state ? -3 : 0;
        _sequence?.Kill();
        _sequence = DOTween.Sequence();

        _sequence.Join((transform as RectTransform)
            .DOAnchorPosY(targetY, 0.4f)
            .SetEase(state ? Ease.OutBack : Ease.InOutBack));
        _sequence.Join(transform.DORotate(Vector3.forward * angle, 0.4f).SetEase(state ? Ease.OutBack : Ease.InOutBack));
    }

    public void RegisterNote(List<ItemEntry> leftEntries, List<ItemEntry> rightEntries, ComparisonType comparison)
    {
        var normalizedLeft = NormalizeEntries(leftEntries);
        var normalizedRight = NormalizeEntries(rightEntries);

        var crossGcd = GcdAcrossSides(normalizedLeft, normalizedRight);
        if (crossGcd > 1)
        {
            DivideEntries(normalizedLeft, crossGcd);
            DivideEntries(normalizedRight, crossGcd);
        }

        normalizedLeft.Sort((a, b) => string.Compare(a.blueprint.id, b.blueprint.id, StringComparison.Ordinal));
        normalizedRight.Sort((a, b) => string.Compare(a.blueprint.id, b.blueprint.id, StringComparison.Ordinal));

        // Canonical form: "Less" hep "Greater" olarak çevrilir (sol-sağ swap)
        if (comparison == ComparisonType.Less)
        {
            (normalizedLeft, normalizedRight) = (normalizedRight, normalizedLeft);
            comparison = ComparisonType.Greater;
        }

        var normalizedNote = new EqualityNote
        {
            leftEntries = normalizedLeft,
            rightEntries = normalizedRight,
            comparison = comparison
        };

        if (IsNoteRegistered(normalizedNote))
            return;

        Soundmanager.WriteNoteBookSound = true;
        equalityNotes.Add(normalizedNote);
        var noteUI = Instantiate(equalityNotePrefab, equalityNotesParent);
        noteUI.SetEntries(normalizedNote.leftEntries, normalizedNote.rightEntries, normalizedNote.comparison);
    }
    
    public bool IsNoteRegistered(List<ItemEntry> leftEntries, List<ItemEntry> rightEntries, ComparisonType comparison)
    {
        var normalizedLeft = NormalizeEntries(leftEntries);
        var normalizedRight = NormalizeEntries(rightEntries);

        var crossGcd = GcdAcrossSides(normalizedLeft, normalizedRight);
        if (crossGcd > 1)
        {
            DivideEntries(normalizedLeft, crossGcd);
            DivideEntries(normalizedRight, crossGcd);
        }

        normalizedLeft.Sort((a, b) => string.Compare(a.blueprint.id, b.blueprint.id, StringComparison.Ordinal));
        normalizedRight.Sort((a, b) => string.Compare(a.blueprint.id, b.blueprint.id, StringComparison.Ordinal));

        // Canonical form: Less → Greater (swap)
        if (comparison == ComparisonType.Less)
        {
            (normalizedLeft, normalizedRight) = (normalizedRight, normalizedLeft);
            comparison = ComparisonType.Greater;
        }

        var candidate = new EqualityNote
        {
            leftEntries = normalizedLeft,
            rightEntries = normalizedRight,
            comparison = comparison
        };

        return IsNoteRegistered(candidate);
    }
    private bool IsNoteRegistered(EqualityNote candidate)
    {
        foreach (var note in equalityNotes)
        {
            if (note.comparison != candidate.comparison) continue;

            // Equal için swap'i de eşit say (A=B ile B=A aynı)
            if (note.comparison == ComparisonType.Equal)
            {
                if (AreEntriesEqual(note.leftEntries, candidate.leftEntries) &&
                    AreEntriesEqual(note.rightEntries, candidate.rightEntries))
                    return true;

                if (AreEntriesEqual(note.leftEntries, candidate.rightEntries) &&
                    AreEntriesEqual(note.rightEntries, candidate.leftEntries))
                    return true;
            }
            else
            {
                // Greater için sıra önemli, doğrudan karşılaştır
                if (AreEntriesEqual(note.leftEntries, candidate.leftEntries) &&
                    AreEntriesEqual(note.rightEntries, candidate.rightEntries))
                    return true;
            }
        }
        return false;
    }

    private List<ItemEntry> NormalizeEntries(List<ItemEntry> entries)
    {
        var dict = new Dictionary<string, ItemEntry>();
        foreach (var e in entries)
        {
            if (dict.TryGetValue(e.blueprint.id, out var existing))
                existing.quantity += e.quantity;
            else
                dict[e.blueprint.id] = new ItemEntry { blueprint = e.blueprint, quantity = e.quantity };
        }
        return new List<ItemEntry>(dict.Values);
    }

    private int GcdAcrossSides(List<ItemEntry> left, List<ItemEntry> right)
    {
        int gcd = 0;
        foreach (var e in left) gcd = Gcd(gcd, e.quantity);
        foreach (var e in right) gcd = Gcd(gcd, e.quantity);
        return gcd == 0 ? 1 : gcd;
    }

    private void DivideEntries(List<ItemEntry> entries, int divisor)
    {
        foreach (var e in entries) e.quantity /= divisor;
    }

    private static int Gcd(int a, int b)
    {
        a = Math.Abs(a);
        b = Math.Abs(b);
        while (b != 0)
        {
            var t = b;
            b = a % b;
            a = t;
        }
        return a;
    }

    private bool AreEntriesEqual(List<ItemEntry> entries1, List<ItemEntry> entries2)
    {
        if (entries1.Count != entries2.Count) return false;

        for (int i = 0; i < entries1.Count; i++)
        {
            if (entries1[i].blueprint != entries2[i].blueprint || entries1[i].quantity != entries2[i].quantity)
                return false;
        }
        return true;
    }
}

public enum ComparisonType
{
    Equal,    // =
    Greater,  // >
    Less      // <  (canonical form'da kullanılmaz, çağrı sırasında Greater'a çevrilir)
}

[Serializable]
public class EqualityNote
{
    public List<ItemEntry> leftEntries;
    public List<ItemEntry> rightEntries;
    public ComparisonType comparison;
}