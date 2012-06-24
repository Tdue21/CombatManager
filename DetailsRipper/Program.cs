/*
 *  Program.cs
 *
 *  Copyright (C) 2010-2012 Kyle Olson, kyle@kyleolson.com
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public License
 *  as published by the Free Software Foundation; either version 2
 *  of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 * 
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 *
 */

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Data.Sqlite;
using System.IO;
using System.Xml.Linq;
using CombatManager;

namespace DetailsRipper
{
    class Program
    {
        static void Main(string[] args)
        {
            if (File.Exists("Details.db"))
            {
                File.Delete("Details.db");
            }

            SqliteConnection cn = new SqliteConnection("Data Source=Details.db");
            cn.Open();
			using (SqliteCommand v = cn.CreateCommand())
			{
				v.CommandText = "Create Table Rules (ID integer primary key, details string)";
				v.ExecuteNonQuery();
			}

            XDocument docRules = XDocument.Load("Rule.xml");

            var vals = docRules.Descendants("Rule");
            var t = cn.BeginTransaction();
           
            foreach (XElement x in vals)
            {
				using (SqliteCommand cm = cn.CreateCommand())
				{
					cm.CommandText = "Insert into Rules (ID, details) values (?, ?)";
					var p1 = cm.CreateParameter();
					p1.Value = x.ElementValue("ID");
					cm.Parameters.Add(p1);
					var p2 = cm.CreateParameter();
					p2.Value = x.ElementValue("Details");
					cm.Parameters.Add(p2);
					cm.ExecuteNonQuery();
				}
                x.Element("Details").Remove();
            }

            t.Commit();
           


            cn.Close();

            docRules.Save("RuleShort.xml");

            XDocument docMagic = XDocument.Load("MagicItems.xml");

            foreach (XElement x in docMagic.Descendants("MagicItem"))
            {
                if (x.Element("DescHTML") != null)
                {
                    x.Element("DescHTML").Remove();
                }
            }

            docMagic.Save("MagicItemsShort.xml");


            XDocument docSpells = XDocument.Load("Spells.xml");

            foreach (XElement x in docSpells.Descendants("Spell"))
            {
                if (x.Element("full_text") != null)
                {
                    x.Element("full_text").Remove();
                }
            }

            docSpells.Save("Spells.xml");


            Dictionary<string, XElement> monsterList = new Dictionary<string, XElement>();

            XDocument docMon = XDocument.Load("Bestiary.xml");

            foreach (XElement x in docMon.Descendants("Monster"))
            {
                if (x.Element("FullText") != null)
                {
                    x.Element("FullText").Remove();
                }
                XElement oldElement;
                string monName = x.Element("Name").Value;
                if (monsterList.TryGetValue(monName, out oldElement))
                {
                    string oldSource = oldElement.ElementValue("Source");
                    string newSource = x.ElementValue("Source");

                    //System.Diagnostics.Debug.WriteLine(x.Element("Name").Value);
                    if (newSource == "Bestiary 3")
                    {
                        oldElement.Remove();
                        //System.Diagnostics.Debug.WriteLine("Remove " + oldSource);
                        monsterList[monName] = x;
                    }
                    else if (oldSource == "Bestiary 3")
                    {
                        x.Remove();
                        //System.Diagnostics.Debug.WriteLine("Remove " + newSource);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("No replace: " + x.Element("Name").Value);
                    }

                }
                else
                {
                    monsterList[monName] = x;
                }
            }

            docMon.Save("BestiaryShort.xml");

            docMon = XDocument.Load("NPC.xml");

            int dupcount = 0;
            List<string> dupNames = new List<string>();
            List<XElement> removeList = new List<XElement>();
            foreach (XElement x in docMon.Descendants("Monster"))
            {
                if (x.Element("FullText") != null)
                {
                    x.Element("FullText").Remove();
                }

                XElement oldElement;
                string monName = x.Element("Name").Value;
                if (monsterList.TryGetValue(monName, out oldElement))
                {
                    dupcount++;
                    dupNames.Add(monName);


                    string oldSource = oldElement.ElementValue("Source");
                    string newSource = x.ElementValue("Source");

                    if (oldSource == "Bestiary 3")
                    {
                        removeList.Add(x);
                        System.Diagnostics.Debug.WriteLine("Remove " + monName);
                    }
                }
                else
                {
                    monsterList[monName] = x;
                }
            }
            foreach (XElement x in removeList)
            {
                x.Remove();
            }
            dupNames.Sort();
            foreach (string dupName in dupNames)
            {

                System.Diagnostics.Debug.WriteLine("Dup NPC: " + dupName);
            }
            System.Diagnostics.Debug.WriteLine("Dup Count: " + dupcount);

            docMon.Save("NPCShort.xml");




        }


    }
    public static class XElementExt
    {

        public static string ElementValue(this XElement x, string name)
        {
            return x.Element(name) == null ? null : x.Element(name).Value;
        }
    }
}

