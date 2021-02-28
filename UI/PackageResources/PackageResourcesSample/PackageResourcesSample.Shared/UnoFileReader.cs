﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using FrameGenerator.FileReading;
using FrameGenerator.Models;
using SkiaSharp;

namespace PackageResourcesSample.Shared
{
    class UnoFileReader : IReadFromFile
    {
        public Dictionary<string, string> GetDictionaryFromFile(string path)
        {
            var dict = new Dictionary<string, string>();

            //var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            //Console.WriteLine(localFolder.Path);
            ////var folders = await localFolder.GetFoldersAsync();
            //foreach (var storageFolder in folders)
            //{
            //    foreach (var storageFile in await storageFolder.GetFilesAsync())
            //    {
            //        Console.WriteLine(storageFile.Name);
            //    }
            //}

            //string[] lines = FileIO.ReadLinesAsync().GetResults();

            //for (var i = 0; i < lines.Length; i += 2)
            //{
            //    dict[lines[i]] = lines[i + 1];
            //}

            return dict;
        }

        public Dictionary<string, string> GetMonsterData(string file, string monsterOverrideFile)
        {
            var monster = new Dictionary<string, string>();

            string[] lines = File.ReadAllLines(file);

            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("  MONS_"))
                {
                    string[] tokens = lines[i].Split(',');
                    tokens[1] = tokens[1].Replace("'", "").Replace(" ", "");
                    tokens[2] = tokens[2].Replace(" ", "");
                    tokens[0] = tokens[0].Replace("MONS_", "").Replace(" ", "").ToLower();
                    //if(!Enum.TryParse(tokens[2], out ColorList2 res)) Console.WriteLine(tokens[1] + tokens[2] + " badly colored: " + tokens[0]);
                    if (monster.TryGetValue(tokens[1] + tokens[2], out var existing))
                    {
                        //Console.WriteLine(tokens[1] + tokens[2] + "exist: " + existing + " new: " + tokens[0]); 
                    }
                    else monster[tokens[1] + tokens[2]] = tokens[0];
                }
            }

            //Overrides for duplicates, others handled by name from monster log

            lines = File.ReadAllLines(monsterOverrideFile);

            foreach (var line in lines)
            {
                var keyValue = line.Split(' ');
                monster[keyValue[0]] = keyValue[1];
            }

            monster.Remove("8BLUE"); //remove roxanne impersonating statue
            return monster;
        }

        public Dictionary<string, string> GetWeaponData(string file)
        {
            var weapon = new Dictionary<string, string>();

            string[] lines = File.ReadAllLines(file);

            for (var i = 0; i < lines.Length; i += 2)
            {
                weapon[lines[i]] = lines[i + 1];
            }

            return weapon;
        }

        public List<NamedMonsterOverride> GetNamedMonsterOverrideData(string monsterOverrideFile)
        {
            var monster = new List<NamedMonsterOverride>();

            string[] lines = File.ReadAllLines(monsterOverrideFile);

            var name = "";
            var location = "";
            var tileNameOverrides = new Dictionary<string, string>(20);

            bool pngParse = false;

            for (var i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                {
                    monster.Add(new NamedMonsterOverride(name, location, tileNameOverrides));
                    name = "";
                    location = "";
                    tileNameOverrides = new Dictionary<string, string>(20);
                    pngParse = false;
                    continue;
                }

                if (pngParse)
                {
                    string[] tokens = lines[i].Split(' ');
                    tileNameOverrides.Add(tokens[0], tokens[1]);
                }
                else
                {
                    string[] tokens = lines[i].Split(';');
                    name = tokens[0];
                    location = tokens.Length > 1 ? tokens[1] : "";
                    pngParse = true;
                }
            }

            return monster;
        }

        public Dictionary<string, string[]> GetFloorAndWallNamesForDungeons(string file)
        {
            var floorandwall = new Dictionary<string, string[]>();
            string[] lines = File.ReadAllLines(file);

            for (var i = 0; i < lines.Length; i += 3)
            {
                string[] temp = new string[2];
                temp[0] = lines[i + 1];
                temp[1] = lines[i + 2];
                floorandwall[lines[i].ToUpper()] = temp;
            }

            return floorandwall;
        }

        public Dictionary<string, SkiaSharp.SKBitmap> GetSKBitmapDictionaryFromFolder(string folder)
        {
            var dict = new Dictionary<string, SkiaSharp.SKBitmap>();
            List<string> pngFiles = Directory.GetFiles(folder, "*.png*", SearchOption.AllDirectories).ToList();
            var files = Directory
                .GetFiles(folder.Substring(0, folder.IndexOf("Extra", StringComparison.OrdinalIgnoreCase) + 5), "*.png",
                    SearchOption.TopDirectoryOnly).ToList();
            pngFiles.AddRange(files);
            foreach (var file in pngFiles)
            {
                FileInfo info = new FileInfo(file);
                SkiaSharp.SKBitmap SKBitmap = SkiaSharp.SKBitmap.Decode(file);
                dict[info.Name.Replace(".png", "")] = SKBitmap;
            }

            return dict;
        }

        public Dictionary<string, SkiaSharp.SKBitmap> GetCharacterPNG(string gameLocation)
        {
            var GetCharacterPNG = new Dictionary<string, SkiaSharp.SKBitmap>();

            List<string> allpngfiles = Directory
                .GetFiles(gameLocation + @"/rltiles/player/base", "*.png*", SearchOption.AllDirectories).ToList();
            allpngfiles.AddRange(Directory
                .GetFiles(gameLocation + @"/rltiles/player/felids", "*.png*", SearchOption.AllDirectories).ToList());
            foreach (var file in allpngfiles)
            {
                FileInfo info = new FileInfo(file);
                SkiaSharp.SKBitmap SKBitmap = SkiaSharp.SKBitmap.Decode(file);


                GetCharacterPNG[info.Name.Replace(".png", "")] = SKBitmap;
            }

            return GetCharacterPNG;
        }

        public Dictionary<string, SkiaSharp.SKBitmap> GetMonsterPNG(string gameLocation)
        {
            var monsterPNG = new Dictionary<string, SkiaSharp.SKBitmap>();
            string[] allpngfiles =
                Directory.GetFiles(gameLocation + @"/rltiles/mon", "*.png*", SearchOption.AllDirectories);
            foreach (var file in allpngfiles)
            {
                FileInfo info = new FileInfo(file);
                SkiaSharp.SKBitmap SKBitmap = SkiaSharp.SKBitmap.Decode(file);
                monsterPNG[info.Name.Replace(".png", "")] = SKBitmap;
            }

            return monsterPNG;
        }

        public Dictionary<string, SkiaSharp.SKBitmap> GetWeaponPNG(string gameLocation)
        {
            var GetWeaponPNG = new Dictionary<string, SkiaSharp.SKBitmap>();

            List<string> allpngfiles = Directory
                .GetFiles(gameLocation + @"/rltiles/player/hand1", "*.png*", SearchOption.AllDirectories).ToList();
            allpngfiles.AddRange(Directory.GetFiles(gameLocation + @"/rltiles/player/transform", "*.png*",
                SearchOption.AllDirectories).ToList());
            foreach (var file in allpngfiles)
            {
                FileInfo info = new FileInfo(file);
                SkiaSharp.SKBitmap SKBitmap = SKBitmap.Decode(file);


                GetWeaponPNG[info.Name.Replace(".png", "")] = SKBitmap;
            }

            return GetWeaponPNG;
        }
    }
}
