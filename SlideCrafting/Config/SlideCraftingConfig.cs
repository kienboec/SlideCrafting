using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Microsoft.Extensions.Options;

namespace SlideCrafting.Config
{
    // https://docs.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-5.0#alternatives-to-built-in-attributes
    public class SlideCraftingConfig 
    {
        public int AttemptsOnError { get; set; }


        public string IndexFilesExtension { get; set; }
        public string ListFilesExtension { get; set; }


        public string YamlKeyInputFiles { get; set; }
        public string YamlKeyExerciseFiles { get; set; }


        public string PandocApp { get; set; }
        public string PdfEngine { get; set; }

        public bool StartWebServer { get; set; }

        public string OriginFolder { get; set; }
        public string WorkFolder { get; set; }
        public string DistFolder { get; set; }
        public string ArchiveFolder { get; set; }
        public string PandocLogFile { get; set; }
        public string DocxReference { get; set; }
        public string PptxReference { get; set; }
        public string BeamerTemplateFolder { get; set; }

        public bool GenerationFlagBeamer => Environment.GetEnvironmentVariable("GEN_BEAMER") == "1";
        public bool GenerationFlagBeamerNotes => Environment.GetEnvironmentVariable("GEN_BEAMER_NOTES") == "1";
        public bool GenerationFlagHandout => Environment.GetEnvironmentVariable("GEN_HANDOUT") == "1";
        public bool GenerationFlagPowerPoint => Environment.GetEnvironmentVariable("GEN_PPTX") == "1";
        public bool GenerationFlagRevealJS => Environment.GetEnvironmentVariable("GEN_REVEALJS") == "1";
        public bool GenerationFlagExercise => Environment.GetEnvironmentVariable("GEN_EXERCISES") == "1";
        public bool GenerationFlagExerciseDocx => Environment.GetEnvironmentVariable("GEN_EXERCISES_DOCX") == "1";
        public bool FilterFlagPlantUml => Environment.GetEnvironmentVariable("FILTER_PLANTUML") == "1";
    }
}
