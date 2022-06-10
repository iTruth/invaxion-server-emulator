﻿using Server.Emulator.EagleTcpPatches;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using BepInEx;
using System;
using System.Text;
using HarmonyLib;

// ReSharper disable All

namespace Server;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Server : BaseUnityPlugin
{
    public static BepInEx.Logging.ManualLogSource logger;
    public static Emulator.Database.Database Database;
    public static List<string> MustImplement = new();
    private GlobalsHelper _globalsHelperInstance;

    private void Awake()
    {
        logger = Logger;
        Database = new Emulator.Database.Database();
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        
        HookManager.Instance.Create();
        _globalsHelperInstance = GlobalsHelper.Instance;
        
        Emulator.Tools.Run.Every(20f, 10f, () =>
        {
            Logger.LogInfo("Current Scene: " + GlobalsHelper.CurrentScene);
        });

        Emulator.Tools.Run.After(60f, () =>
        {
            Logger.LogInfo("Attempting to dump the language file...");
            var output = new StringBuilder();
            Aquatrax.GlobalConfig.getInstance().languageList.ToList().ForEach(kv =>
            {
                output.Append(kv.Key);
                output.Append("\n");
            });
            File.WriteAllText("language.txt", output.ToString());
        });
    }

    private void OnApplicationQuit()
    {
        var commands = new List<string>();

        foreach (var command in MustImplement)
        {
            if (!commands.Contains(command))
            {
                commands.Add($"{command} (x{MustImplement.Count(command.Equals)})");
            }
        }
        
        var output = commands.Aggregate("", (current, line) => current + $"{line}\n");
        File.WriteAllText("must-implement.txt", output);
    }
}

public class GlobalsHelper
{
    private static GlobalsHelper _instance;
    public static GlobalsHelper Instance
    {
        get { return _instance ??= new GlobalsHelper(); }
    }

    public static string CurrentScene;

    private GlobalsHelper()
    {
        Emulator.Tools.Run.Every(60f, 3f, () =>
        {
            CurrentScene = GetCurrentScene();
        });
    }
    
    public string GetCurrentScene()
    {
        try
        {
            return Aquatrax.InputManager.currentRef.getCurentGroup();
        }
        catch (Exception e)
        {
            Server.logger.LogError(e);
            return "none";
        }
    }
}
