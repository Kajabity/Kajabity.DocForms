using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Kajabity.DocForms.Forms;

namespace HexViewer
{
    // Storage belongs to the sample. The KDF library never chooses an application settings format.
    internal static class WindowPlacementSettings
    {
        private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(WindowPlacement));

        private static string Filename => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Kajabity", "HexViewer", "window-placement.xml");

        internal static WindowPlacement Load()
        {
            try
            {
                using (var stream = File.OpenRead(Filename))
                using (var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null }))
                    return (WindowPlacement)Serializer.Deserialize(reader);
            }
            catch (IOException) { return null; }
            catch (UnauthorizedAccessException) { return null; }
            catch (InvalidOperationException) { return null; }
            catch (XmlException) { return null; }
        }

        internal static void Save(WindowPlacement placement)
        {
            if (placement == null)
                return;

            string temporary = null;
            try
            {
                string filename = Filename;
                Directory.CreateDirectory(Path.GetDirectoryName(filename));
                temporary = filename + "." + Guid.NewGuid().ToString("N") + ".tmp";
                using (var stream = File.Create(temporary))
                    Serializer.Serialize(stream, placement);
                if (File.Exists(filename))
                    File.Replace(temporary, filename, null);
                else
                    File.Move(temporary, filename);
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            finally
            {
                if (temporary != null)
                {
                    try { File.Delete(temporary); }
                    catch (IOException) { }
                    catch (UnauthorizedAccessException) { }
                }
            }
        }
    }
}
