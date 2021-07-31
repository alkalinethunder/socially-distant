using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SociallyDistant.Core;
using StbImageSharp;
using Thundershock.Content;
using Thundershock.Core;
using Thundershock.IO;
using Action = Gtk.Action;

namespace SociallyDistant.ContentEditor
{
    public class PakWorker
    {
        private IContentEditor _editor;
        private FileSystem _fs;
        private AssetRegistry _registry;
        private string _out;
        private string _work;
        private FileSystem _workfs;
        private Dictionary<string, string> _textures = new();
        
        private ConcurrentProperty<PakWorkerProgress> _progress;

        public PakWorkerProgress Progress => _progress.Value;
        
        public PakWorker(FileSystem fs, AssetRegistry registry, string path, IContentEditor editor)
        {
            _fs = fs;
            _out = path;
            _registry = registry;
            _editor = editor;

            _progress = new(new PakWorkerProgress());
        }

        public event EventHandler Finished;

        public async Task PackageAsync()
        {
            // start by creating a working directory.
            await Task.Run(() =>
            {
                _progress.Value.Status = "Creating working directory...";

                var work = _out + ".work";
                if (Directory.Exists(work))
                {
                    throw new InvalidOperationException(
                        "The working directory for the project build operation already exists.");
                }

                _work = work;

                Directory.CreateDirectory(work);

                _work = work;
                _workfs = FileSystem.FromHostDirectory(_work);

                try
                {
                    _progress.Value.Status = "Copying textures...";
                    CopyTextures();

                    _progress.Value.Status = "Saving assets...";
                    CopyAssets();

                    _progress.Value.Status = "Running ThunderPak...";
                    PakUtils.MakePak(_work, _out);
                }
                catch (Exception ex)
                {
                    _editor.Error("An error has occurred packaging the project:" + Environment.NewLine +
                                  Environment.NewLine + ex.ToString());
                }

                // delete the working directory.
                if (Directory.Exists(_work))
                {
                    Directory.Delete(_work, true);
                    _work = null;
                }

                EntryPoint.CurrentApp.EnqueueAction(() => { Finished?.Invoke(this, EventArgs.Empty); });
            }).ConfigureAwait(false);
        }

        private void CopyTextures()
        {
            var textureCount = 0;
            if (_fs.DirectoryExists("/Images"))
            {
                foreach (var file in _fs.GetFiles("/Images"))
                {
                    textureCount++;

                    var mapped = "/img/" + textureCount;

                    _textures.Add(file, mapped);
                }
            }

            _workfs.CreateDirectory("/img");

            var c = 0;
            foreach (var tex in _textures.Keys)
            {
                _progress.Value.Status = "Copying textures... (" + c + " / " + textureCount + ")...";
                var texOut = _textures[tex];

                CopyTexture(tex, texOut);
                c++; // HA.
            }
        }

        private void CopyTexture(string src, string dest)
        {
            // Open the source and destination files.
            using var sourceStream = _fs.OpenFile(src);
            using var destStream = _workfs.OpenFile(dest);

            // Open a binary writer for the dest file
            using var writer = new BinaryWriter(destStream, Encoding.UTF8);

            // Now for the cpu-intensive part.
            var bytes = Array.Empty<byte>();
            var width = 0;
            var height = 0;

            // Open a GDI+ image for the source.
            using var bmp = (Bitmap) Image.FromStream(sourceStream);

            // store width and height for later
            width = bmp.Width;
            height = bmp.Height;

            // Lock the bitmap.
            var lck = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            // Calculate byte size and resize buffer to fit.
            var size = Math.Abs(lck.Stride) * lck.Height;
            Array.Resize(ref bytes, size);

            // Copy the data from gdi+ over to the land of C#.
            Marshal.Copy(lck.Scan0, bytes, 0, bytes.Length);

            // Unlock the bits.
            bmp.UnlockBits(lck);
            lck = null;

            // Now we need to swap the red and blue channels - thundershock does this on its own with
            // Texture2D.FromFile, but we don't get that luxury when dealing with pak data.
            //
            // In all honesty, that's a good thing. If it's in a pak, the  engine should assume it's in
            // an optimal format.
            for (var i = 0; i < bytes.Length; i += 4)
            {
                _progress.Value.Percentage = i / (float) bytes.Length;

                var r = bytes[i];
                var b = bytes[i + 2];

                bytes[i] = b;
                bytes[i + 2] = r;
            }

            // So now the annoying part's done, just gotta write the data to the pak asset.
            // Start with the width and height.
            writer.Write(width);
            writer.Write(height);
            
            // write the bytes  per pixel count.
            writer.Write((byte) 4);
            
            // Write all of the pixels now.
            writer.Write(bytes);
            
            // We're done.
            writer.Close();
            sourceStream.Close();
            destStream.Close();
        }
        
        private void CopyAssets()
        {
            _workfs.CreateDirectory("/asset");

            using var assetDatabase = _workfs.OpenFile("/assets.map");
            
            // Write the glorious header ID of 80710a06.
            assetDatabase.Write(AssetRegistry.AssetMapHeaderId, 0, AssetRegistry.AssetMapHeaderId.Length);
            
            // actual data storage.
            using var binWriter = new BinaryWriter(assetDatabase, Encoding.UTF8);

            // Retrieve all assets in the entire game.
            var assets = _registry.GetAssets().ToArray();
            
            // Progress update.
            _progress.Value.Status = $"Saving assets (0 / {assets.Length})";
            _progress.Value.Percentage = 0;
            
            // Write the asset count to the asset map.
            binWriter.Write(assets.Length);
            
            // We'll need this for signing assets.
            using var sha256 = SHA256.Create();
            
            // Dictionary of image paths after translation to image paths before translation.
            var imageTranslations = new Dictionary<string, string>();
           
            // Iterate through each asset.
            var done = 0;
            foreach (var asset in assets)
            {
                var assetType = asset.GetType();
                var path = "/asset/" + assetType.Name + "_" + asset.Id.ToString();

                // Write the asset information.
                binWriter.Write(asset.Id.ToString());   // So we can load by  ID.
                binWriter.Write(asset.Name);            // So we know what the asset's called.
                binWriter.Write(assetType.FullName);    // All assets implement IAsset, but we need to be able to deserialize the right type when loading.
                binWriter.Write(path);                  // Where the frack is it stored?
                
                // Encode all of this information as a sha256 hash for validation/integrity.
                var infoCipher = Encoding.UTF8.GetBytes(asset.Id.ToString() + asset.Name + assetType.FullName + path);
                var infoHash = sha256.ComputeHash(infoCipher);
                
                // Write the info signature.
                binWriter.Write(infoHash.Length);
                binWriter.Write(infoHash);
                
                // Serialize the data as json.
                var json = JsonSerializer.Serialize(asset, assetType);
                
                // Replace any image reference paths.
                foreach (var image in _textures.Keys)
                    json = json.Replace(image, _textures[image]);
                
                // Write the data to the pak file.
                _workfs.WriteAllText(path, json);
                
                // Hash it.
                var assetHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(json));
                
                // Write the hash.
                binWriter.Write(assetHash.Length);
                binWriter.Write(assetHash);

                // Update progress.
                done++;
                _progress.Value.Percentage = (float) done / assets.Length;
                _progress.Value.Status = $"Saving assets ({done} / {assets.Length})";
            }
            
            // We're done.
            binWriter.Close();
            assetDatabase.Close();
        }
    }

    public class PakWorkerProgress
    {
        public float Percentage { get; set; }
        public string Status { get; set; }
        
        
    }
}