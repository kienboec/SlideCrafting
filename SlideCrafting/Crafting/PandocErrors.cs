using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlideCrafting.Crafting
{
    // https://pandoc.org/MANUAL.html#exit-codes
    public enum PandocErrors
    {
        OK = 0,
        PandocFailOnWarningError = 3,
        PandocAppError = 4,
        PandocTemplateError = 5,
        PandocOptionError = 6,
        PandocUnknownReaderError = 21,
        PandocUnknownWriterError = 22,
        PandocUnsupportedExtensionError = 23,
        PandocEpubSubdirectoryError = 31,
        PandocPDFError = 43,
        PandocPDFProgramNotFoundError = 47,
        PandocHttpError = 61,
        PandocShouldNeverHappenError = 62,
        PandocSomeError = 63,
        PandocParseError = 64,
        PandocParsecError = 65,
        PandocMakePDFError = 66,
        PandocSyntaxMapError = 67,
        PandocFilterError = 83,
        PandocMacroLoop = 91,
        PandocUTF8DecodingError = 92,
        PandocIpynbDecodingError = 93,
        PandocCouldNotFindDataFileError = 97,
        PandocResourceNotFound = 99,
    }
}
