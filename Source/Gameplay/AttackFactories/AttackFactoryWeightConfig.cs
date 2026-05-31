namespace PromisedEigong.Gameplay.AttackFactories;

#nullable disable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using BepInEx;
using Core;
using UnityEngine;

static class AttackFactoryWeightConfig
{
    const string JsonFileName = "PromisedEigong.AttackFactoryWeights.json";
    const string TxtFileName = "PromisedEigong.AttackFactoryWeights.txt";

    static readonly Dictionary<string, int> configuredWeights = new();
    static readonly Dictionary<string, AttackFactoryWeightEntry> defaultWeights = new();
    static bool hasLoaded;

    static string JsonPath => Path.Combine(Paths.ConfigPath, JsonFileName);
    static string TxtPath => Path.Combine(Paths.ConfigPath, TxtFileName);

    public static int GetWeight (string attack, int phase, string target, int defaultWeight)
    {
        LoadIfNeeded();

        var key = BuildKey(attack, phase, target);
        defaultWeights[key] = new AttackFactoryWeightEntry
        {
            attack = attack,
            phase = phase,
            target = target,
            weight = defaultWeight
        };

        return configuredWeights.TryGetValue(key, out var configuredWeight) ? configuredWeight : defaultWeight;
    }

    public static void WriteDefaultsIfNeeded ()
    {
        LoadIfNeeded();

        if (ShouldPreserveExistingJsonConfig() || defaultWeights.Count == 0)
            return;

        try
        {
            Directory.CreateDirectory(Paths.ConfigPath);
            var configFile = new AttackFactoryWeightConfigFile();
            configFile.weights.AddRange(defaultWeights.Values);
            File.WriteAllText(JsonPath, JsonUtility.ToJson(configFile, true));
            KLog.Info($"Created default attack factory weight config at {JsonPath}");
        }
        catch (Exception exception)
        {
            KLog.Warning($"Could not create default attack factory weight config: {exception.Message}");
        }
    }

    public static void Reload ()
    {
        configuredWeights.Clear();
        hasLoaded = false;
        LoadIfNeeded();
    }

    public static bool TryGetConfiguredWeight (string attack, int phase, AttackWeight attackWeight, out int weight)
    {
        weight = 0;

        if (attackWeight.state == null)
            return false;

        var key = BuildKey(attack, phase, attackWeight.state.name);
        if (!defaultWeights.TryGetValue(key, out var defaultWeight))
            return false;

        weight = configuredWeights.TryGetValue(key, out var configuredWeight) 
            ? configuredWeight 
            : defaultWeight.weight;

        return true;
    }

    static void LoadIfNeeded ()
    {
        if (hasLoaded)
            return;

        hasLoaded = true;

        if (File.Exists(JsonPath))
        {
            LoadJson();
            return;
        }

        if (File.Exists(TxtPath))
            LoadTxt();
    }

    static bool ShouldPreserveExistingJsonConfig ()
    {
        if (!File.Exists(JsonPath))
            return false;

        var jsonText = File.ReadAllText(JsonPath);
        if (string.IsNullOrWhiteSpace(jsonText))
            return false;

        try
        {
            var configFile = JsonUtility.FromJson<AttackFactoryWeightConfigFile>(jsonText);
            return configFile?.weights is { Count: > 0 };
        }
        catch
        {
            return true;
        }
    }

    static void LoadJson ()
    {
        try
        {
            var configFile = JsonUtility.FromJson<AttackFactoryWeightConfigFile>(File.ReadAllText(JsonPath));
            if (configFile?.weights == null)
                return;

            foreach (var entry in configFile.weights)
                AddConfiguredWeight(entry);
        }
        catch (Exception exception)
        {
            KLog.Warning($"Could not load attack factory weight JSON config: {exception.Message}");
        }
    }

    static void LoadTxt ()
    {
        try
        {
            foreach (var line in File.ReadAllLines(TxtPath))
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.Length == 0 || trimmedLine.StartsWith("#"))
                    continue;

                var equalsIndex = trimmedLine.LastIndexOf('=');
                if (equalsIndex <= 0 || equalsIndex == trimmedLine.Length - 1)
                    continue;

                var weightText = trimmedLine[(equalsIndex + 1)..].Trim();
                if (!int.TryParse(weightText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var weight))
                    continue;

                var keyParts = trimmedLine[..equalsIndex].Split('|');
                if (keyParts.Length != 3 || !int.TryParse(keyParts[1].Trim(), out var phase))
                    continue;

                AddConfiguredWeight(new AttackFactoryWeightEntry
                {
                    attack = keyParts[0].Trim(),
                    phase = phase,
                    target = keyParts[2].Trim(),
                    weight = weight
                });
            }
        }
        catch (Exception exception)
        {
            KLog.Warning($"Could not load attack factory weight TXT config: {exception.Message}");
        }
    }

    static void AddConfiguredWeight (AttackFactoryWeightEntry entry)
    {
        if (entry == null || string.IsNullOrEmpty(entry.attack) || string.IsNullOrEmpty(entry.target))
            return;

        configuredWeights[BuildKey(entry.attack, entry.phase, entry.target)] = entry.weight;
    }

    static string BuildKey (string attack, int phase, string target) => $"{attack}|{phase}|{target}";
}

[Serializable]
class AttackFactoryWeightConfigFile
{
    public List<AttackFactoryWeightEntry> weights = new();
}

[Serializable]
class AttackFactoryWeightEntry
{
    public string attack;
    public int phase;
    public string target;
    public int weight;
}
