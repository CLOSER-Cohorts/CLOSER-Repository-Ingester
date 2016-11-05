﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Linq;
using Algenta.Colectica.Model;
using Algenta.Colectica.Model.Ddi;
using Algenta.Colectica.Model.Ddi.Serialization;

namespace CLOSER_Repository_Ingester.ControllerSystem.Actions
{
    class LoadInstrument : DDIFileAction
    {
        string filepath;

        public LoadInstrument(string scope, string filepath)
        {
            this.scope = scope;
            this.filepath = filepath;
        }

        public override void Validate()
        {
            if (!System.IO.File.Exists(this.filepath))
            {
                throw new System.Exception("Missing file: " + this.filepath);
            }

            DdiValidator validator = new DdiValidator(this.filepath, DdiFileFormat.Ddi32);
            if (validator.Validate())
            {
                this.doc = validator.ValidatedXDocument;
            } 
            else
            {
                throw new System.Exception("Invalid file: " + this.filepath);
            }
        }

        public override IEnumerable<IVersionable> Build()
        {
            Collection<IVersionable> allItems = getAllItems();

            var questionSchemes = allItems.OfType<QuestionScheme>();
            foreach (var questionScheme in questionSchemes)
            {
                foreach (Question qi in questionScheme.Questions)
                {
                    var p = new Parameter();
                    p.ParameterType = InstrumentParameterType.Out;
                    p.Name.Add("en-GB", "p_" + qi.ItemName.Best);
                    qi.OutParameters.Add(p);
                }
            }

            //I assume that there are the corresponding caddies question constructs
            //add an InParameter based on the first (the only) OutParameter of its QuestionItem,
            //an OutputParameter based on the construct name
            //and a binding between the two
            foreach (QuestionActivity qc in allItems.OfType<QuestionActivity>())
            {
                if (qc.Question != null)
                {
                    Parameter p = new Parameter();
                    p.ParameterType = InstrumentParameterType.In;
                    p.Name.Add("en-GB", qc.Question.OutParameters.First().Name.Best);
                    qc.InParameters.Add(p);
                    Parameter p2 = new Parameter();
                    p2.ParameterType = InstrumentParameterType.Out;
                    p2.Name.Add("en-GB", "p_" + qc.ItemName.Best);
                    qc.OutParameters.Add(p2);
                }
                else if (qc.QuestionGrid != null)
                {
                    continue;
                }
                else
                {
                    //Trace.WriteLine("   question construct with no source");
                }
            }

            return allItems;
        }
    }
}
