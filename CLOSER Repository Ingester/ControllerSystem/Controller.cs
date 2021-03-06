﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CLOSER_Repository_Ingester.ControllerSystem.Actions;

namespace CLOSER_Repository_Ingester.ControllerSystem
{
    class Controller
    {
        string filepath;
        public string basePath { get; set; }
        public List<Group> groups { get; private set; }
        public List<IAction> globalActions { get; private set; }

        public Controller(string filepath)
        {
            this.filepath = filepath;
            groups = new List<Group>();
            globalActions = new List<IAction>();
        }

        public void loadFile()
        {
            loadFile(filepath);
        }

        public void loadFile(string filepath)
        {
            Console.WriteLine("Started loading {0}", filepath);
            var lines = File.ReadAllLines(filepath);
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.Length == 0) continue;
                if (trimmedLine[0] == '#') continue;
                parseLine(trimmedLine);
                Console.WriteLine("Parsed lined: {0}", trimmedLine);
            }
            Console.WriteLine("Finished loading {0}", filepath);
        }

        private void parseLine(string line)
        {
            var pieces = line.Split(new char[] { '\t' });
            Group group;

            if (pieces.Length > 1)
            {
                switch (pieces[0].ToLower())
                {
                    case "group":
                        groups.Add(new Group(pieces[1]));
                        return;

                    case "rootdir":
                        basePath = pieces[1];
                        return;

                    case "control":
                        loadFile(Path.Combine(Path.GetDirectoryName(filepath), pieces[1]));
                        return;

                    case "concepts":
                        globalActions.Add(new LoadTopics(BuildFilePath(pieces[1])));
                        return;

                    default:
                        group = groups.Find(x => string.Compare(x.name, pieces[1]) == 0);
                        break;
                }
                if (group != null)
                {
                    switch (pieces[0].ToLower())
                    {
                        case "instrument":
                            if (pieces.Length > 4)
                            {
                                group.AddAction(pieces[2], new LoadInstrument(BuildFilePath(pieces[3]), pieces[4]));
                            }
                            else
                            {
                                group.AddAction(pieces[2], new LoadInstrument(BuildFilePath(pieces[3])));
                            }
                            break;

                        case "studysweep":
                            group.AddAction(new LoadStudySweep(BuildFilePath(pieces[2])));
                            break;

                        case "dataset":
                            if (pieces.Length > 3)
                            {
                                group.AddAction(pieces[2], new LoadDataset(BuildFilePath(pieces[3])));
                            }
                            else
                            {
                                group.AddAction(new LoadDataset(BuildFilePath(pieces[2])));
                            }
                            break;

                        case "qvmapping":
                            if (pieces.Length > 3)
                            {
                                group.AddAction(pieces[2], new LoadQVMapping(BuildFilePath(pieces[3])));
                            }
                            else
                            {
                                group.AddAction(new LoadQVMapping(BuildFilePath(pieces[2])));
                            }
                            break;

                        case "dvmapping":
                            group.AddAction(pieces[2], new LoadDVMapping(BuildFilePath(pieces[3])));
                            break;

                        case "tqlinking":
                            group.AddAction(pieces[2], new LoadTQLinking(BuildFilePath(pieces[3])));
                            break;

                        case "tvlinking":
                            if (pieces.Length > 3)
                            {
                                group.AddAction(pieces[2], new LoadTVLinking(BuildFilePath(pieces[3])));
                            }
                            else
                            {
                                group.AddAction(new LoadTVLinking(BuildFilePath(pieces[2])));
                            }
                            break;

                        case "images":
                            string dirName, dirPath, ccsName;
                            if (pieces.Length > 4)
                            {
                                dirName = pieces[4];
                                dirPath = BuildFilePath(pieces[4]);
                                ccsName = pieces[3];
                            }
                            else
                            {
                                dirName = pieces[3];
                                dirPath = BuildFilePath(pieces[3]);
                                ccsName = pieces[2] + "_ccs01";
                            }
                            group.AddAction(pieces[2], new AttachExternalAids(ccsName, dirName, dirPath));
                            break;

                        case "pdf":
                            group.AddAction(pieces[2], new AttachExternalInstrument(pieces[3], BuildFilePath(pieces[4])));
                            break;
                    }
                }
            }
        }

        private string BuildFilePath(string filepath)
        {
            var output = Path.Combine(basePath, filepath);

            output = output.Replace('/', Path.DirectorySeparatorChar);
            output = output.Replace('\\', Path.DirectorySeparatorChar);

            return output;
        }
    }
}
