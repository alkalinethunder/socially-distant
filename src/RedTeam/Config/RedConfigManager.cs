using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Thundershock;
using Thundershock.IO;

namespace RedTeam.Config
{
    public class RedConfigManager : GlobalComponent
    {
        private FileSystem _fs;
        private RedConfig _config;

        public RedConfig ActiveConfig
            => _config;

        public IEnumerable<RedTermPalette> GetPalettes()
        {
            // TODO
            yield break;
        }
        
        public void ApplyChanges()
        {
            // TODO
        }

        public void DiscardChanges()
        {
            // TODO
        }

        protected override void OnLoad()
        {
            _fs = FileSystem.FromHostDirectory(ThundershockPlatform.LocalDataPath);
            
            // redterm palettes
            if (!_fs.DirectoryExists("/palettes"))
                _fs.CreateDirectory("/palettes");

            // write default palettes if they don't exist.
            WritePalette("default");
            WritePalette("light");
            WritePalette("highContrast");

            // palettes readme
            WriteReadme();

            
            base.OnLoad();
        }
        
        private void WritePalette(string resourceName)
        {
            var resource = this.GetType().Assembly.GetManifestResourceStream("RedTeam.Resources.RedTermPalettes." + resourceName + ".json");
            using var reader = new StreamReader(resource, Encoding.UTF8, true);

            var json = reader.ReadToEnd();

            _fs.WriteAllText($"/palettes/" + resourceName + ".json", json);
        }

        private void WriteReadme()
        {
            var readme = this.GetType().Assembly
                .GetManifestResourceStream("RedTeam.Resources.RedTermPalettes.README.txt");
            using var reader = new StreamReader(readme, Encoding.UTF8, true);
            var readmeText = reader.ReadToEnd();

            _fs.WriteAllText("/palettes/README.txt", readmeText);
        }

    }
}